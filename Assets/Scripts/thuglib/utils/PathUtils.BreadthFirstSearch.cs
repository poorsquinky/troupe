using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public partial class PathUtils
    {
        public delegate bool CanStep(int x, int y);

        public delegate int StepCost(int x, int y);

        public delegate int DirectedStepCost(int x, int y, eStep direction);

        private static int[][] bestCost;

        private static eStep[][] visitedFrom;

        // returns a Path if there is one or null if not
        // default to allow diagonal steps
        public static Path BFSPath(int xStart, int yStart, int xEnd, int yEnd,
           CanStep stepFrom, CanStep stepTo, StepCost costFrom,
           DirectedStepCost costTo, MapRectangle limits)
        {
            return BFSPath(xStart, yStart, xEnd, yEnd, stepFrom, stepTo,
               costFrom, costTo, limits, true);
        }

        // returns a Path if there is one or null if not
        public static Path BFSPath(int xStart, int yStart, int xEnd, int yEnd,
           CanStep stepFrom, CanStep stepTo, StepCost costFrom,
           DirectedStepCost costTo, MapRectangle limits, bool allowDiagonalSteps)
        {
            bool debug = false;
            if (debug)
            {
                Console.WriteLine("BFS start");
            }

            if (bestCost == null || bestCost.Length < limits.w)
            {
                bestCost = new int[limits.w][];
            }
            if (bestCost[0] == null || bestCost[0].Length < limits.h)
            {
                for (int x = 0; x < limits.w; x++)
                {
                    bestCost[x] = new int[limits.h];
                }
            }

            // update bestCost to hold the Dijkstra map
            CalculateDijkstraMap(xStart, yStart, stepFrom, stepTo, costFrom,
               costTo, bestCost, limits, -1, allowDiagonalSteps);

            if (bestCost[xEnd - limits.x][yEnd - limits.y] != -1)
            {
                // reverse pass and path gen
                int x = xEnd, y = yEnd;
                List<eStep> steps = new List<eStep>();
                while (x != xStart || y != yStart)
                {
                    eStep backStep = visitedFrom[x - limits.x][y - limits.y];
                    steps.Add(backStep);
                    int dx = StepDX[(int)backStep];
                    int dy = StepDY[(int)backStep];
                    x += dx;
                    y += dy;
                }
                Path solution = new Path(new eStep[steps.Count]);
                for (int i = 0; i < steps.Count; i++)
                {
                    solution.Steps[i] = ReverseStep[(int)steps[steps.Count - i
                       - 1]];
                }
                return solution;
            }
            else
            {
                return null; // there is no path
            }
        }

        public static void CalculateDijkstraMap(int xStart, int yStart,
           CanStep stepFrom, CanStep stepTo, StepCost costFrom,
           DirectedStepCost costTo, int[][] dijkstraMap, MapRectangle limits,
           int unreachedValue)
        {
            CalculateDijkstraMap(xStart, yStart, stepFrom, stepTo, costFrom,
               costTo, dijkstraMap, limits, unreachedValue, true);
        }

        public static void CalculateDijkstraMap(int xStart, int yStart,
           CanStep stepFrom, CanStep stepTo, StepCost costFrom,
           DirectedStepCost costTo, int[][] dijkstraMap, MapRectangle limits,
           int unreachedValue, bool allowDiagonalSteps)
        {
            for (int i = limits.x; i <= limits.x2; i++)
            {
                for (int j = limits.y; j <= limits.y2; j++)
                {
                    dijkstraMap[i][j] = unreachedValue;
                }
            }

            bool debug = false;
            if (debug)
            {
                Console.WriteLine("Dijkstra map start");
            }

            // update visitedFrom to hold the back steps
            if (visitedFrom == null || visitedFrom.Length < limits.w)
            {
                visitedFrom = new eStep[limits.w][];
            }
            if (visitedFrom[0] == null || visitedFrom[0].Length < limits.h)
            {
                for (int x = 0; x < limits.w; x++)
                {
                    visitedFrom[x] = new eStep[limits.h];
                }
            }

            // clear dijkstraMap to -1 = unvisited
            for (int x = 0; x < limits.w; x++)
            {
                for (int y = 0; y < limits.h; y++)
                {
                    dijkstraMap[x][y] = -1;
                }
            }

            // forward pass
            List<int> frontX = new List<int>();
            List<int> frontY = new List<int>();
            List<int> newFrontX = new List<int>();
            List<int> newFrontY = new List<int>();
            frontX.Add(xStart);
            frontY.Add(yStart);
            dijkstraMap[xStart - limits.x][yStart - limits.y] = 0;
            bool done = false;
            while (!done)
            {
                if (debug)
                {
                    Console.WriteLine("BFS new iteration, front size = " +
                       frontX.Count);
                }
                newFrontX.Clear();
                newFrontY.Clear();
                done = true;
                for (int i = 0; i < frontX.Count; i++)
                {
                    int baseCost = dijkstraMap[frontX[i] - limits.x][
                       frontY[i] - limits.y];
                    if (costFrom != null)
                    {
                        baseCost += costFrom(frontX[i], frontY[i]);
                    }
                    if (stepFrom == null || stepFrom(frontX[i], frontY[i]))
                    {
                        for (int j = 0; j < 8; j += (allowDiagonalSteps ? 1 : 2))
                        {
                            int xNew = frontX[i] + StepDX[j];
                            int yNew = frontY[i] + StepDY[j];
                            if (debug) 
                            {
                                Console.WriteLine("   attempt step from " +
                                   frontX[i] + ", " + frontY[i] + " dir " + j +
                                   " delta = " + StepDX[j] + ", " + StepDY[j] +
                                   " to " + xNew + ", " + yNew);
                            }
                            if (xNew >= limits.x && yNew >= limits.y &&
                               xNew <= limits.x2 && yNew <= limits.y2 &&
                               (stepTo == null || stepTo(xNew, yNew)))
                            {
                                int newCost = baseCost;
                                if (costTo != null)
                                {
                                    newCost += costTo(xNew, yNew, (eStep)j);
                                }
                                int dx = xNew - limits.x;
                                int dy = yNew - limits.y;
                                int currentCost = dijkstraMap[dx][dy];
                                if (currentCost == -1 ||
                                   currentCost > newCost)
                                {
                                    dijkstraMap[dx][dy] = newCost;
                                    visitedFrom[dx][dy] = ReverseStep[
                                       (int)j];
                                    newFrontX.Add(xNew);
                                    newFrontY.Add(yNew);
                                    if (debug)
                                    {
                                        Console.WriteLine("  step to " + xNew +
                                           ", " + yNew + " new cost = " +
                                           newCost);
                                    }
                                    done = false;
                                }
                            }
                        }
                    }
                }
                if (!done)
                {
                    List<int> swap = newFrontX;
                    newFrontX = frontX;
                    frontX = swap;
                    swap = newFrontY;
                    newFrontY = frontY;
                    frontY = swap;
                }
            }
        }
    }
}
