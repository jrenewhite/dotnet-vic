using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace dotnet_vic.core.Model
{
    /// <summary>
    /// Represents the header of an ARFF file consisting of relation name and feature declarations.
    /// </summary>
    [Serializable]
    public class Instance
    {
        public int Length => Values != null ? Values.Count : 0;
        public Header Header { get; private set; }
        public double UnknownOrMissing { get; set; }

        public Dictionary<Feature, object> Values { get; private set; }

        public Instance()
        {
            Values = new Dictionary<Feature, object>();
            UnknownOrMissing = double.NaN;
        }

        public Instance(Header header) : this()
        {
            Header = header;
            Values = Header.Features.ToDictionary(feature => feature, feature => (object)null);
        }

        public Instance(IEnumerable<Feature> features) : this(new Header("Unknown", features))
        {
        }

        public Instance(Header header, IEnumerable<object> values) : this(header)
        {
            if (header.Features.Count() != values.Count())
            {
                throw new ArgumentOutOfRangeException("The number of values must be equal to the number of features");
            }
            Values = header.Features.Zip(values, (k, v) => new { Key = k, Value = v })
                     .ToDictionary(x => x.Key, x => x.Value);
        }

        public Instance(IEnumerable<Feature> features, IEnumerable<object> values) : this(new Header("Unknown", features), values)
        {
        }


        public double[] GetColumns(IEnumerable<Feature> features) => features.Select(GetColumn).ToArray();

        public double[] GetColumns(IEnumerable<string> featureNames) => featureNames.Select(GetColumn).ToArray();

        public double[] GetColumns(IEnumerable<int> featureIdxs) => featureIdxs.Select(GetColumn).ToArray();

        public double[] GetInputs() => GetColumns(Header.Features);

        public double GetColumn(Feature feature) => (feature.IsRetrievable()) ? (double)this[feature] : UnknownOrMissing;

        public double GetColumn(string featureName) => GetColumn(Header[featureName]);

        public double GetColumn(int featureIdx) => GetColumn(Header[featureIdx]);

        public int GetClass() => (int)GetColumn(Header.ClassFeature);


        public object this[Feature feature]
        {
            get { return Values[feature]; }
            set
            {
                if (value == null || value.ToString() == "?")
                {
                    Values[feature] = null;
                }
                else if (feature.Type is IntegerFeature)
                {
                    if (!int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int _))
                        throw new InvalidDataException($"Unrecognized data value: \"{value}\"");

                    Values[feature] = value;
                }
                else if (feature.Type is NumericFeature)
                {
                    if (!double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double _))
                        throw new InvalidDataException($"Unrecognized data value: \"{value}\"");

                    Values[feature] = value;
                }
                else
                {
                    Values[feature] = value;
                }
            }
        }

        public object this[string featureName]
        {
            get { return this[Header[featureName]]; }
            set { this[Header[featureName]] = value; }
        }

        public object this[int featureIdx]
        {
            get { return this[Header[featureIdx]]; }
            set { this[Header[featureIdx]] = value; }
        }
    }
}
