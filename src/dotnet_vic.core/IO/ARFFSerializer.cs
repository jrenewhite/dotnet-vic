using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using dotnet_vic.core.Model;

namespace dotnet_vic.core.IO
{
    internal enum TokenType
    {
        Unquoted,
        Quoted,
        EndOfLine,
        EndOfFile
    }

    /// <summary>
    /// A serializer for ARFF files
    /// </summary>
    public class ARFFSerializer : DatasetSerializer
    {
        /// <summary>
        /// Creates an instance of <see cref="ARFFSerializer"/>
        /// </summary>
        /// <param name="path">Path of the file to serialize/deserialize</param>
        public ARFFSerializer(string path) : base(path)
        {
            if (!Path.GetExtension(FilePath).Equals(".arff", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Wrong file extension. Was expecting '*.arff'.", "Path");
            }
        }

        /// <summary>
        /// Obtains a definition for a <see cref="Dataset"/> from an ARFF file
        /// </summary>
        /// <returns>A <see cref="Dataset"/> definition (may include all <see cref="Instance"/> elements)</returns>
        public override Dataset DeserializeDataset()
        {
            if (ReadFromPath())
            {
                using ARFFReader reader = new ARFFReader(Content);
                return reader.ReadDataset();
            }
            else
            {
                return new Dataset(new Header("Unknown", new Feature[0]), new Instance[0]);
            }
        }

        /// <summary>
        /// Obtains a definition for a <see cref="Dataset"/> from an ARFF file asynchronously
        /// </summary>
        /// <returns>A <see cref="Dataset"/> definition (may include all <see cref="Instance"/> elements)</returns>
        public override async Task<Dataset> DeserializeDatasetAsync()
        {
            if (await ReadFromPathAsync())
            {
                using ARFFReader reader = new ARFFReader(Content);
                return reader.ReadDataset();
            }
            else
            {
                return new Dataset(new Header("Unknown", new Feature[0]), new Instance[0]);
            }
        }

        /// <summary>
        /// Obtains all instances available in the ARFF file
        /// </summary>
        /// <returns>An enumeration of all <see cref="Instance"/> elements</returns>
        public override IEnumerable<Instance> DeserializeInstances()
        {
            if (ReadFromPath())
            {
                using ARFFReader reader = new ARFFReader(Content);
                return reader.ReadAllInstances();
            }
            else
            {
                return new Instance[0];
            }
        }

        /// <summary>
        /// Obtains all instances available in the ARFF file asynchronously
        /// </summary>
        /// <returns>An enumeration of all <see cref="Instance"/> elements</returns>
        public override async Task<IEnumerable<Instance>> DeserializeInstancesAsync()
        {
            if (await ReadFromPathAsync())
            {
                using ARFFReader reader = new ARFFReader(Content);
                return reader.ReadAllInstances();
            }
            else
            {
                return new Instance[0];
            }
        }

        /// <summary>
        /// Writes a <see cref="Dataset"/> into an ARFF file
        /// </summary>
        /// <param name="dataset">A <see cref="Dataset"/> definition that my contain any number of <see cref="Instance"/> elements</param>
        public override void SerializeDataset(Dataset dataset)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a <see cref="Dataset"/> into an ARFF file asynchronously
        /// </summary>
        /// <param name="dataset">A <see cref="Dataset"/> definition that my contain any number of <see cref="Instance"/> elements</param>
        /// <returns>A <see cref="Task"/> operation result</returns>
        public override Task SerializeDatasetAsync(Dataset dataset)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes all <see cref="Instance"/> elements into the ARFF file
        /// </summary>
        /// <param name="instances">An enumeration of <see cref="Instance"/> elements</param>
        public override void SerializeInstances(IEnumerable<Instance> instances)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes all <see cref="Instance"/> elements into the ARFF file asynchronously
        /// </summary>
        /// <param name="instances">An enumeration of <see cref="Instance"/> elements</param>
        /// <returns>A <see cref="Task"/> operation result</returns>
        public override Task SerializeInstancesAsync(IEnumerable<Instance> instances)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An ARFF file reader
    /// </summary>
    public class ARFFReader : IDisposable
    {
        StreamReader streamReader;

        Header Header;

        Instance[] Instances;

        int unprocessedChar = -1;

        bool disposed = false;

        /// <summary>
        /// Initializes a new <see cref="ARFFReader"/> instance that reads from the specified stream using UTF-8 encoding.
        /// </summary>
        /// <param name="stream">The underlying stream that the <see cref="ARFFReader"/> should read from.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public ARFFReader(byte[] bytes) : this(new MemoryStream(bytes))
        {

        }
        /// <summary>
        /// Initializes a new <see cref="ARFFReader"/> instance that reads from the specified stream using UTF-8 encoding.
        /// </summary>
        /// <param name="stream">The underlying stream that the <see cref="ARFFReader"/> should read from.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public ARFFReader(byte[] bytes, Encoding encoding) : this(new MemoryStream(bytes), encoding)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="ARFFReader"/> instance that reads from the specified stream using UTF-8 encoding.
        /// </summary>
        /// <param name="stream">The underlying stream that the <see cref="ARFFReader"/> should read from.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public ARFFReader(Stream stream)
        {
            streamReader = new StreamReader(stream);
        }

        /// <summary>
        /// Initializes a new <see cref="ARFFReader"/> instance that reads from the specified stream using the specified encoding.
        /// </summary>
        /// <param name="stream">The underlying stream that the <see cref="ARFFReader"/> should read from.</param>
        /// <param name="encoding">The character encoding that should be used.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public ARFFReader(Stream stream, Encoding encoding)
        {
            streamReader = new StreamReader(stream, encoding);
        }

        /// <summary>
        /// Initializes a new <see cref="ARFFReader"/> instance that reads from the specified file path using UTF-8 encoding.
        /// </summary>
        /// <param name="path">The file path that the <see cref="ARFFReader"/> should read from.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        public ARFFReader(string path)
        {
            streamReader = new StreamReader(path);
        }

        /// <summary>
        /// Initializes a new <see cref="ARFFReader"/> instance that reads from the specified file path using the specified encoding.
        /// </summary>
        /// <param name="path">The file path that the <see cref="ARFFReader"/> should read from.</param>
        /// <param name="encoding">The character encoding that should be used.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        public ARFFReader(string path, Encoding encoding)
        {
            streamReader = new StreamReader(path, encoding);
        }

        private char UnescapeChar(char c)
        {
            switch (c)
            {
                case '"':
                case '\'':
                case '%':
                case '\\':
                default:
                    return c;
                case 'r':
                    return '\r';
                case 'n':
                    return '\n';
                case 't':
                    return '\t';
                case 'u': // the only universal character name supported is \u001E
                    return '\u001e';
            }
        }

        private string ReadToken(out TokenType tokenType, TextReader textReader)
        {
            int c;

            // if the last character read hasn't been processed yet, do so now
            if (unprocessedChar == -1)
                c = textReader.Read();
            else
            {
                c = unprocessedChar;
                unprocessedChar = -1;
            }

            // skip whitespace (except line terminators)
            while (c != '\r' && c != '\n' && c != -1 && char.IsWhiteSpace((char)c))
                c = textReader.Read();

            int quoteChar = -1;

            switch (c)
            {
                case -1:
                    tokenType = TokenType.EndOfFile;
                    return null;
                case '\r':
                    if (textReader.Peek() == '\n')
                        textReader.Read();
                    tokenType = TokenType.EndOfLine;
                    return null;
                case '\n':
                    tokenType = TokenType.EndOfLine;
                    return null;
                case '%': // skip comment and return end-of-line
                    do
                    {
                        c = textReader.Read();
                    } while (c != '\r' && c != '\n' && c != -1);
                    if (c == '\r' && textReader.Peek() == '\n')
                        textReader.Read();
                    tokenType = TokenType.EndOfLine;
                    return null;
                case '\'':
                case '\"':
                    quoteChar = c;
                    c = textReader.Read();
                    if (c == -1)
                        throw new InvalidDataException("Unexpected end-of-line. Expected closing quotation mark.");
                    break;
                case ',':
                case '{':
                case '}':
                    tokenType = TokenType.Unquoted;
                    return Convert.ToString((char)c);
            }

            StringBuilder token = new StringBuilder();

            while (true)
            {
                if (quoteChar == -1)
                    token.Append((char)c);
                else
                {
                    if (c == quoteChar)
                        break;
                    else if (c == '\\')
                    {
                        c = textReader.Read();

                        if (c == -1)
                            throw new InvalidDataException($"Unexpected end-of-file.");

                        // the only universal character name supported is \u001E
                        if (c == 'u')
                            if (textReader.Read() != '0' ||
                                textReader.Read() != '0' ||
                                textReader.Read() != '1' ||
                                textReader.Read() != 'E')
                                throw new InvalidDataException($"Unsupported universal character name.");

                        token.Append(UnescapeChar((char)c));
                    }
                    else
                        token.Append((char)c);
                }

                c = textReader.Read();

                if (c == -1)
                {
                    if (quoteChar != -1)
                        throw new InvalidDataException("Unexpected end-of-file. Expected closing quotation mark.");

                    break;
                }
                else if (c == '\r' || c == '\n')
                {
                    if (quoteChar != -1)
                        throw new InvalidDataException("Unexpected end-of-line. Expected closing quotation mark.");

                    unprocessedChar = c;
                    break;
                }
                else if (quoteChar == -1 && (c == ',' || c == '{' || c == '}' || c == '%' || char.IsWhiteSpace((char)c)))
                {
                    unprocessedChar = c;
                    break;
                }
            }

            tokenType = quoteChar != -1 ? TokenType.Quoted : TokenType.Unquoted;

            return token.ToString();
        }

        private string ReadToken(out bool quoting, string expectedToken = null, bool ignoreCase = false, bool skipEndOfLine = false, bool? endOfLine = null, TextReader textReader = null)
        {
            if (textReader == null)
                textReader = streamReader;

            string token;
            TokenType tokenType;

            do
            {
                token = ReadToken(out tokenType, textReader);
            } while (skipEndOfLine && tokenType == TokenType.EndOfLine);

            if (endOfLine != null)
                if (endOfLine == true && token != null)
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected end-of-line.");
                else if (endOfLine == false && token == null)
                    if (expectedToken == null)
                        throw new InvalidDataException($"Unexpected end-of-line. Expected value.");
                    else
                        throw new InvalidDataException($"Unexpected end-of-line. Expected token \"{expectedToken}\".");

            if (expectedToken != null)
                if (string.Compare(token, expectedToken, ignoreCase, CultureInfo.InvariantCulture) != 0)
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected \"{expectedToken}\".");

            quoting = tokenType == TokenType.Quoted;

            return token;
        }

        private string ReadToken(string expectedToken = null, bool ignoreCase = false, bool skipEndOfLine = false, bool? endOfLine = null, bool? quoting = null, TextReader textReader = null)
        {
            if (textReader == null)
                textReader = streamReader;

            string token;
            TokenType tokenType;

            do
            {
                token = ReadToken(out tokenType, textReader);
            } while (skipEndOfLine && tokenType == TokenType.EndOfLine);

            if (endOfLine != null)
                if (endOfLine == true && token != null)
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected end-of-line.");
                else if (endOfLine == false && token == null)
                    if (expectedToken == null)
                        throw new InvalidDataException($"Unexpected end-of-line. Expected value.");
                    else
                        throw new InvalidDataException($"Unexpected end-of-line. Expected token \"{expectedToken}\".");

            if (expectedToken != null)
                if (token == null)
                    throw new InvalidDataException($"Unexpected end-of-line. Expected token \"{expectedToken}\".");
                else if (string.Compare(token, expectedToken, ignoreCase, CultureInfo.InvariantCulture) != 0)
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected \"{expectedToken}\".");

            if (quoting != null)
                if (quoting.Value != (tokenType == TokenType.Quoted))
                    if (token != null)
                        throw new InvalidDataException($"Incorrect quoting for token \"{token}\".");
                    else
                        throw new InvalidDataException($"Unexpected end-of-line. Expected value.");

            return token;
        }

        private Feature ReadAttribute()
        {
            string attributeName = ReadToken(endOfLine: false);
            string typeString = ReadToken(endOfLine: false, quoting: false);

            FeatureType attributeType;

            if (string.Equals(typeString, "integer", StringComparison.OrdinalIgnoreCase))
            {
                attributeType = FeatureType.Integer;
                ReadToken(endOfLine: true);
            }
            else if (string.Equals(typeString, "numeric", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(typeString, "real", StringComparison.OrdinalIgnoreCase))
            {
                attributeType = FeatureType.Numeric;
                ReadToken(endOfLine: true);
            }
            else if (string.Equals(typeString, "string", StringComparison.OrdinalIgnoreCase))
            {
                attributeType = FeatureType.String;
                ReadToken(endOfLine: true);
            }
            else if (string.Equals(typeString, "date", StringComparison.OrdinalIgnoreCase))
            {
                string dateFormat = ReadToken();

                if (dateFormat == null)
                    attributeType = FeatureType.Date();
                else
                {
                    attributeType = FeatureType.Date(dateFormat);
                    ReadToken(endOfLine: true);
                }
            }
            else if (typeString == "{")
            {
                List<string> nominalValues = new List<string>();

                while (true)
                {
                    string value = ReadToken(out bool quoted, endOfLine: false);

                    if (!quoted && value == "}")
                        break;
                    else if (!quoted && value == ",")
                        continue;
                    else
                        nominalValues.Add(value);
                }

                attributeType = FeatureType.Nominal(nominalValues);
                ReadToken(endOfLine: true);
            }
            else if (string.Equals(typeString, "relational", StringComparison.OrdinalIgnoreCase))
            {
                ReadToken(endOfLine: true);

                List<Feature> childAttributes = new List<Feature>();

                while (true)
                {
                    string token = ReadToken(skipEndOfLine: true, endOfLine: false, quoting: false);

                    if (string.Equals(token, "@attribute", StringComparison.OrdinalIgnoreCase))
                    {
                        Feature attribute = ReadAttribute();

                        childAttributes.Add(attribute);
                    }
                    else if (string.Equals(token, "@end", StringComparison.OrdinalIgnoreCase))
                    {
                        ReadToken(expectedToken: attributeName, endOfLine: false);
                        ReadToken(endOfLine: true);
                        break;
                    }
                    else
                        throw new InvalidDataException($"Unexpected token \"{token}\". Expected \"@attribute\" or \"@end\".");
                }

                attributeType = FeatureType.Relational(childAttributes);
            }
            else
                throw new InvalidDataException($"Unexpected token \"{typeString}\". Expected attribute type.");

            return new Feature(attributeName, attributeType);
        }

        public Dataset ReadDataset()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            ReadHeader();
            ReadAllInstances();
            return new Dataset(Header, Instances);
        }

        /// <summary>
        /// Reads relation name and attribute declarations as an <see cref="Weka.Header"/> instance.
        /// </summary>
        /// <returns><see cref="Weka.Header"/> instance with read data.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="InvalidDataException"/>
        public Header ReadHeader()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (Header != null)
                throw new InvalidOperationException("The header has already been read by a previous call of ReadHeader.");

            List<Feature> attributes = new List<Feature>();

            ReadToken(expectedToken: "@relation", ignoreCase: true, skipEndOfLine: true, endOfLine: false, quoting: false);

            string relationName = ReadToken(endOfLine: false);

            ReadToken(endOfLine: true);

            while (true)
            {
                string token = ReadToken(skipEndOfLine: true, endOfLine: false, quoting: false);

                if (string.Equals(token, "@attribute", StringComparison.OrdinalIgnoreCase))
                {
                    Feature attribute = ReadAttribute();

                    attributes.Add(attribute);
                }
                else if (string.Equals(token, "@data", StringComparison.OrdinalIgnoreCase))
                {
                    ReadToken(endOfLine: true);
                    break;
                }
                else
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected \"@attribute\" or \"@data\".");
            }

            if (attributes.Count == 0)
                throw new InvalidDataException("Expected at least one \"@attribute\".");

            Header = new Header(relationName, attributes);

            return Header;
        }

        /// <summary>
        /// Reads data of a single instance. <c>null</c> is returned if the end-of-file is reached.
        /// </summary>
        /// <returns>The instance data as <see cref="object"/>[], or <c>null</c> if the end-of-file was reached.
        /// <para>The element types in the returned array depend on the type of their corresponding attribute:
        /// <see cref="double"/> (numeric attribute),
        /// <see cref="string"/> (string attribute),
        /// <see cref="int"/> (nominal attribute, index into nominal values array),
        /// <see cref="DateTime"/> (date attribute),
        /// <see cref="object"/>[][] (relational attribute).
        /// Missing values are represented as <c>null</c>.</para>
        /// </returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="InvalidDataException"/>
        public Instance ReadInstance()
        {
            return ReadInstance(out double? instanceWeight);
        }

        /// <summary>
        /// Reads data of a single instance. <c>null</c> is returned if the end-of-file is reached.
        /// </summary>
        /// <param name="instanceWeight">Variable that will be set to the instance weight or to <c>null</c>, if no weight is associated with the instance.</param>
        /// <returns>The instance data or <c>null</c> if the end-of-file was reached.
        /// <para>The element types in the returned array depend on the type of their corresponding attribute:
        /// <see cref="double"/> (numeric attribute),
        /// <see cref="string"/> (string attribute),
        /// <see cref="int"/> (nominal attribute, index into nominal values array),
        /// <see cref="DateTime"/> (date attribute),
        /// <see cref="object"/>[][] (relational attribute).
        /// Missing values are represented as <c>null</c>.</para>
        /// </returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="InvalidDataException"/>
        public Instance ReadInstance(out double? instanceWeight)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (Header == null)
                throw new InvalidOperationException("Before any instances can be read, the header needs to be read by a call to ReadHeader.");

            return ReadInstance(out instanceWeight, Header.Features, streamReader);
        }

        private Instance ReadInstance(out double? instanceWeight, IReadOnlyList<Feature> attributes, TextReader textReader)
        {
            instanceWeight = null;

            int c;

            // skip whitespace, comments and end-of-line
            while (true)
            {
                c = textReader.Peek();

                if (c == -1)
                    break;
                else if (char.IsWhiteSpace((char)c))
                    textReader.Read();
                else if (c == '%')
                {
                    do
                    {
                        c = textReader.Read();
                    } while (c != '\r' && c != '\n' && c != -1);
                    if (c == '\r' && textReader.Peek() == '\n')
                        textReader.Read();
                }
                else
                    break;
            }

            if (c == -1)
                return null;

            Instance instance;

            if (c == '{')
                instance = ReadSparseInstance(attributes, textReader);
            else
            {
                //instance = new object[attributes.Count];
                //instance = new Instance(attributes);
                instance = new Instance(Header);

                for (int i = 0; i < instance.Length; i++)
                //for (int i = 0; i < instance.Values.Keys.Count; i++)
                {
                    string value = ReadToken(out bool quoted, endOfLine: false, textReader: textReader);

                    //instance[i] = ParseValue(value, quoted, attributes[i].Type);
                    instance[attributes[i]] = ParseValue(value, quoted, attributes[i].Type);

                    if (i != instance.Length - 1)
                        //if (i != instance.Values.Keys.Count - 1)
                        ReadToken(expectedToken: ",", endOfLine: false, quoting: false, textReader: textReader);
                }
            }

            string token = ReadToken(quoting: false, textReader: textReader);

            if (token != null)
                if (token == ",")
                {
                    ReadToken(expectedToken: "{", endOfLine: false, quoting: false, textReader: textReader);
                    string weightToken = ReadToken(endOfLine: false, textReader: textReader);

                    if (!double.TryParse(weightToken, NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                        throw new InvalidDataException($"Invalid instance weight \"{weightToken}\".");

                    instanceWeight = weight;

                    ReadToken(expectedToken: "}", endOfLine: false, quoting: false, textReader: textReader);
                    ReadToken(endOfLine: true, textReader: textReader);
                }
                else
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected \",\" or end-of-line.");

            return instance;
        }

        private Instance ReadSparseInstance(IReadOnlyList<Feature> attributes, TextReader textReader)
        {
            //Instance instance = new object[attributes.Count];
            //Instance instance = new Instance(attributes);
            Instance instance = new Instance(Header);

            for (int i = 0; i < instance.Length; i++)
                if (attributes[i].Type is NumericFeature)
                    //instance[i] = 0.0;
                    instance[attributes[i]] = 0.0;
                else if (attributes[i].Type is NominalFeature)
                    //instance[i] = 0;
                    instance[attributes[i]] = 0;

            ReadToken(expectedToken: "{", endOfLine: false, quoting: false, textReader: textReader);

            string token = ReadToken(endOfLine: false, quoting: false, textReader: textReader);

            if (token == "}")
                return instance;

            while (true)
            {
                if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out int i))
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected index.");

                if (i < 0 || i >= instance.Length)
                    throw new InvalidDataException($"Out-of-range index \"{token}\".");

                string value = ReadToken(out bool quoted, endOfLine: false, textReader: textReader);

                //instance[index] = ParseValue(value, quoted, attributes[index].Type);
                instance[attributes[i]] = ParseValue(value, quoted, attributes[i].Type);

                token = ReadToken(endOfLine: false, quoting: false, textReader: textReader);

                if (token == "}")
                    break;
                else if (token != ",")
                    throw new InvalidDataException($"Unexpected token \"{token}\". Expected \",\" or \"}}\".");

                token = ReadToken(endOfLine: false, quoting: false, textReader: textReader);
            }

            return instance;
        }

        private object ParseValue(string value, bool quoted, FeatureType attributeType)
        {
            if (!quoted && value == "?")
                return null;

            if (attributeType == FeatureType.Integer)
            {
                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int d))
                    throw new InvalidDataException($"Unrecognized data value: \"{value}\"");

                return d;
            }
            else if (attributeType == FeatureType.Numeric)
            {
                if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    throw new InvalidDataException($"Unrecognized data value: \"{value}\"");

                return d;
            }
            else if (attributeType == FeatureType.String)
            {
                return value;
            }
            else if (attributeType is NominalFeature nominalAttribute)
            {
                int index = nominalAttribute.Values.IndexOf(value);

                if (index == -1)
                    throw new InvalidDataException($"Unrecognized data value: \"{value}\"");

                return index;
            }
            else if (attributeType is DateFeature dateAttribute)
            {
                if (!DateTime.TryParseExact(value, dateAttribute.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out DateTime d))
                    throw new InvalidDataException($"Unrecognized data value: \"{value}\"");

                return d;
            }
            else if (attributeType is RelationalFeature relationalAttribute)
            {
                List<Instance> relationalInstances = new List<Instance>();

                using (StringReader stringReader = new StringReader(value))
                    while (true)
                    {
                        // weights for relational instances are currently discarded
                        Instance instance = ReadInstance(out double? instanceWeight, relationalAttribute.ChildFeatures, stringReader);

                        if (instance == null)
                            break;

                        relationalInstances.Add(instance);
                    }

                return relationalInstances.ToArray();
            }
            else
                throw new ArgumentException("Unsupported ArffAttributeType.", nameof(attributeType));
        }

        /// <summary>
        /// Reads data of all instances.
        /// </summary>
        /// <returns>Array with data of all instances.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="InvalidDataException"/>
        /// <seealso cref="ReadInstance()"/>
        public Instance[] ReadAllInstances()
        {
            List<Instance> instances = new List<Instance>();

            Instance instance;

            while ((instance = ReadInstance()) != null)
                instances.Add(instance);

            Instances = instances.ToArray();
            return Instances;
        }

        /// <summary>
        /// Returns an enumerable that reads data of all instances during enumeration.
        /// </summary>
        /// <returns>Enumerable with data of all instances.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="InvalidDataException"/>
        /// <seealso cref="ReadAllInstances()"/>
        public IEnumerable<Instance> ReadInstances()
        {
            Instance instance;

            while ((instance = ReadInstance()) != null)
                yield return instance;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ARFFReader"/> object.
        /// </summary>
        /// <param name="disposing">Whether this method is called from <see cref="IDisposable.Dispose"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    streamReader.Dispose();
                }

                streamReader = null;
                Header = null;

                disposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ARFFReader"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }

    /// <summary>
    /// An ARFF writer
    /// </summary>
    public class ARFFWriter : IDisposable
    {
        StreamWriter streamWriter;
        bool disposed = false;
        /// <summary>
        /// Releases all resources used by the <see cref="ARFFWriter"/> object.
        /// </summary>
        /// <param name="disposing">Whether this method is called from <see cref="IDisposable.Dispose"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    streamWriter.Dispose();


                disposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ARFFWriter"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
