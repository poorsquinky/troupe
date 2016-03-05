using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class OpenSimplexHeightmapGenerator : MapGenerator
    {
        // smallest number of map squares that can support a bump
        private int minVarianceScale;

        // largest number of map squares over which a single bump
        // can exist
        private int maxVarianceScale;

        // determines how rough the finer-scale terrain is.  1.5 or 2
        // is probably realistic but 1.0 makes more lakes
        private double fractalExponent;

        // since the heightmap is inherently double-valued, we need to
        // convert it to fixed point before sticking it in int.  Multiply
        // the doubles by this, then round to convert
        private double doubleToIntFactor;

        // sea level, as a fraction from lowest possible height to 
        // highest possible height (0 = no sea, 1 = all sea); sea is identified
        // by being height zero
        private double seaLevel;

        public OpenSimplexHeightmapGenerator(int[] pixelTypes,
           MapCoordinate coordinate, int minVarianceScale, int
           maxVarianceScale, double fractalExponent = 1.0, double 
           doubleToIntFactor = 10000.0, double seaLevel = 0.5) :
           base(pixelTypes, coordinate)
        {
            this.minVarianceScale = minVarianceScale;
            this.maxVarianceScale = maxVarianceScale;
            this.fractalExponent = fractalExponent;
            this.doubleToIntFactor = doubleToIntFactor;
            this.seaLevel = seaLevel;
            InitOpenSimplex();
        }

        // pixel types: 
        //    not used; pixel value = height
        // custom params:
        //    none
        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            for (int i = fillRegion.x; i <= fillRegion.x2; i++)
            {
                for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                {
                    map[i][j] = 0;
                }
            }
            double maxHeight = 0.0;
            for (long scale = minVarianceScale; scale <= maxVarianceScale;
               scale *= 2)
            {
                InitOpenSimplexNoise((scale << 32) + coordinate.guid);
                double heightScale = Math.Pow((double)scale, fractalExponent);
                double heightFactor = heightScale * doubleToIntFactor;
                maxHeight += heightScale;
                double oneOverScale = 1.0 / ((double)scale);
                for (int i = fillRegion.x; i <= fillRegion.x2; i++)
                {
                    double iOverScale = i * oneOverScale;
                    double jOverScale = fillRegion.y * oneOverScale;
                    for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                    {
                        map[i][j] += MathUtils.RoundToInt(
                           Evaluate(iOverScale, jOverScale) * heightFactor);
                        jOverScale += oneOverScale;
                    }
                }
            }

            // now apply the sea level
            int seaLevelOffset = MathUtils.RoundToInt(doubleToIntFactor *
                (seaLevel - 0.5) * 2 * maxHeight);
            for (int i = fillRegion.x; i <= fillRegion.x2; i++)
            {
                for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                {
                    map[i][j] += seaLevelOffset;
                    if (map[i][j] < 0) map[i][j] = 0;
                }
            }

            return new List<MapRoom>(new MapRoom[0]);
        }

        private const double STRETCH_2D = -0.211324865405187;    //(1/Math.sqrt(2+1)-1)/2;
        private const double SQUISH_2D = 0.366025403784439;      //(Math.sqrt(2+1)-1)/2;
        private const double NORM_2D = 1.0 / 47.0;

        private byte[] perm;
        private byte[] perm2D;

        private double[] gradients2D = new double[]
        {
           5,  2,    2,  5,
          -5,  2,   -2,  5,
           5, -2,    2, -5,
          -5, -2,   -2, -5,
        };

        private Contribution2[] lookup2D;

        private void InitOpenSimplex()
        {
            var base2D = new int[][]
            {
                new int[] { 1, 1, 0, 1, 0, 1, 0, 0, 0 },
                new int[] { 1, 1, 0, 1, 0, 1, 2, 1, 1 }
            };
            var p2D = new int[] { 0, 0, 1, -1, 0, 0, -1, 1, 0, 2, 1,
               1, 1, 2, 2, 0, 1, 2, 0, 2, 1, 0, 0, 0 };
            var lookupPairs2D = new int[] { 0, 1, 1, 0, 4, 1, 17, 0, 20,
               2, 21, 2, 22, 5, 23, 5, 26, 4, 39, 3, 42, 4, 43, 3 };

            var contributions2D = new Contribution2[p2D.Length / 4];
            for (int i = 0; i < p2D.Length; i += 4)
            {
                var baseSet = base2D[p2D[i]];
                Contribution2 previous = null, current = null;
                for (int k = 0; k < baseSet.Length; k += 3)
                {
                    current = new Contribution2(baseSet[k], baseSet[k + 1], baseSet[k + 2]);
                    if (previous == null)
                    {
                        contributions2D[i / 4] = current;
                    }
                    else
                    {
                        previous.Next = current;
                    }
                    previous = current;
                }
                current.Next = new Contribution2(p2D[i + 1], p2D[i + 2], p2D[i + 3]);
            }

            lookup2D = new Contribution2[64];
            for (var i = 0; i < lookupPairs2D.Length; i += 2)
            {
                lookup2D[lookupPairs2D[i]] = contributions2D[lookupPairs2D[i + 1]];
            }
        }

        private static int FastFloor(double x)
        {
            var xi = (int)x;
            return x < xi ? xi - 1 : xi;
        }

        private void InitOpenSimplexNoise(long seed)
        {
            perm = new byte[256];
            perm2D = new byte[256];
            var source = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                source[i] = (byte)i;
            }
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            for (int i = 255; i >= 0; i--)
            {
                seed = seed * 6364136223846793005L + 1442695040888963407L;
                int r = (int)((seed + 31) % (i + 1));
                if (r < 0)
                {
                    r += (i + 1);
                }
                perm[i] = source[r];
                perm2D[i] = (byte)(perm[i] & 0x0E);
                source[r] = source[i];
            }
        }

        private double Evaluate(double x, double y)
        {
            var stretchOffset = (x + y) * STRETCH_2D;
            var xs = x + stretchOffset;
            var ys = y + stretchOffset;

            var xsb = FastFloor(xs);
            var ysb = FastFloor(ys);

            var squishOffset = (xsb + ysb) * SQUISH_2D;
            var dx0 = x - (xsb + squishOffset);
            var dy0 = y - (ysb + squishOffset);

            var xins = xs - xsb;
            var yins = ys - ysb;

            var inSum = xins + yins;

            var hash =
               (int)(xins - yins + 1) |
               (int)(inSum) << 1 |
               (int)(inSum + yins) << 2 |
               (int)(inSum + xins) << 4;

            var c = lookup2D[hash];

            var value = 0.0;
            while (c != null)
            {
                var dx = dx0 + c.dx;
                var dy = dy0 + c.dy;
                var attn = 2 - dx * dx - dy * dy;
                if (attn > 0)
                {
                    var px = xsb + c.xsb;
                    var py = ysb + c.ysb;

                    var i = perm2D[(perm[px & 0xFF] + py) & 0xFF];
                    var valuePart = gradients2D[i] * dx + gradients2D[i + 1] * dy;

                    attn *= attn;
                    value += attn * attn * valuePart;
                }
                c = c.Next;
            }
            return value * NORM_2D;
        }

        private class Contribution2
        {
            public double dx, dy;
            public int xsb, ysb;
            public Contribution2 Next;

            public Contribution2(double multiplier, int xsb, int ysb)
            {
                dx = -xsb - multiplier * SQUISH_2D;
                dy = -ysb - multiplier * SQUISH_2D;
                this.xsb = xsb;
                this.ysb = ysb;
            }
        }
    }
}
