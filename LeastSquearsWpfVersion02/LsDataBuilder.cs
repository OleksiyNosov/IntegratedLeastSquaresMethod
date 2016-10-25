/*
 * File: LsDataBuilder.cs
 * ----------------------------------------
 * This file consist of:
 *      1. Enum with noise types
 *      2. Class that create data such as:
 *      X, Y, noise and names
 *      for LsParameters       
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace LeastSquearsWpfVersion02
{
    /// <summary>
    /// Determines type of noise/error should be added to calculations
    /// </summary>
    public enum NoiseTypes
    {
        None = 0,
        White = 1,
        Colorful = 2
    }

    /// <summary>
    /// Create data specially for LsData
    /// </summary>
    public static class LsDataBuilder
    {
        private static Matrix<double> X;
        private static Vector<double> Y; 
        private static Matrix<double> XNoise;
        private static Vector<double> YNoise;
        private static Vector<double> Beta;

        private static NoiseTypes noiseX;
        private static NoiseTypes noiseY;

        private static int tests;
        private static int numbX;

        private static string[] xValsNames;
        private static string yValName;

        private static Random rand;

        /// <summary>
        /// Generate default LsData with starting parameters
        /// </summary>
        /// <param name="numbX">Number of X values</param>
        /// <param name="tests">Number of experiments</param>
        /// <param name="beta">Values which have influence for Y</param>
        /// <param name="noiseX">Type of noise/error for X values</param>
        /// <param name="noiseY">Type of noise/error for Y values</param>
        /// <param name="gamma">Used in further calculations</param>
        /// <param name="theta">Used in further calculations</param>
        /// <returns>Returns LsData which ready for further calculations</returns>
        public static LsData Build(int numbX, int tests, double[] beta, NoiseTypes noiseX, NoiseTypes noiseY, double gamma = 0, double theta = 1)
        {
            LsDataBuilder.numbX = numbX;
            LsDataBuilder.tests = tests;
            Beta = Vector<double>.Build.DenseOfArray(beta);
            LsDataBuilder.noiseX = noiseX;
            LsDataBuilder.noiseY = noiseY;

            CreateData();

            return new LsData(
                new LsParameters()
                {
                    XMtx = X,
                    YVtr = Y,
                    XMtxNoise = XNoise,
                    YVtrNoise = YNoise,
                    Beta = Beta,
                    NoiseX = LsDataBuilder.noiseX,
                    NoiseY = LsDataBuilder.noiseY,
                    NumbOfXVal = LsDataBuilder.numbX,
                    NumbOfTests = LsDataBuilder.tests,
                    XValsNames = xValsNames,
                    YValName = yValName,
                    Gamma = gamma,
                    Theta = theta
                });
        }

        /// <summary>
        /// Create all data for LsParameters
        /// </summary>
        /// <remarks>
        ///  tests, numbX and noises have to be initialized
        /// </remarks>
        public static void CreateData()
        {
            CreateNames();
            CreateXNoise();
            CreateYNoise();
            CreateX();
            CreateY();
            AddNoiseToX();
            AddNoiseToY();
        }

        /// <summary>
        /// Create standart names for X and Y matrices
        /// </summary>
        private static void CreateNames()
        {
            xValsNames = new string[numbX];
            for (int i = 0; i < numbX; i++)
                xValsNames[i] = $"X{i}";

            yValName = "Y";
        }

        #region Noise creation

        /// <summary>
        /// Create noise for X values based XNoise
        /// </summary>
        private static void CreateXNoise()
        {
            rand = new Random();

            switch (noiseX)
            {
                case NoiseTypes.None:
                    XNoise = Matrix<double>.Build.Dense(tests, numbX);
                    break;
                case NoiseTypes.White:
                    XNoise = Matrix<double>.Build.Dense(tests, numbX, (i, j) => rand.NextDouble() * 2 - 1); // / 5 - 0.1
                    XNoise.Column(0).SetValues(Vector<double>.Build.Dense(tests, (i) => 0).ToArray());
                    break;
                case NoiseTypes.Colorful:
                    XNoise = Matrix<double>.Build.Dense(tests, numbX);
                    break;
                default:
                    XNoise = Matrix<double>.Build.Dense(tests, numbX);
                    break;
            }
        }

        /// <summary>
        /// Create noise for Y values based YNoise
        /// </summary>
        private static void CreateYNoise()
        {
            rand = new Random();

            switch (noiseY)
            {
                case NoiseTypes.None:
                    YNoise = Vector<double>.Build.Dense(tests);
                    break;
                case NoiseTypes.White:
                    YNoise = Vector<double>.Build.Dense(tests, (i) => rand.NextDouble() * 2 - 1);
                    break;
                case NoiseTypes.Colorful:
                    YNoise = Vector<double>.Build.Dense(tests);
                    break;
                default:
                    YNoise = Vector<double>.Build.Dense(tests);
                    break;
            }
        }

        #endregion


        /// <summary>
        /// Create X matrix
        /// </summary>
        /// <remarks>
        /// x_ij = sin(ω*i + φ) + ε,
        /// first column have to be filled with 1
        /// </remarks>
        private static void CreateX()
        {
            var alpha = 30;
            var T = 0.360;
            var w = 2 * Constants.Pi / T;

            X = Matrix<double>.Build.Dense(tests, numbX, (i, j) => Math.Sin(w * i + alpha * (j - 1)));
            X.SetColumn(0, Vector<double>.Build.Dense(tests, (i) => 1));
        }

        /// <summary>
        /// Create Y vector from X data
        /// </summary>
        /// <remarks>
        /// Y = β0 + β1*x1 + β2*x2 + … + βn*xn
        /// </remarks>
        private static void CreateY()
        {
            Y = Vector<double>.Build.Dense(tests);

            for (int i = 0; i < Y.Count; i++)
            {
                double sum = 0;

                for (int j = 0; j < X.ColumnCount; j++)
                    sum += Beta[j] * X[i, j];

                Y[i] = sum;
            }
        }

        /// <summary>
        /// Add generated noise/error to every value of X matrix
        /// </summary>
        private static void AddNoiseToX()
        {
            if (noiseX == NoiseTypes.None)
                return;

            for (int i = 0; i < X.RowCount; i++)
                for (int j = 0; j < X.ColumnCount; j++)
                    X[i, j] += XNoise[i, j];
        }

        /// <summary>
        /// Add generated noise/error to every value of Y matrix
        /// </summary>
        private static void AddNoiseToY()
        {
            if (noiseY == NoiseTypes.None)
                return;

            for (int i = 0; i < Y.Count; i++)
                Y[i] += YNoise[i];
        }
    }
}
