using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace dotnet_vic.core.Model
{
    /// <summary>
    /// Represents an feature in a dataset.
    /// </summary>
    [Serializable]
    public class Feature
    {
        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the feature.
        /// </summary>
        public FeatureType Type { get; }

        /// <summary>
        /// Gets the information of the feature.
        /// </summary>
        public FeatureInformation FeatureInformation { get; set; }

        public Feature()
        {
            Name = "Unknown";
            Type = FeatureType.Numeric;
        }

        /// <summary>
        /// Initializes a new <see cref="Feature"/> instance with the specified name and feature type.
        /// </summary>
        /// <param name="name">The name of the feature to create.</param>
        /// <param name="type">The type of the feature to create.</param>
        /// <exception cref="ArgumentNullException"/>
        public Feature(string name, FeatureType type)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Name = name;
            Type = type;
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="Feature"/> with the same name and type).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            Feature other = obj as Feature;

            if (other == null)
                return false;

            return other.Name == Name && other.Type.Equals(Type);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            return $"@feature {QuoteAndEscape(Name)} {Type}";
        }

        internal static string QuoteAndEscape(string s)
        {
            if (s == string.Empty)
                return "''";
            if (s == "?")
                return "'?'";

            StringBuilder stringBuilder = new StringBuilder(s.Length + 2);

            bool quote = false;

            foreach (char c in s)
                switch (c)
                {
                    case '"':
                        stringBuilder.Append("\\\"");
                        quote = true;
                        break;
                    case '\'':
                        stringBuilder.Append("\\'");
                        quote = true;
                        break;
                    case '%':
                        stringBuilder.Append("\\%");
                        quote = true;
                        break;
                    case '\\':
                        stringBuilder.Append("\\\\");
                        quote = true;
                        break;
                    case '\r':
                        stringBuilder.Append("\\r");
                        quote = true;
                        break;
                    case '\n':
                        stringBuilder.Append("\\n");
                        quote = true;
                        break;
                    case '\t':
                        stringBuilder.Append("\\t");
                        quote = true;
                        break;
                    case '\u001E':
                        stringBuilder.Append("\\u001E");
                        quote = true;
                        break;
                    case ' ':
                    case ',':
                    case '{':
                    case '}':
                        stringBuilder.Append(c);
                        quote = true;
                        break;
                    default:
                        stringBuilder.Append(c);
                        break;
                }

            if (quote)
            {
                stringBuilder.Insert(0, '\'');
                stringBuilder.Append('\'');
            }

            return stringBuilder.ToString();
        }

        public bool IsRetrievable()
        {
            return Type.ValueRepresentationType == typeof(int) || Type.ValueRepresentationType == typeof(double);
        }
    }

    /// <summary>
    /// Abstract base class for all ARFF feature types.
    /// </summary>
    [Serializable]
    public abstract class FeatureType
    {
        public Type ValueRepresentationType { get; protected set; }

        /// <summary>
        /// Integer feature type.
        /// </summary>
        public static readonly IntegerFeature Integer = new IntegerFeature();

        /// <summary>
        /// Numeric feature type.
        /// </summary>
        public static readonly NumericFeature Numeric = new NumericFeature();

        /// <summary>
        /// String feature type.
        /// </summary>
        public static readonly StringFeature String = new StringFeature();

        static readonly DateFeature date = new DateFeature();

        internal FeatureType()
        {
            ValueRepresentationType = typeof(object);
        }

        /// <summary>
        /// Nominal feature type with the specified nominal values.
        /// </summary>
        /// <param name="values">Nominal values of the feature to create.</param>
        /// <returns>An <see cref="NominalFeature"/> instance representing the feature type.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static NominalFeature Nominal(params string[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return new NominalFeature(values);
        }

        /// <summary>
        /// Nominal feature type with the specified nominal values.
        /// </summary>
        /// <param name="values">Nominal values of the feature to create.</param>
        /// <returns>An <see cref="NominalFeature"/> instance representing the feature type.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static NominalFeature Nominal(IList<string> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return new NominalFeature(values);
        }

        /// <summary>
        /// Date feature type.
        /// </summary>
        /// <returns></returns>
        public static DateFeature Date()
        {
            return date;
        }

        /// <summary>
        /// Date feature type using the specified date format.
        /// </summary>
        /// <param name="dateFormat">Date format pattern as required by Java class <c>java.text.SimpleDateFormat</c>.</param>
        /// <returns>An <see cref="DateFeature"/> instance representing the feature type.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static DateFeature Date(string dateFormat)
        {
            if (dateFormat == null)
                throw new ArgumentNullException(nameof(dateFormat));

            return new DateFeature(dateFormat);
        }

        /// <summary>
        /// Relational feature type combining the specified child features.
        /// </summary>
        /// <param name="childAttributes">The child features of the relational feature type.</param>
        /// <returns>An <see cref="RelationalFeature"/> instance representing the feature type.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static RelationalFeature Relational(params Feature[] childAttributes)
        {
            if (childAttributes == null)
                throw new ArgumentNullException(nameof(childAttributes));

            return new RelationalFeature(childAttributes);
        }

        /// <summary>
        /// Relational feature type combining the specified child features.
        /// </summary>
        /// <param name="childAttributes">The child features of the relational feature type.</param>
        /// <returns>An <see cref="RelationalFeature"/> instance representing the feature type.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static RelationalFeature Relational(IList<Feature> childAttributes)
        {
            if (childAttributes == null)
                throw new ArgumentNullException(nameof(childAttributes));

            return new RelationalFeature(childAttributes);
        }

        /// <summary>
        /// Checks if a value is missing according to the <see cref="FeatureType"/>.
        /// </summary>
        public abstract bool IsMissing(object value);

        /// <summary>
        /// Returns a string representation of the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string ValueToString(object value);
    }

    /// <summary>
    /// Represents the numeric feature type.
    /// </summary>
    [Serializable]
    public sealed class IntegerFeature : FeatureType
    {
        internal IntegerFeature()
        {
            ValueRepresentationType = typeof(int);
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="IntegerFeature"/> with the same name).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is IntegerFeature;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool IsMissing(object value)
        {
            return value == null ? true : double.IsNaN(Convert.ToDouble((int)value));
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            return "integer";
        }

        public override string ValueToString(object value)
        {
            return double.IsNaN((int)value) ? "?" : value.ToString();
        }

    }

    /// <summary>
    /// Represents the numeric feature type.
    /// </summary>
    [Serializable]
    public sealed class NumericFeature : FeatureType
    {
        internal NumericFeature()
        {
            ValueRepresentationType = typeof(double);
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="NumericFeature"/> with the same name).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is NumericFeature;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool IsMissing(object value)
        {
            return value == null ? true : double.IsNaN((double)value);
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            return "numeric";
        }

        public override string ValueToString(object value)
        {
            return double.IsNaN((double)value) ? "?" : value.ToString();
        }
    }

    /// <summary>
    /// Represents the string feature type.
    /// </summary>
    [Serializable]
    public sealed class StringFeature : FeatureType
    {
        internal StringFeature()
        {
            ValueRepresentationType = typeof(string);
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="StringFeature"/> with the same name).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is StringFeature;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool IsMissing(object value)
        {
            return string.IsNullOrEmpty(value.ToString());
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            return "string";
        }

        public override string ValueToString(object value)
        {
            return value == null ? "?" : value.ToString();
        }
    }

    /// <summary>
    /// Represents the nominal feature type.
    /// </summary>
    [Serializable]
    public sealed class NominalFeature : FeatureType
    {
        /// <summary>
        /// Gets the nominal values of this nominal feature type.
        /// </summary>
        public ReadOnlyCollection<string> Values { get; }

        internal NominalFeature(IList<string> values)
        {
            Values = new ReadOnlyCollection<string>(values);
            ValueRepresentationType = typeof(int);
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="NominalFeature"/> with the same name and nominal values).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            NominalFeature other = obj as NominalFeature;

            if (other == null)
                return false;

            return other.Values.SequenceEqual(Values);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 19;

            foreach (string value in Values)
                hashCode = unchecked(hashCode * 31 + value.GetHashCode());

            return hashCode;
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            return "{" + string.Join(",", Values.Select(Feature.QuoteAndEscape)) + "}";
        }

        public override bool IsMissing(object value)
        {
            return value == null ? true : Values.Count < (int)value && (int)value <= 0;
        }

        public override string ValueToString(object value)
        {
            if (value == null || double.IsNaN((double)value))
                return "?";
            return "'" + Values[(int)value] + "'";
        }
    }

    /// <summary>
    /// Represents the date feature type.
    /// </summary>
    [Serializable]
    public sealed class DateFeature : FeatureType
    {
        /// <summary>
        /// Gets the date format that this date feature type is using.
        /// </summary>
        public string DateFormat { get; }

        internal const string DefaultDateFormat = "yyyy-MM-dd'T'HH:mm:ss";

        internal DateFeature()
        {
            DateFormat = DefaultDateFormat;
            ValueRepresentationType = typeof(DateTime);
        }

        internal DateFeature(string dateFormat)
        {
            DateFormat = dateFormat;
            ValueRepresentationType = typeof(DateTime);
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="DateFeature"/> with the same name and date format).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            DateFeature other = obj as DateFeature;

            if (other == null)
                return false;

            return other.DateFormat == DateFormat;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return DateFormat.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            if (DateFormat == DefaultDateFormat)
                return "date";
            else
                return "date " + Feature.QuoteAndEscape(DateFormat);
        }

        public override bool IsMissing(object value)
        {
            return DateTime.Equals((DateTime)value, null);
        }

        public override string ValueToString(object value)
        {
            if (value != null)
                return ((DateTime)value).ToString(DefaultDateFormat);
            return DateTime.MinValue.ToString(DefaultDateFormat);
        }
    }

    /// <summary>
    /// Represents the relational feature type.
    /// </summary>
    [Serializable]
    public sealed class RelationalFeature : FeatureType
    {
        /// <summary>
        /// Gets the child features of this relational feature type.
        /// </summary>
        public ReadOnlyCollection<Feature> ChildFeatures { get; }

        internal RelationalFeature(IList<Feature> childFeatures)
        {
            ChildFeatures = new ReadOnlyCollection<Feature>(childFeatures);
            ValueRepresentationType = typeof(object);
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="RelationalFeature"/> with the same name and child features).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            RelationalFeature other = obj as RelationalFeature;

            if (other == null)
                return false;

            return other.ChildFeatures.SequenceEqual(ChildFeatures);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 19;

            foreach (Feature attribute in ChildFeatures)
                hashCode = unchecked(hashCode * 31 + attribute.GetHashCode());

            return hashCode;
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            return "relational";
        }

        public override bool IsMissing(object value)
        {
            return ChildFeatures.Contains((Feature)value);
        }

        public override string ValueToString(object value)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents information about the <seealso cref="Feature"/> itself.
    /// </summary>
    [Serializable]
    public class FeatureInformation
    {
        public Feature Feature { protected get; set; }
        public int MissingValueCount { get; set; }

        public override string ToString()
        {
            return new string($"Missing values: {MissingValueCount}");
        }
    }

    [Serializable]
    public class NominalFeatureInformation : FeatureInformation
    {
        public NominalFeature NominalFeature
        {
            get { return Feature.Type as NominalFeature; }
        }
        public double[] Distribution { get; set; }
        public double[] ValueProbability { get; set; }
        public double[] Ratio { get; set; }

        public override string ToString()
        {
            return new string(base.ToString() +
            $"\nDistribution: [{string.Join("; ", Distribution.Select(x => x.ToString("F")))}]" +
            $"\nProbability per value: [{string.Join("; ", ValueProbability.Select(x => x.ToString("F")))}]" +
            $"\nRatio: [{string.Join("; ", Ratio.Select(x => x.ToString("F")))}]");
        }
    }

    [Serializable]
    public class IntegerFeatureInformation : FeatureInformation
    {
        public IntegerFeature IntegerFeature
        {
            get { return Feature.Type as IntegerFeature; }
        }
        public int MinValue;
        public int MaxValue;

        public override string ToString()
        {
            return new string(base.ToString() +
            $"\nMinimum value: {MinValue.ToString()}" +
            $"\nMaximum value: {MaxValue.ToString()}");
        }
    }

    [Serializable]
    public class NumericFeatureInformation : FeatureInformation
    {
        public NumericFeature NumericFeature
        {
            get { return Feature.Type as NumericFeature; }
        }
        public double MinValue;
        public double MaxValue;

        public override string ToString()
        {
            return new string(base.ToString() +
            $"\nMinimum value: {MinValue.ToString("F")}" +
            $"\nMaximum value: {MaxValue.ToString("F")}");
        }
    }

}
