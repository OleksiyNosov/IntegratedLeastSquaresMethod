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
    /*
     * Determines which type of noise should be generated
     */
    public enum NoiseTypes
    {
        None = 0,
        White = 1,
        Colorful = 2
    }

    /*
     * Create data specially for LsData
     */
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

        /*
         * 1. Receives parameters
         * 2. Generate data
         * 3. Return LsData with fully filled LsParameters 
         */
        public static LsData Build(int numberOfXValues, int numberOfExperiments, double[] beta, NoiseTypes noiseX, NoiseTypes noiseY, double gamma = 0, double theta = 1)
        {
            numbX = numberOfXValues;
            tests = numberOfExperiments;
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
                    NumbOfTests = numberOfExperiments,
                    NumbOfXVal = numberOfXValues,
                    XValsNames = xValsNames,
                    YValName = yValName,
                    Gamma = gamma,
                    Theta = theta
                });
        }

        /*
         * Pre-condition: 
         *      tests, numbX and noises are initialized
         *      
         * Post-condition:
         *      Create all data for LsParameters
         */
        public static void CreateData()
        {
            CreateNames();
            CreateXNoise();
            CreateYNoise();
            CreateX();
            CreateY();
        }

        /*
         * Create standart names for values
         */
        private static void CreateNames()
        {
            xValsNames = new string[numbX];
            for (int i = 0; i < numbX; i++)
                xValsNames[i] = $"X{i}";

            yValName = "Y";
        }

        #region Noise creation
        /* 
         * Create custom noise for X and Y
         */

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
                    break;
                case NoiseTypes.Colorful:
                    XNoise = Matrix<double>.Build.Dense(tests, numbX);
                    break;
                default:
                    XNoise = Matrix<double>.Build.Dense(tests, numbX);
                    break;
            }
        }
        private static void CreateYNoise()
        {
            rand = new Random();

            switch (noiseY)
            {
                case NoiseTypes.None:
                    YNoise = Vector<double>.Build.Dense(tests);
                    break;
                case NoiseTypes.White:
                    YNoise = Vector<double>.Build.Dense(tests, (i) => rand.NextDouble() / 5 - 0.1);
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


        /* 
         * Create X matrix based on 
         * x_ij = sin(ω*i + φ) + ε
         * first column have to be filled with 1
         */
        private static void CreateX()
        {
            var alpha = 30;
            var T = 0.360;
            var w = 2 * Constants.Pi / T;

            X = Matrix<double>.Build.Dense(tests, numbX, (i, j) => Math.Sin(w * i + alpha * (j - 1)) + XNoise[i, j]);
            X.SetColumn(0, Vector<double>.Build.Dense(tests, (i) => 1 + XNoise[i, 0]));
        }
        /*
         * Create Y vector from X data
         * Y = β0 + β1*x1 + β2*x2 + … + βn*xn
         */
        private static void CreateY()
        {
            Y = Vector<double>.Build.Dense(tests);

            for (int i = 0; i < Y.Count; i++)
            {
                double sum = 0;

                for (int j = 0; j < X.ColumnCount; j++)
                    sum += Beta[j] * X[i, j] + YNoise[i];

                Y[i] = sum;
            }
        }
    }
}
