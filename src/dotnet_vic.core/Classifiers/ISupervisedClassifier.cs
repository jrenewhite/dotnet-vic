using dotnet_vic.core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet_vic.core.Classifiers
{
    public interface ISupervisedClassifier
    {
        /// <summary>
        /// States if the classifier has been initialized or not
        /// </summary>
        bool Initialized { get; }
        Header Header { get; }
        double Accuracy { get; }
        int[,] ConfusionMatrix { get; }
        /// <summary>
        /// Initializes the classifier
        /// </summary>
        /// <returns>A <see cref="bool"/> value indicating if classsifier is initialized</returns>
        bool Initialize(Header header);

        /// <summary>
        /// Initializes the classifier
        /// </summary>
        /// <param name="configurations">Configurations for the current classifier</param>
        /// <returns>A <see cref="bool"/> value indicating if classsifier is initialized</returns>
        bool Initialize(Header header, Dictionary<string, object> configurations);

        void Train(double[][] inputs, int[] outputs);

        int Classify(double[] input);

        int[] Classify(double[][] inputs);

        void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs);

        void CalculateConfusionMatrix(int[] expected, int[] actual = null);
    }
}
