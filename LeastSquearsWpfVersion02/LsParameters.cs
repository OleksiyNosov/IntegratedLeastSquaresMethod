/*
 * File: LsParameters.cs
 * ----------------------------------------
 * This class contains:
 *      -X and Y values, noises and beta
 */

using MathNet.Numerics.LinearAlgebra;

namespace LeastSquearsWpfVersion02
{
    public class LsParameters
    {
        #region Values names

        public const string BetaName = "β";

        public string[] XValsNames { get; set; }
        public string YValName { get; set; }

        #endregion

        public Matrix<double> XMtx { get; set; }
        public Vector<double> YVtr { get; set; }
        public Matrix<double> XMtxNoise { get; set; }
        public Vector<double> YVtrNoise { get; set; }
        public Vector<double> Beta { get; set; }


        public double Theta { get; set; }
        public double Gamma { get; set; }

        public NoiseTypes NoiseX { get; set; }
        public NoiseTypes NoiseY { get; set; }

        public int NumbOfTests { get; set; }
        public int NumbOfXVal { get; set; }

        /*
         * Pre-condition: 
         *      NumbOfXVal and NumbOfTests are initialized
         *      
         * Post-condition: 
         *      Initialize matrices and vectors             
         *      and sets its values to zero,           
         *      for the possibility of their use
         */
        public void InitializeMatrices()
        {
            XValsNames = new string[NumbOfXVal];

            XMtx = XMtxNoise = Matrix<double>.Build.Dense(NumbOfTests, NumbOfXVal, 0);
            YVtr = YVtrNoise = Vector<double>.Build.Dense(NumbOfTests, 0);
            Beta = Vector<double>.Build.Dense(NumbOfXVal);

            XMtxNoise = Matrix<double>.Build.Dense(NumbOfTests, NumbOfXVal, 0);
        } 

    }
}