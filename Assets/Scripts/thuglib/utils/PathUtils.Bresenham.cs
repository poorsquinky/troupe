using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public partial class PathUtils
    {
        // based on the Wikipedia implementation; will reuse fillPath to
        // save allocations if possible.  Null fillPath is ok.
        public static Path GetBresenhamPath(int x1, int y1, int x2, int y2,
           Path fillPath)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            // handle the degenerate case
            if (dx == 0 && dy == 0)
            {
                return new Path(new eStep[0]);
            }

            // move the problem to the first octant
            eStep diagonalStep, straightStep;
            if (dx < 0)
            {
                dx = -dx;
                if (dy < 0)
                {
                    dy = -dy;
                    if (dx >= dy)
                    {
                        diagonalStep = eStep.Southwest;
                        straightStep = eStep.West;
                    }
                    else
                    {
                        MathUtils.Swap(ref dx, ref dy);
                        diagonalStep = eStep.Southwest;
                        straightStep = eStep.South;
                    }
                }
                else
                {
                    if (dx >= dy)
                    {
                        diagonalStep = eStep.Northwest;
                        straightStep = eStep.West;
                    }
                    else
                    {
                        MathUtils.Swap(ref dx, ref dy);
                        diagonalStep = eStep.Northwest;
                        straightStep = eStep.North;
                    }
                }
            }
            else
            {
                if (dy < 0)
                {
                    dy = -dy;
                    if (dx >= dy)
                    {
                        diagonalStep = eStep.Southeast;
                        straightStep = eStep.East;
                    }
                    else
                    {
                        MathUtils.Swap(ref dx, ref dy);
                        diagonalStep = eStep.Southeast;
                        straightStep = eStep.South;
                    }
                }
                else
                {
                    if (dx >= dy)
                    {
                        diagonalStep = eStep.Northeast;
                        straightStep = eStep.East;
                    }
                    else
                    {
                        MathUtils.Swap(ref dx, ref dy);
                        diagonalStep = eStep.Northeast;
                        straightStep = eStep.North;
                    }
                }
            }

            // allocate results
            if (fillPath == null)
            {
                fillPath = new Path(new eStep[dx]);
            }
            else if (fillPath.Steps == null || fillPath.Steps.Length < dx)
            {
                fillPath.Steps = new eStep[dx];
                fillPath.NSteps = dx;
            }
            else
            {
                fillPath.NSteps = dx;
            }

            // run the first-octant Bresenham line algorithm
            int two_dy = dy + dy;
            int two_dx = dx + dx;
            int D = two_dy - dx;
            for (int x = 0; x < dx; x++)
            {
                if (D > 0)
                {
                    D -= two_dx;
                    fillPath.Steps[x] = diagonalStep;
                }
                else
                {
                    fillPath.Steps[x] = straightStep;
                }
                D += two_dy;
            }
            return fillPath;
        }

        public delegate int UpdateProductForSquare(int product, object square);

        public static int CalculateBresenhamProductSquareToSquare(int x1,
           int y1, int x2, int y2, object[][] map, UpdateProductForSquare update,
           int startingProduct)
        {
            return CalculateBresenhamProductSquareToSquare(x1, y1, x2, y2, map,
               update, startingProduct, false, true);
        }

        public static int CalculateBresenhamProductSquareToSquare(int x1,
           int y1, int x2, int y2, object[][] map, UpdateProductForSquare update,
           int startingProduct, bool includeFirstSquare, bool includeLastSquare)
        {
            x1 = MathUtils.Limit(x1, 0, map.Length - 1);
            y1 = MathUtils.Limit(y1, 0, map[0].Length - 1);
            x2 = MathUtils.Limit(x2, 0, map.Length - 1);
            y2 = MathUtils.Limit(y2, 0, map[0].Length - 1);
            Path path = GetBresenhamPath(x1, y1, x2, y2, null);
            int product = startingProduct;
            int x = x1, y = y1;
            for (int i = 0; i < path.NSteps; i++)
            {
                if (i != 0 || includeFirstSquare)
                {
                    product = update(product, map[x][y]);
                }
                x += StepDX[(int)path.Steps[i]];
                y += StepDY[(int)path.Steps[i]];
            }
            if (includeLastSquare)
            {
                product = update(product, map[x][y]);
            }
            return product;
        }

        public delegate int UpdateProductForSquareWithCoords(int product,
           object square, int x, int y);

        public static int CalculateBresenhamProductSquareToSquare(int x1,
           int y1, int x2, int y2, object[][] map, UpdateProductForSquareWithCoords
           update, int startingProduct)
        {
            return CalculateBresenhamProductSquareToSquare(x1, y1, x2, y2, map,
               update, startingProduct, false, true);
        }

        public static int CalculateBresenhamProductSquareToSquare(int x1,
           int y1, int x2, int y2, object[][] map, UpdateProductForSquareWithCoords
           update, int startingProduct, bool includeFirstSquare,
           bool includeLastSquare)
        {
            x1 = MathUtils.Limit(x1, 0, map.Length - 1);
            y1 = MathUtils.Limit(y1, 0, map[0].Length - 1);
            x2 = MathUtils.Limit(x2, 0, map.Length - 1);
            y2 = MathUtils.Limit(y2, 0, map[0].Length - 1);
            Path path = GetBresenhamPath(x1, y1, x2, y2, null);
            int product = startingProduct;
            int x = x1, y = y1;
            for (int i = 0; i < path.NSteps; i++)
            {
                if (i != 0 || includeFirstSquare)
                {
                    product = update(product, map[x][y], x, y);
                }
                x += StepDX[(int)path.Steps[i]];
                y += StepDY[(int)path.Steps[i]];
            }
            if (includeLastSquare)
            {
                product = update(product, map[x][y], x, y);
            }
            return product;
        }

        public static void CalculateBresenhamProductsToRectangle(int fromX, int
           fromY, object[][] map, MapRectangle rectangle, UpdateProductForSquare
           update, int startingProduct, int[][] outputMap)
        {
            CalculateBresenhamProductsToRectangle(fromX, fromY, map,
               rectangle, update, startingProduct, false, true, outputMap);
        }

        public static void CalculateBresenhamProductsToRectangle(int fromX, int
           fromY, object[][] map, MapRectangle rectangle, UpdateProductForSquare
           update, int startingProduct, bool includeFirstSquare, bool
           includeLastSquare, int[][] outputMap)
        {
            // top and bottom edges
            for (int x = rectangle.x; x <= rectangle.x2; x++)
            {
                outputMap[fromX][fromY] = startingProduct;
                CalculateBresenhamProductSquareToSquare(fromX, fromY,
                   x, rectangle.y, map,
                   (previous, mapval, xs, ys) => {
                      outputMap[xs][ys] = update(previous, mapval);
                      return outputMap[xs][ys];
                   }, startingProduct, includeFirstSquare, includeLastSquare);
                outputMap[fromX][fromY] = startingProduct;
                CalculateBresenhamProductSquareToSquare(fromX, fromY,
                   x, rectangle.y2, map,
                   (previous, mapval, xs, ys) => {
                      outputMap[xs][ys] = update(previous, mapval);
                      return outputMap[xs][ys];
                   }, startingProduct, includeFirstSquare, includeLastSquare);
            }

            // right and left edges
            for (int y = rectangle.y + 1; y <= rectangle.y2 - 1; y++)
            {
                outputMap[fromX][fromY] = startingProduct;
                CalculateBresenhamProductSquareToSquare(fromX, fromY,
                   rectangle.x, y, map,
                   (previous, mapval, xs, ys) => {
                      outputMap[xs][ys] = update(previous, mapval);
                      return outputMap[xs][ys];
                   }, startingProduct, includeFirstSquare, includeLastSquare);
                outputMap[fromX][fromY] = startingProduct;
                CalculateBresenhamProductSquareToSquare(fromX, fromY,
                   rectangle.x2, y, map,
                   (previous, mapval, xs, ys) => {
                      outputMap[xs][ys] = update(previous, mapval);
                      return outputMap[xs][ys];
                   }, startingProduct, includeFirstSquare, includeLastSquare);
            }
        }
    }
}
