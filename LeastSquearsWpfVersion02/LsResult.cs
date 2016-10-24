/*
 * File: LsResult.cs
 * ----------------------------------------
 * This class contains:
 *      1. All calculated data and analysis of Beta
 *      2. Names of variables
 */

using MathNet.Numerics.LinearAlgebra;

namespace LeastSquearsWpfVersion02
{
    public class LsResult
    {
        #region Values names

        public const string BetaName = "B";
        public const string EName = "e";
        public const string DeltaName = "δ";
        public const string TbetaName = "Tβ";

        public const string BetaAvgName = "Tβ";
        public const string SigmaName = "σ";
        public const string RName = "R";
        public const string Rp2Name = "R^2";
        public const string FName = "F";
        public const string FTableName = "F-table";
        public static readonly string[] SglNames =
        {
            BetaAvgName,
            SigmaName,
            RName,
            Rp2Name,
            FName,
            FTableName
        };

        #endregion


        /*
         * Have to be true when all data calculated 
         */
        public bool IsCalculated { get; set; }

        public Vector<double> YRes { get; set; }
        public Vector<double> Beta { get; set; }
        public Vector<double> E { get; set; }
        public Vector<double> Delta { get; set; }
        public Vector<double> Tbeta { get; set; }

        public double BetaAvg { get; set; }
        public double SigmaE { get; set; }
        public double R { get; set; }
        public double Rp2 { get; set; }
        public double F { get; set; }
        public double Ftable { get; set; }

        public LsResult()
        {
            IsCalculated = false;
        }
        
    }
}