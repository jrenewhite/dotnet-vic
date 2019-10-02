using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet_vic.core.Evaluation
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfusionMatrix
    {
        public string PredictedClass { get; private set; }
        public int TotalInstances { get; private set; }
        public int TruePositive { get; private set; }
        public int TrueNegative { get; private set; }
        public int FalsePositive { get; private set; }
        public int FalseNegative { get; private set; }

        public int[,] Matrix
        {
            get
            {
                return new int[,]{
                    { TruePositive, FalsePositive },
                    { TrueNegative, FalseNegative }
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected ConfusionMatrix()
        {
            PredictedClass = "Unknown";
            TruePositive = 0;
            TrueNegative = 0;
            FalsePositive = 0;
            FalseNegative = 0;
        }

        /// <summary>
        /// Creates a new <see cref="ConfusionMatrix"/> instance
        /// </summary>
        /// <param name="predictedClass"></param>
        /// <param name="totalInstances"></param>
        /// <param name="truePositive"></param>
        /// <param name="trueNegative"></param>
        /// <param name="falsePositive"></param>
        /// <param name="falseNegative"></param>
        public ConfusionMatrix(string predictedClass, int totalInstances, int truePositive, int trueNegative, int falsePositive, int falseNegative) : this()
        {
            PredictedClass = predictedClass ?? "Unknown";
            TotalInstances = totalInstances < 0 ? 0 : totalInstances;
            TruePositive = truePositive < 0 ? 0 : truePositive;
            TrueNegative = trueNegative < 0 ? 0 : trueNegative;
            FalsePositive = falsePositive < 0 ? 0 : falsePositive;
            FalseNegative = falseNegative < 0 ? 0 : falseNegative;
        }

        public int this[int row, int colum]
        {
            get { return Matrix[row, colum]; }
            set { Matrix[row, colum] = value; }
        }

        #region Totals
        public int TotalPositives { get { return TruePositive + FalsePositive; } }
        public int TotalNegatives { get { return TrueNegative + FalseNegative; } }

        public int TotalConditionPositives { get { return TruePositive + FalseNegative; } }
        public int TotalConditionNegatives { get { return TrueNegative + FalsePositive; } }
        #endregion

        #region Rates
        public double TruePositiveRate { get { var value = TruePositive * 1.0 / TotalConditionPositives; return double.IsNaN(value) ? 0.0 : value; } }
        public double TrueNegativeRate { get { var value = TrueNegative * 1.0 / TotalConditionNegatives; return double.IsNaN(value) ? 0.0 : value; } }
        public double FalsePositiveRate { get { var value = FalsePositive * 1.0 / TotalConditionNegatives; return double.IsNaN(value) ? 0.0 : value; } }
        public double FalseNegativeRate { get { var value = FalseNegative * 1.0 / TotalConditionPositives; return double.IsNaN(value) ? 0.0 : value; } }
        public double FalseOmissionRate { get { var value = FalseNegative * 1.0 / TotalNegatives; return double.IsNaN(value) ? 0.0 : value; } }
        public double FalseDiscoveryRate { get { var value = FalsePositive * 1.0 / TotalPositives; return double.IsNaN(value) ? 0.0 : value; } }
        public double YieldRate { get { var value = TotalPositives * 1.0 / TotalInstances; return double.IsNaN(value) ? 0.0 : value; } }
        #endregion

        #region True positive rate aliases
        public double Recall { get { return TruePositiveRate; } }
        public double HitRate { get { return TruePositiveRate; } }
        public double Sensitivity { get { return TruePositiveRate; } }
        #endregion

        #region True negative rate aliases
        public double Specificity { get { return TrueNegativeRate; } }
        public double Selectivity { get { return TrueNegativeRate; } }
        #endregion

        #region False negative rate aliases
        public double MissRate { get { return FalseNegativeRate; } }
        #endregion

        #region False positive rate aliases
        public double FallOut { get { return FalsePositiveRate; } }
        #endregion

        #region Other scores, indexes and derived metrics
        public double BookmakerInformedness { get { return TruePositiveRate + TrueNegativeRate - 1; } }
        public double Informedness { get { return BookmakerInformedness; } }

        public double ThreatScore { get { var value = TruePositive * 1.0 / (TotalConditionPositives + FalsePositive); return double.IsNaN(value) ? 0.0 : value; } }
        public double CriticalSuccessIndex { get { return ThreatScore; } }

        public double PositivePredictingValue { get { var value = TruePositive * 1.0 / TotalPositives; return double.IsNaN(value) ? 0.0 : value; } }
        public double NegativePredictingValue { get { var value = TrueNegative * 1.0 / TotalNegatives; return double.IsNaN(value) ? 0.0 : value; } }
        public double Precision { get { return PositivePredictingValue; } }

        public double Accuracy { get { var value = (TruePositive + TrueNegative) * 1.0 / (TotalPositives + TotalNegatives); return double.IsNaN(value) ? 0.0 : value; } }
        public double F1Score { get { var value = 2.0 * ((PositivePredictingValue * TruePositiveRate) / (PositivePredictingValue + TruePositiveRate)); return double.IsNaN(value) ? 0.0 : value; } }
        public double Markedness { get { return PositivePredictingValue + NegativePredictingValue - 1; } }
        public double MatthewsCorrelationCoefficient { get { var value = ((TruePositive * TrueNegative) - (FalsePositive * FalseNegative)) / Math.Sqrt((TruePositive + FalsePositive) * (TruePositive + FalseNegative) * (TrueNegative + FalsePositive) * (TrueNegative + FalseNegative)); return double.IsNaN(value) ? 0.0 : value; } }

        public double AreaUnderTheCurveROC { get { return (1 + (TruePositiveRate - FalsePositiveRate)) / 2; } }
        #endregion
    }
}
