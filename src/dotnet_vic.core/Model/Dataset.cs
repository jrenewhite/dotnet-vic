using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using Yaap;
using static Yaap.YaapConsole;
using static System.Linq.Enumerable;

namespace dotnet_vic.core.Model
{
    /// <summary>
    /// Represents the header of an ARFF file consisting of relation name and attribute declarations.
    /// </summary>
    [Serializable]
    public class Dataset
    {
        public string Name { get; private set; }
        public Header Header { get; protected set; }
        public Instance[] Instances { get; protected set; }

        public DatasetInformation DatasetInformation { get; protected set; }

        protected Dataset()
        {
            Name = "Unknown";
            Header = new Header("Unknown", new Feature[0]);
            Instances = new Instance[0];
        }

        public Dataset(Header header, Instance[] instances) : this()
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Instances = instances ?? throw new ArgumentNullException(nameof(instances));
            Name = (Header != null) ? Header.RelationName : "Unknown";

            var iterableFeatures = Header.Features.Yaap(settings: new YaapSettings()
            {
                Description = $"Processing features of {Name}",
                ColorScheme = YaapColorScheme.Bright,
                UnitName = "feature"
            });
            foreach (var feature in iterableFeatures)
            {
                if (feature.Type is IntegerFeature)
                {
                    var nonMissingValues = Instances.Where(x => !feature.Type.IsMissing(x[feature])).Select(x => (int)x[feature]).ToArray();
                    var valuesmissing = Instances.Length - nonMissingValues.Length;
                    int max, min;
                    if (nonMissingValues.Length > 0)
                    {
                        max = nonMissingValues.Max();
                        min = nonMissingValues.Min();
                    }
                    else
                    {
                        max = 0;
                        min = 0;
                    }

                    feature.FeatureInformation = new IntegerFeatureInformation()
                    {
                        MissingValueCount = valuesmissing,
                        MaxValue = max,
                        MinValue = min,
                        Feature = feature,
                    };
                }
                else if (feature.Type is NumericFeature)
                {
                    var nonMissingValues = Instances.Where(x => !feature.Type.IsMissing(x[feature])).Select(x => (double)x[feature]).ToArray();
                    var valuesmissing = Instances.Length - nonMissingValues.Length;
                    double max, min;
                    if (nonMissingValues.Length > 0)
                    {
                        max = nonMissingValues.Max();
                        min = nonMissingValues.Min();
                    }
                    else
                    {
                        max = 0;
                        min = 0;
                    }

                    feature.FeatureInformation = new NumericFeatureInformation()
                    {
                        MissingValueCount = valuesmissing,
                        MaxValue = max,
                        MinValue = min,
                        Feature = feature,
                    };
                }
                else if (feature.Type is NominalFeature)
                {
                    var len = ((NominalFeature)feature.Type).Values.Count;
                    double[] valuesCount = new double[len];

                    bool missingFeatures = Instances.Any(x => !feature.Type.IsMissing(x[feature]));

                    var iterableValues = Range(0, len).Yaap(settings: new YaapSettings()
                    {
                        Description = $"Counting each value of {feature.Name}'s appearances",
                        ColorScheme = YaapColorScheme.NoColor
                    });
                    foreach (var i in iterableValues)
                        valuesCount[i] = Instances.Where(x => !feature.Type.IsMissing(x[feature])).Count(x => (int)x[feature] == i);

                    //for (int i = 0; i < len; i++)
                    //    valuesCount[i] = Instances.Where(x => !feature.Type.IsMissing(x[feature])).Count(x => (int)x[feature] == i);

                    var valuesmissing = Instances.Select(x => x[feature]).Count(feature.Type.IsMissing);
                    var valueProbability = valuesCount.Select(x => x / (valuesCount.Sum() * 1.0)).ToArray();
                    var ratio = valuesCount.Select(x => x / (valuesCount.Min() * 1F)).ToArray();

                    feature.FeatureInformation = new NominalFeatureInformation()
                    {
                        Distribution = valuesCount,
                        MissingValueCount = valuesmissing,
                        ValueProbability = valueProbability,
                        Ratio = ratio,
                        Feature = feature,
                    };
                }
            }

            int objWithIncompleteData = 0;

            var iterableInstances = Instances.Yaap(settings: new YaapSettings()
            {
                Description = "Processing instances...",
                ColorScheme = YaapColorScheme.Bright,
                UnitName = "feature"
            });

            foreach (var instance in iterableInstances)
                if (Header.Features.Any(feature => feature.Type.IsMissing(instance[feature])))
                    objWithIncompleteData++;

            DatasetInformation = new DatasetInformation()
            {
                ObjectsWithIncompleteData = objWithIncompleteData,
                GlobalAbscenseInformation = Header.Features.Sum(feature => feature.FeatureInformation.MissingValueCount)
            };

        }

        public double[][] GetColumns(IEnumerable<Feature> features) => Instances.Select(i => i.GetColumns(features)).ToArray();
        public double[][] GetColumns(IEnumerable<string> featureNames) => Instances.Select(i => i.GetColumns(featureNames)).ToArray();
        public double[][] GetColumns(IEnumerable<int> featureIdxs) => Instances.Select(i => i.GetColumns(featureIdxs)).ToArray();
        public double[][] GetInputs() => Instances.Select(i => i.GetInputs()).ToArray();

        public double[] GetColumn(Feature feature) => Instances.Select(i => i.GetColumn(feature)).ToArray();
        public double[] GetColumn(string featureName) => Instances.Select(i => i.GetColumn(featureName)).ToArray();
        public double[] GetColumn(int featureIdx) => Instances.Select(i => i.GetColumn(featureIdx)).ToArray();
        public int[] GetClasses() => Instances.Select(i => i.GetClass()).ToArray();

    }


    [Serializable]
    public class DatasetInformation
    {
        public int ObjectsWithIncompleteData { get; set; }
        public int GlobalAbscenseInformation { get; set; }
    }

}
;