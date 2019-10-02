using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using Accord.MachineLearning.Performance;
using Accord.Math.Optimization.Losses;
using Accord.Math.Random;
using Accord.Statistics.Analysis;
using dotnet_vic.core.Model;

namespace dotnet_vic.core.Classifiers
{
    public interface IAccordUnsupervisedClassifiers<TLearner, TModel> : IUnsupervisedClassifier
    {
    }

    public abstract class AccordKMeansClustering<TLearner, TModel> : IAccordUnsupervisedClassifiers<TLearner, TModel> where TModel : class, ICentroidClusterCollection<double[], KMeansClusterCollection.KMeansCluster> where TLearner : class, IClusteringAlgorithm<double[]>
    {
        public TLearner learner;
        public TModel classifier;

        protected int[] ActualResults;
        public bool Initialized { get; protected set; }
        public Header Header { get; protected set; }

        public abstract int Classify(double[] input);

        public abstract int[] Classify(double[][] inputs);

        public abstract bool Initialize();

        public abstract bool Initialize(Dictionary<string, object> configurations);

        public abstract void Train(double[][] inputs, int[] outputs);

    }

    public class AccordKMeans : AccordKMeansClustering<KMeans, KMeansClusterCollection>
    {
        public override int Classify(double[] input)
        {
            if (Initialized && classifier != null)
            {
                return classifier.Decide(input);
            }
            else
            {
                throw new NullReferenceException("You need to train the classifier before attempting to classify an instance");
            }
        }

        public override int[] Classify(double[][] inputs)
        {
            if (Initialized && classifier != null)
            {
                ActualResults = classifier.Decide(inputs);
                return ActualResults;
            }
            else
            {
                throw new NullReferenceException("You need to train the classifier before attempting to classify any instance");
            }
        }

        public override bool Initialize(Dictionary<string, object> configurations)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            int k = configurations.ContainsKey("k") ? (int)configurations["k"] : 3;

            learner = new KMeans(k);

            Initialized = true;
            return Initialized;
        }

        public override bool Initialize()
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new KMeans(k: 3);

            Initialized = true;
            return Initialized;
        }

        public override void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs);
            }
        }
    }
}
