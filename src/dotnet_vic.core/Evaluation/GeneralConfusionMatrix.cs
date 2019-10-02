using dotnet_vic.core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace dotnet_vic.core.Evaluation
{
    public class GeneralConfusionMatrix
    {
        public int[,] Matrix { get; private set; }

        public int Rows { get { return Matrix.GetLength(0); } }
        public int Columns { get { return Matrix.GetLength(1); } }

        public ConfusionMatrix[] ClassConfusionMatrices { get; private set; }

        private readonly ReadOnlyCollection<string> Classes;

        protected GeneralConfusionMatrix()
        {
            Matrix = new int[0, 0];
            Classes = null;
        }

        public GeneralConfusionMatrix(Feature classFeature) : this()
        {
            if (classFeature.Type is NominalFeature)
            {
                Classes = ((NominalFeature)classFeature.Type).Values;
                int values = Classes.Count;
                Matrix = new int[values, values];
                ClassConfusionMatrices = Classes.Select(c => new ConfusionMatrix(c, 0, 0, 0, 0, 0)).ToArray();
            }
            else
            {
                throw new ArgumentException("Cannot create a confusion matrix for a non-nominal class feature");
            }
        }

        public GeneralConfusionMatrix(Feature classFeature, int[] real, int[] predicted) : this(classFeature)
        {
            if (real.Length != predicted.Length)
                throw new ArgumentException("Cannot evaluate classification. Real and Predicted counts are different.");

            for (int i = 0; i < real.Length; i++)
            {
                Matrix[predicted[i], real[i]]++;
            }
            foreach (var predictedClass in Classes)
            {
                GetConfusionMatrix(predictedClass);
            }

        }

        public ConfusionMatrix GetConfusionMatrix(string className)
        {
            int classIdx = Classes.IndexOf(className);
            return GetClassErrorMatrixEvaluation(classIdx);
        }

        public ConfusionMatrix GetClassErrorMatrixEvaluation(int classIdx)
        {
            int truePositve = Matrix[classIdx, classIdx];
            int trueNegative = -Matrix[classIdx, classIdx];
            int falsePositive = Matrix[Matrix.GetLongLength(0) - 1, classIdx] - Matrix[classIdx, classIdx];
            int falseNegative = 0;

            for (int i = 0; i < Matrix.GetLength(1); i++)
            {
                trueNegative += Matrix[i, i];
                falsePositive += Matrix[classIdx, i];
            }

            falseNegative = TotalInstances - (truePositve + trueNegative + falsePositive);

            return new ConfusionMatrix(
                predictedClass: Classes[classIdx],
                totalInstances: TotalInstances,
                truePositive: Matrix[classIdx, classIdx],
                trueNegative: trueNegative,
                falsePositive: falsePositive,
                falseNegative: falseNegative);
        }

        public int this[int row, int colum]
        {
            get { return Matrix[row, colum]; }
            set { Matrix[row, colum] = value; }
        }


        public int TotalInstances
        {
            get
            {
                int result = 0;
                for (int i = 0; i < Matrix.GetLongLength(0); i++)
                {
                    for (int j = 0; j < Matrix.GetLongLength(1); j++)
                    {
                        result += Matrix[i, j];
                    }
                }


                return result;
            }
        }

    }
}
