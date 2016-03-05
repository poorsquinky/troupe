using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class MathUtils
    {
        public static int RoundToInt(double d)
        {
            return (int)(d + 0.5);
        }

        public static void Swap(ref int a, ref int b)
        {
            int swap = a;
            a = b;
            b = swap;
        }

        public static void Swap(ref List<int> a, ref List<int> b)
        {
            List<int> swap = a;
            a = b;
            b = swap;
        }

        public static int Limit(int a, int low, int high)
        {
            if (a < low) a = low;
            if (a > high) a = high;
            return a;
        }

        public static int[][] Int2DArray(int w, int h, int v)
        {
            int[][] retval = new int[w][];
            for (int i = 0; i < w; i++)
            {
                retval[i] = new int[h];
                for (int j = 0; j < h; j++)
                {
                    retval[i][j] = v;
                }
            }
            return retval;
        }

        public static double DotProduct(double[] a, double[] b, int n)
        {
            double aDotB = 0.0;
            for (int i = 0; i < n; i++) aDotB += a[i] * b[i];
            return aDotB;
        }
#region Splines
        private static double[] gam;

        public static bool TridiagonalLinearSolve(double[] sub, 
           double[] diag, double[] super, double[] rhs, double[] soln, int n)
        {
            if (gam == null || gam.Length < n) gam = new double[n];
            if (diag[0] == 0) return false; // singular
            double bet = diag[0];
            soln[0] = rhs[0] / diag[0];
            for (int i = 1; i < n; i++)
            {
                gam[i] = super[i - 1] / bet;
                bet = diag[i] - sub[i] * gam[i];
                if (bet == 0) return false; // singular
                soln[i] = (rhs[i] - sub[i] * soln[i - 1]) / bet;
            }
            for (int i = n - 2; i >= 0; i--)
            {
                soln[i] -= gam[i + 1] * soln[i + 1];
            }
            return true;
        }

        private static double[] yCyc;

        private static double[] zCyc;

        // matrix is Tridiag(sub, diag, super) + u (x) v
        public static bool ShermanMorrisonTridiagonalLinearSolve(double[] sub,
           double[] diag, double[] super, double[] u, double[] v,
           double[] rhs, double[] soln, int n)
        {
            if (yCyc == null || yCyc.Length < n) yCyc = new double[n];
            if (zCyc == null || zCyc.Length < n) zCyc = new double[n];
            if (!TridiagonalLinearSolve(sub, diag, super, rhs, yCyc, n)
               || !TridiagonalLinearSolve(sub, diag, super, u, zCyc, n))
            {
                return false;
            }
            double vDotZ = DotProduct(v, zCyc, n);
            double vDotY = DotProduct(v, yCyc, n);
            double factor = vDotY / (1.0 + vDotZ);
            for (int i = 0; i < n; i++)
            {
                soln[i] = yCyc[i] - factor * zCyc[i];
            }
            return true;
        }

        private static double[] splineRHSX;

        private static double[] splineRHSY;

        private static double[] splineDiag;

        private static double[] splineSub;

        private static double[] splineSuper;

        // note that there should be n + 1 x and y values
        public static bool NaturalSplineFit(double[] x, double[] y,
           double[] x2, double[] y2, int n)
        {
            if (splineRHSX == null || splineRHSX.Length < n)
            {
                splineRHSX = new double[n];
                splineRHSY = new double[n];
                splineDiag = new double[n];
                splineSub = new double[n];
                splineSuper = new double[n];
            }
            for (int i = 0; i < n; i++)
            {
                if (i == 0 || i == n - 1)
                {
                    splineDiag[i] = 1;
                    splineSub[i] = 0;
                    splineSuper[i] = 0;
                    splineRHSX[i] = 0;
                    splineRHSY[i] = 0;
                } 
                else
                {
                    splineDiag[i] = 4;
                    splineSub[i] = 1;
                    splineSuper[i] = 1;
                    splineRHSX[i] = 6 * (x[i + 1] + x[i - 1] - 2 * x[i]);
                    splineRHSY[i] = 6 * (y[i + 1] + y[i - 1] - 2 * y[i]);
                }
            }
            return (TridiagonalLinearSolve(splineSub, splineDiag, splineSuper,
               splineRHSX, x2, n) && TridiagonalLinearSolve(splineSub,
               splineDiag, splineSuper, splineRHSY, y2, n));
        }

        public const double ONE_SIXTH = 0.1666666666666666666;

        public const double ONE_HALF = 0.5;

        public static double NaturalSplineCellEvaluate(double[] x, double[] x2,
           int idx, double s)
        {
            double c0 = x[idx];
            double c1 = (x[idx + 1] - x[idx]) - ONE_SIXTH *
               (2 * x2[idx] + x2[idx + 1]);
            double c2 = ONE_HALF * x2[idx];
            double c3 = ONE_SIXTH * (x2[idx + 1] - x2[idx]);
            return ((c3 * s + c2) * s + c1) * s + c0;
        }

        public static void NaturalSplineEvaluate(double[] x, double[] y, 
           double[] x2, double[] y2, int n, double s, out double ex,
           out double ey)
        {
            int idx;
            if (s < 0)
            {
                idx = 0;
            }
            else if (s >= n - 2)
            {
                idx = n - 2;
            }
            else
            {
                idx = (int)s;
            }
            s -= idx;
            ex = NaturalSplineCellEvaluate(x, x2, idx, s);
            ey = NaturalSplineCellEvaluate(y, y2, idx, s);
        }

        private static double[] splineCyclicU;

        private static double[] splineCyclicV;

        public static bool CyclicSplineFit(double[] x, double[] y,
           double[] x2, double[] y2, int n)
        {
            if (splineRHSX == null || splineRHSX.Length < n)
            {
                splineRHSX = new double[n];
                splineRHSY = new double[n];
                splineDiag = new double[n];
                splineSub = new double[n];
                splineSuper = new double[n];
                splineCyclicU = new double[n];
                splineCyclicV = new double[n];
            }
            for (int i = 0; i < n; i++)
            {
                bool isEnd = (i == 0 || i == (n - 1));
                splineDiag[i] = 4 - (isEnd ? 1 : 0);
                splineSub[i] = 1;
                splineSuper[i] = 1;
                int iMinus = (i - 1 + n) % n;
                int iPlus = (i + 1) % n;
                splineRHSX[i] = 6 * (x[iPlus] + x[iMinus] - 2 * x[i]);
                splineRHSY[i] = 6 * (y[iPlus] + y[iMinus] - 2 * y[i]);
                splineCyclicU[i] = isEnd ? 1 : 0;
                splineCyclicV[i] = isEnd ? 1 : 0;
            }
            return ShermanMorrisonTridiagonalLinearSolve(splineSub,
               splineDiag, splineSuper, splineCyclicU, splineCyclicV,
               splineRHSX, x2, n) &&
               ShermanMorrisonTridiagonalLinearSolve(splineSub,
               splineDiag, splineSuper, splineCyclicU, splineCyclicV,
               splineRHSY, y2, n);
        }

        public static double CyclicSplineCellEvaluate(double[] x, double[] x2,
           int idx, int n, double s)
        {
            int idx2 = (idx + 1) % n;
            double c0 = x[idx];
            double c1 = (x[idx2] - x[idx]) - ONE_SIXTH *
               (2 * x2[idx] + x2[idx2]);
            double c2 = ONE_HALF * x2[idx];
            double c3 = ONE_SIXTH * (x2[idx2] - x2[idx]);
            return ((c3 * s + c2) * s + c1) * s + c0;
        }

        public static void CyclicSplineEvaluate(double[] x, double[] y, 
           double[] x2, double[] y2, int n, double s, out double ex,
           out double ey)
        {
            int idx;
            if (s < 0)
            {
                idx = 0;
            }
            else if (s >= n - 1)
            {
                idx = n - 1;
            }
            else
            {
                idx = (int)s;
            }
            s -= idx;
            ex = CyclicSplineCellEvaluate(x, x2, idx, n, s);
            ey = CyclicSplineCellEvaluate(y, y2, idx, n, s);
        }
#endregion

#region Testing Splines
        public static int ReadPointData(String filename, out double[] x,
           out double[] y)
        {
            List<double> xl = new List<double>();
            List<double> yl = new List<double>();
            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                char[] delim = new char[] { ' ', '\t', '\n' };
                while ((line = reader.ReadLine()) != null)
                {
                    String[] tokens = line.Split(delim,
                       StringSplitOptions.RemoveEmptyEntries);
                    xl.Add(Double.Parse(tokens[0]));
                    yl.Add(Double.Parse(tokens[1]));
                }
                reader.Close();
            }
            x = xl.ToArray();
            y = yl.ToArray();
            return x.Length;
        }

        // for command-line test of cyclic parametric spline
        public static void MainCyclic(String[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: mono MathUtils.exe <data file>");
                return;
            }
            double[] x, y;
            int n = ReadPointData(args[0], out x, out y);
            double[] x2 = new double[n];
            double[] y2 = new double[n];
            if (!CyclicSplineFit(x, y, x2, y2, x.Length))
            {
                Console.WriteLine("Spline fit failed!");
                return;
            }
            for (double s = -0.1; s <= n + 0.1; s += 0.01)
            {
                double ex, ey;
                CyclicSplineEvaluate(x, y, x2, y2, n, s, out ex, out ey);
                Console.WriteLine(ex + "   " + ey);
            }
        }

        // for command-line test of natural parametric spline
        public static void MainNatural(String[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: mono MathUtils.exe <data file>");
                return;
            }
            double[] x, y;
            int n = ReadPointData(args[0], out x, out y);
            double[] x2 = new double[n];
            double[] y2 = new double[n];
            if (!NaturalSplineFit(x, y, x2, y2, x.Length))
            {
                Console.WriteLine("Spline fit failed!");
                return;
            }
            for (double s = -0.1; s <= n - 0.9; s += 0.01)
            {
                double ex, ey;
                NaturalSplineEvaluate(x, y, x2, y2, n, s, out ex, out ey);
                Console.WriteLine(ex + "   " + ey);
            }
        }
#endregion
    }
}
