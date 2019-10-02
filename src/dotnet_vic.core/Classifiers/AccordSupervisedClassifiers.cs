using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using DT = Accord.MachineLearning.DecisionTrees;
using DTL = Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.Performance;
using Accord.Math.Optimization.Losses;
using Accord.Math.Random;
using Accord.Statistics.Analysis;
using dotnet_vic.core.Model;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using B = Accord.MachineLearning.Bayes;

namespace dotnet_vic.core.Classifiers
{
    public interface IAccordSupervisedClassifier<TLearner, TModel> : ISupervisedClassifier
    {
    }
    #region Base class for usual Accord.net supervised classifiers

    public abstract class AccordSupervisedClassifiers<TLearner, TModel> : IAccordSupervisedClassifier<TLearner, TModel> where TModel : class, ITransform<double[], int> where TLearner : class, ISupervisedLearning<TModel, double[], int>
    {
        public TLearner learner;
        public TModel classifier;
        public CrossValidation<TModel, TLearner, double[], int> crossValidation;

        public bool Initialized { get; protected set; }
        public double Accuracy { get; protected set; }
        public int[,] ConfusionMatrix { get; protected set; }
        public Header Header { get; protected set; }

        protected int[] ActualResults;

        protected AccordSupervisedClassifiers()
        {
            Initialized = false;
            Accuracy = 0.0;
            ConfusionMatrix = new int[0, 0];
            Header = null;
            ActualResults = new int[0];
        }

        public abstract int Classify(double[] input);

        public abstract int[] Classify(double[][] inputs);

        public abstract void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs);

        public abstract bool Initialize(Header header);

        public abstract bool Initialize(Header header,Dictionary<string, object> configurations);

        public abstract void Train(double[][] inputs, int[] outputs);

        public void CalculateConfusionMatrix(int[] expected, int[] actual = null)
        {

            if (actual != null && actual.Length == expected.Length)
            {
                ConfusionMatrix = new GeneralConfusionMatrix(((NominalFeature)Header.ClassFeature.Type).Values.Count(), expected, actual).Matrix;
            }
            else if (ActualResults.Length > 0 && ActualResults.Length == expected.Length)
            {
                ConfusionMatrix = new GeneralConfusionMatrix(((NominalFeature)Header.ClassFeature.Type).Values.Count(), expected, ActualResults).Matrix;
            }
            throw new ArgumentException("Please provide an actual result to generate the confusion matrix or classify instances");
        }
    }
    #endregion

    #region C45 algorithm
    public class C45 : AccordSupervisedClassifiers<DTL.C45Learning, DT.DecisionTree>
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

        public override void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs)
        {
            crossValidation = CrossValidation.Create(

                k: 10, // We will be using 10-fold cross validation

                learner: (p) => new DTL.C45Learning() // here we create the learning algorithm
                {
                    Join = 2,
                    MaxHeight = 5
                },

                // Now we have to specify how the tree performance should be measured:
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),

                // This function can be used to perform any special
                // operations before the actual learning is done, but
                // here we will just leave it as simple as it can be:
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),

                // Finally, we have to pass the input and output data
                // that will be used in cross-validation. 
                x: inputs, y: outputs
            );
            var result = crossValidation.Learn(inputs, outputs);

            ConfusionMatrix = result.ToConfusionMatrix(inputs, outputs).Matrix;
        }

        public override bool Initialize(Header header,Dictionary<string, object> configurations)
        {
            return Initialize(header);
        }

        public override bool Initialize(Header header)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new DTL.C45Learning();

            foreach (Feature feature in Header.Features.Where(f => f.IsRetrievable()))
            {
                if (feature.Type == FeatureType.Numeric)
                {
                    learner.Add(new DT.DecisionVariable(feature.Name, DT.DecisionVariableKind.Continuous));
                }
                else
                {
                    learner.Add(new DT.DecisionVariable(feature.Name, DT.DecisionVariableKind.Discrete));
                }
            }

            Initialized = true;
            return Initialized;
        }

        public override void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs, outputs);
            }
        }
    }
    #endregion

    #region Random forest algorithm

    public class RandomForest : AccordSupervisedClassifiers<DT.RandomForestLearning, DT.RandomForest>
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

        public override void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs)
        {
            crossValidation = CrossValidation.Create(

                k: 10, // We will be using 10-fold cross validation

                learner: (p) => new DT.RandomForestLearning() // here we create the learning algorithm
                {
                    NumberOfTrees = 100
                },

                // Now we have to specify how the tree performance should be measured:
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),

                // This function can be used to perform any special
                // operations before the actual learning is done, but
                // here we will just leave it as simple as it can be:
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),

                // Finally, we have to pass the input and output data
                // that will be used in cross-validation. 
                x: inputs, y: outputs
            );
            var result = crossValidation.Learn(inputs, outputs);

            ConfusionMatrix = result.ToConfusionMatrix(inputs, outputs).Matrix;
        }

        public override bool Initialize(Header header,Dictionary<string, object> configurations)
        {
            return Initialize(header);
        }

        public override bool Initialize(Header header)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new DT.RandomForestLearning()
            {
                NumberOfTrees = 100
            };

            Initialized = true;
            return Initialized;
        }

        public override void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs, outputs);
            }
        }
    }
    #endregion
   
    #region K-Nearest Neighbours

    public class KNN : AccordSupervisedClassifiers<KNearestNeighbors, KNearestNeighbors>
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

        public override void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs)
        {
            crossValidation = CrossValidation.Create(

                k: 10, // We will be using 10-fold cross validation

                learner: (p) => new KNearestNeighbors() // here we create the learning algorithm
                {

                },

                // Now we have to specify how the tree performance should be measured:
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),

                // This function can be used to perform any special
                // operations before the actual learning is done, but
                // here we will just leave it as simple as it can be:
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),

                // Finally, we have to pass the input and output data
                // that will be used in cross-validation. 
                x: inputs, y: outputs
            );
            var result = crossValidation.Learn(inputs, outputs);

            ConfusionMatrix = result.ToConfusionMatrix(inputs, outputs).Matrix;
        }

        public override bool Initialize(Header header,Dictionary<string, object> configurations)
        {
            return Initialize(header);
        }

        public override bool Initialize(Header header)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new KNearestNeighbors()
            {
            };

            Initialized = true;
            return Initialized;
        }

        public override void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs, outputs);
            }
        }
    }
    #endregion

    #region Linear Discriminant Analysis
    public class LDA : AccordSupervisedClassifiers<LinearDiscriminantAnalysis, LinearDiscriminantAnalysis.Pipeline>
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

        public override void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs)
        {
            crossValidation = CrossValidation.Create(

                k: 10, // We will be using 10-fold cross validation

                learner: (p) => new LinearDiscriminantAnalysis() // here we create the learning algorithm
                ,

                // Now we have to specify how the tree performance should be measured:
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),

                // This function can be used to perform any special
                // operations before the actual learning is done, but
                // here we will just leave it as simple as it can be:
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),

                // Finally, we have to pass the input and output data
                // that will be used in cross-validation. 
                x: inputs, y: outputs
            );
            var result = crossValidation.Learn(inputs, outputs);

            ConfusionMatrix = result.ToConfusionMatrix(inputs, outputs).Matrix;
        }

        public override bool Initialize(Header header,Dictionary<string, object> configurations)
        {
            return Initialize(header);
        }

        public override bool Initialize(Header header)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new LinearDiscriminantAnalysis();

            Initialized = true;
            return Initialized;
        }

        public override void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs, outputs);
            }
        }
    }
    #endregion

    #region Quadratic Discriminant Analysis
    public class QDA : AccordSupervisedClassifiers<KernelDiscriminantAnalysis, KernelDiscriminantAnalysis.Pipeline>
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

        public override void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs)
        {
            crossValidation = CrossValidation.Create(

                k: 10, // We will be using 10-fold cross validation

                learner: (p) => new KernelDiscriminantAnalysis() // here we create the learning algorithm
                {
                    Kernel = new Quadratic() // We can choose any kernel function
                },

                // Now we have to specify how the tree performance should be measured:
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),

                // This function can be used to perform any special
                // operations before the actual learning is done, but
                // here we will just leave it as simple as it can be:
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),

                // Finally, we have to pass the input and output data
                // that will be used in cross-validation. 
                x: inputs, y: outputs
            );
            var result = crossValidation.Learn(inputs, outputs);

            ConfusionMatrix = result.ToConfusionMatrix(inputs, outputs).Matrix;
        }

        public override bool Initialize(Header header,Dictionary<string, object> configurations)
        {
            return Initialize(header);
        }

        public override bool Initialize(Header header)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new KernelDiscriminantAnalysis()
            {
                Kernel = new Quadratic()
            };

            Initialized = true;
            return Initialized;
        }

        public override void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs, outputs);
            }
        }
    }
    #endregion

    #region Base class for support vector machines based classifiers

    public abstract class AccordSVMClassifier<TLearner, TModel> : IAccordSupervisedClassifier<TLearner, TModel> where TModel : MulticlassSupportVectorMachine<Gaussian> where TLearner : class, ISupervisedLearning<TModel, double[], int>
    {
        public TLearner learner;
        public TModel classifier;
        public CrossValidation<TModel, TLearner, double[], int> crossValidation;

        public bool Initialized { get; protected set; }
        public double Accuracy { get; protected set; }
        public int[,] ConfusionMatrix { get; protected set; }
        public Header Header { get; protected set; }

        protected int[] ActualResults;

        protected AccordSVMClassifier()
        {
            Initialized = false;
            Accuracy = 0.0;
            ConfusionMatrix = new int[0, 0];
            Header = null;
            ActualResults = new int[0];
        }

        public AccordSVMClassifier(Header header) : this()
        {
            Header = header;
        }

        public abstract int Classify(double[] input);

        public abstract int[] Classify(double[][] inputs);

        public abstract void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs);

        public abstract bool Initialize(Header header);

        public abstract bool Initialize(Header header,Dictionary<string, object> configurations);

        public abstract void Train(double[][] inputs, int[] outputs);

        public void CalculateConfusionMatrix(int[] expected, int[] actual = null)
        {

            if (actual != null && actual.Length == expected.Length)
            {
                ConfusionMatrix = new GeneralConfusionMatrix(((NominalFeature)Header.ClassFeature.Type).Values.Count(), expected, actual).Matrix;
            }
            else if (ActualResults.Length > 0 && ActualResults.Length == expected.Length)
            {
                ConfusionMatrix = new GeneralConfusionMatrix(((NominalFeature)Header.ClassFeature.Type).Values.Count(), expected, ActualResults).Matrix;
            }
            throw new ArgumentException("Please provide an actual result to generate the confusion matrix or classify instances");
        }
    }
    #endregion

    #region Gaussian Distribution Support Vector Machine
    public class SVM : AccordSVMClassifier<MulticlassSupportVectorLearning<Gaussian>, MulticlassSupportVectorMachine<Gaussian>>
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

        public override void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs)
        {
            crossValidation = CrossValidation.Create(

                k: 10, // We will be using 10-fold cross validation

                learner: (p) => new MulticlassSupportVectorLearning<Gaussian>()
                {
                    // Configure the learning algorithm to use SMO to train the
                    //  underlying SVMs in each of the binary class subproblems.
                    Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                    {
                        // Estimate a suitable guess for the Gaussian kernel's parameters.
                        // This estimate can serve as a starting point for a grid search.
                        UseKernelEstimation = true
                    }
                },

                // Now we have to specify how the tree performance should be measured:
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),

                // This function can be used to perform any special
                // operations before the actual learning is done, but
                // here we will just leave it as simple as it can be:
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),

                // Finally, we have to pass the input and output data
                // that will be used in cross-validation. 
                x: inputs, y: outputs
            );
            var result = crossValidation.Learn(inputs, outputs);

            ConfusionMatrix = result.ToConfusionMatrix(inputs, outputs).Matrix;
            //throw new NotImplementedException();
        }

        public override bool Initialize(Header header,Dictionary<string, object> configurations)
        {
            return Initialize(header);
        }

        public override bool Initialize(Header header)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new MulticlassSupportVectorLearning<Gaussian>()
            {
                // Configure the learning algorithm to use SMO to train the
                //  underlying SVMs in each of the binary class subproblems.
                Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                {
                    // Estimate a suitable guess for the Gaussian kernel's parameters.
                    // This estimate can serve as a starting point for a grid search.
                    UseKernelEstimation = true
                }
            };

            Initialized = true;
            return Initialized;
        }

        public override void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs, outputs);
            }
        }
    }
    #endregion

    #region Naive Bayes implementation from Accord.net

    public class NaiveBayes : ISupervisedClassifier
    {
        public B.NaiveBayesLearning learner;
        public B.NaiveBayes classifier;
        public CrossValidation<B.NaiveBayes, B.NaiveBayesLearning, int[], int> crossValidation;

        public bool Initialized { get; protected set; }
        public double Accuracy { get; protected set; }
        public int[,] ConfusionMatrix { get; protected set; }
        public Header Header { get; protected set; }

        protected int[] ActualResults;

        protected NaiveBayes()
        {
            Initialized = false;
            Accuracy = 0.0;
            ConfusionMatrix = new int[0, 0];
            Header = null;
            ActualResults = new int[0];
        }

        public int Classify(double[] input)
        {
            if (Initialized && classifier != null)
            {
                return classifier.Decide(input.Select(v => Convert.ToInt32(v)).ToArray());
            }
            else
            {
                throw new NullReferenceException("You need to train the classifier before attempting to classify an instance");
            }
        }

        public int[] Classify(double[][] inputs)
        {
            if (Initialized && classifier != null)
            {
                ActualResults = classifier.Decide(inputs.Select(u => u.Select(v => Convert.ToInt32(v)).ToArray()).ToArray());
                return ActualResults;
            }
            else
            {
                throw new NullReferenceException("You need to train the classifier before attempting to classify any instance");
            }
        }

        public void ApplyCrossValidation(int folds, double[][] inputs, int[] outputs)
        {
            crossValidation = CrossValidation.Create(

                k: 10, // We will be using 10-fold cross validation

                learner: (p) => new B.NaiveBayesLearning() // here we create the learning algorithm
                {

                },

                // Now we have to specify how the tree performance should be measured:
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),

                // This function can be used to perform any special
                // operations before the actual learning is done, but
                // here we will just leave it as simple as it can be:
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),

                // Finally, we have to pass the input and output data
                // that will be used in cross-validation. 
                x: inputs.Select(v => v.Select(u => Convert.ToInt32(u)).ToArray()).ToArray(), y: outputs
            );
            var result = crossValidation.Learn(inputs.Select(u => u.Select(v => Convert.ToInt32(v)).ToArray()).ToArray(), outputs);

            ConfusionMatrix = result.ToConfusionMatrix(inputs.Select(v => v.Select(u => Convert.ToInt32(u)).ToArray()).ToArray(), outputs).Matrix;
        }

        public bool Initialize(Header header,Dictionary<string, object> configurations)
        {
            return Initialize(header);
        }

        public bool Initialize(Header header)
        {
            // Ensure we have reproducible results
            Generator.Seed = 0;

            learner = new B.NaiveBayesLearning()
            {
            };

            Initialized = true;
            return Initialized;
        }

        public void Train(double[][] inputs, int[] outputs)
        {
            if (Initialized)
            {
                classifier = learner.Learn(inputs.Select(v => v.Select(u => Convert.ToInt32(u)).ToArray()).ToArray(), outputs);
            }
        }

        public void CalculateConfusionMatrix(int[] expected, int[] actual = null)
        {

            if (actual != null && actual.Length == expected.Length)
            {
                ConfusionMatrix = new GeneralConfusionMatrix(((NominalFeature)Header.ClassFeature.Type).Values.Count(), expected, actual).Matrix;
            }
            else if (ActualResults.Length > 0 && ActualResults.Length == expected.Length)
            {
                ConfusionMatrix = new GeneralConfusionMatrix(((NominalFeature)Header.ClassFeature.Type).Values.Count(), expected, ActualResults).Matrix;
            }
            throw new ArgumentException("Please provide an actual result to generate the confusion matrix or classify instances");
        }
    }

    #endregion
}
