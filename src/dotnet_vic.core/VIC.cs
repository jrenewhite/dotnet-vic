using dotnet_vic.core.Classifiers;
using dotnet_vic.core.Evaluation;
using dotnet_vic.core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnet_vic.core
{
    public class VIC
    {
        public Dataset ClusteredDataset { get; set; }
        public Dataset OriginalDataset { get; set; }

        public IEnumerable<ISupervisedClassifier> SupervisedClassifiers { get; set; }

        public IUnsupervisedClassifier ClusteringAlgorithm { get; set; }

        protected VIC()
        {
        }

        public VIC(Dataset dataset, IEnumerable<ISupervisedClassifier> supervisedClassifiers, IUnsupervisedClassifier clusteringAlgorithm)
        {
            OriginalDataset = dataset;
            ClusteredDataset = null;
            SupervisedClassifiers = supervisedClassifiers;
            ClusteringAlgorithm = clusteringAlgorithm;
        }

        public double GetValidityIndex(Dataset clusteredDataset = null, int numberOfSubsets = 5)
        {
            if (clusteredDataset == null)
            {
                Console.WriteLine("Not clustered dataset provided, creating clustered dataset");
                GetClusteredDataset(OriginalDataset, ClusteringAlgorithm);
            }
            else
            {
                ClusteredDataset = clusteredDataset;
            }

            Dataset[] subsets = SubdivideDataset(ClusteredDataset, numberOfSubsets);
            List<Feature> features = ClusteredDataset.Header.Features.ToList();
            Feature classFeature = ClusteredDataset.Header.ClassFeature;
            features.Remove(classFeature);
            double validityIndex = 0.0;
            foreach (var classifier in SupervisedClassifiers)
            {
                classifier.Initialize(ClusteredDataset.Header);
                double currentValidity = 0.0;
                for (int i = 0; i < numberOfSubsets; i++)
                {
                    IEnumerable<Instance> instances = ClusteredDataset.Instances.Except(subsets[i].Instances);
                    double[][] trainingFeatures = instances.Select(i => i.GetColumns(features)).ToArray();
                    double[][] testingFeatures = subsets[i].GetInputs();
                    int[] trainingClusters = instances.Select(i => i.GetClass()).ToArray();
                    int[] testingClusters = subsets[i].GetClasses();
                    classifier.Train(trainingFeatures, trainingClusters);
                    int[] results = classifier.Classify(testingFeatures);
                    GeneralConfusionMatrix confusionMatrix = new GeneralConfusionMatrix(classFeature, testingClusters, results);
                    currentValidity += confusionMatrix.GetClassErrorMatrixEvaluation(0).AreaUnderTheCurveROC;
                }
                validityIndex = Math.Max(validityIndex, currentValidity);
            }

            return validityIndex;
        }

        public Dataset[] SubdivideDataset(Dataset dataset, int n = 5)
        {
            if (n < 2)
            {
                throw new IndexOutOfRangeException("The dataset must be subdivided at least by 2");
            }
            if (dataset.Instances.Count() <= n)
            {
                throw new ArgumentOutOfRangeException($"There are not enough instances to create {n} subdivisions");
            }

            int sampleSize = dataset.Instances.Count() / n;
            Dataset[] Subdivisions = new Dataset[n];
            for (int i = 0; i < n; i++)
            {
                var instances = dataset.Instances.OrderBy(x => Guid.NewGuid()).Take(sampleSize).ToArray();
                Subdivisions[i] = new Dataset(dataset.Header, instances);
            }
            return Subdivisions;
        }

        public Dataset GetClusteredDataset(Dataset dataset, IUnsupervisedClassifier clusteringAlgorithm)
        {
            if (dataset == null)
            {
                throw new ArgumentNullException("An initial dataset is required");
            }
            Header header = new Header($"Clustered {dataset.Header.RelationName}", dataset.Header.Features);
            IEnumerable<Instance> instances = dataset.Instances;
            instances.ToList().ForEach(i => i[dataset.Header.ClassFeature] = null);
            Dataset clusteredDataset = new Dataset(header, instances.ToArray());
            Feature classFeature = clusteredDataset.Header.ClassFeature;
            List<Feature> features = clusteredDataset.Header.Features.ToList();
            features.Remove(clusteredDataset.Header.ClassFeature);
            clusteringAlgorithm.Initialize(new Dictionary<string, object> {
                {"k", ((NominalFeature)classFeature.Type).Values.Count }
            });
            clusteringAlgorithm.Train(clusteredDataset.GetColumns(features), clusteredDataset.GetClasses());
            int[] clusteringResults = clusteringAlgorithm.Classify(clusteredDataset.GetColumns(features));

            for (int i = 0; i < clusteringResults.Length; i++)
            {
                clusteredDataset.Instances[i][classFeature] = clusteringResults[i];
            }

            return clusteredDataset;
        }
    }
}
