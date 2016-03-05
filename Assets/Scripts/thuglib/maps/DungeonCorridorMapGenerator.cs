using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class DungeonCorridorMapGenerator : MapGenerator
    {
        private int turnCostPenalty;

        private int[] intoPixelCostPenalty;

        public DungeonCorridorMapGenerator(int[] pixelTypes, MapCoordinate
           coordinate, int turnCostPenalty, int[] intoPixelCostPenalty) :
           base(pixelTypes, coordinate)
        {
            this.turnCostPenalty = turnCostPenalty;
            this.intoPixelCostPenalty = intoPixelCostPenalty;
        }

        // pixel types: 
        //    [0] = wall
        //    [1] = floor
        //    [2] = door
        //    >2  = other sorts of wall
        //    not on list = impenetrable
        // custom params:
        //    none; skips rooms with doNotConnect set to true
        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            bool debug = false;
            if (debug) 
            {
                Console.WriteLine("DCMG start, " + roomsToInclude.Count +
                   " rooms");
            }
            int corridorPixelType = 1234567;

            // pass 1 : get all the rooms connected to each other

            List<int> alreadyConnectedRoomIndices = new List<int>();
            for (int i = 0; (i < roomsToInclude.Count) &&
               (alreadyConnectedRoomIndices.Count < 1); i++)
            {
                if (!roomsToInclude[i].doNotConnect)
                {
                    alreadyConnectedRoomIndices.Add(i);
                    if (debug)
                    {
                        Console.WriteLine("DCMG to connect room " + i);
                    }
                }
            }

            if (alreadyConnectedRoomIndices.Count < 1)
            {
                // none of the rooms want to be hooked up to each other
                return roomsToInclude;
            }

            // distanceMap = -1 for impassable squares
            int[][] distanceMap = Allocate2DIntArray(fillRegion.w,
               fillRegion.h);
            // fromDirectionMap = -1 for unvisited, 0 1 2 3 = from N S E W
            int[][] fromDirectionMap = Allocate2DIntArray(fillRegion.w,
               fillRegion.h);
            // set true for the goal squares of a search
            bool[][] goalMap = Allocate2DBoolArray(fillRegion.w,
               fillRegion.h);
            // > 0 for squares that are expensive to step into
            int[][] extraCostMap = Allocate2DIntArray(fillRegion.w,
               fillRegion.h);
            if (intoPixelCostPenalty != null)
            {
                for (int i = 0; i < fillRegion.w; i++)
                {
                    for (int j = 0; j < fillRegion.h; j++)
                    {
                        int pixel = map[fillRegion.x + i][fillRegion.y + j];
                        if (pixel >= 0 && pixel < intoPixelCostPenalty.Length)
                        {
                            extraCostMap[i][j] = intoPixelCostPenalty[pixel];
                        }
                    }
                }
            }

            for (int i = alreadyConnectedRoomIndices[0] + 1;
               i < roomsToInclude.Count; i++)
            {
                MapRoom thisRoom = roomsToInclude[i];
                if (!thisRoom.doNotConnect)
                {
                    if (debug)
                    {
                        Console.WriteLine("DCMG try to connect " + i + 
                           " to net");
                    }
                    int bestDoorIndex = -1;
                    int bestDoorDistanceSquared = 0;
                    int bestTargetRoomIndex = -1;
                    for (int j = 0; j < thisRoom.doors.Count; j++)
                    {
                        for (int k = 0; k < alreadyConnectedRoomIndices.Count;
                           k++)
                        {
                            int testRoomIndex = alreadyConnectedRoomIndices[k];
                            MapRoom r2 = roomsToInclude[testRoomIndex];
                            int dx = thisRoom.doors[j].x - r2.bounds.xCenter;
                            int dy = thisRoom.doors[j].y - r2.bounds.yCenter;
                            int testDistanceSquared = dx * dx + dy * dy;
                            if (bestTargetRoomIndex == -1 ||
                               testDistanceSquared < bestDoorDistanceSquared)
                            {
                                bestTargetRoomIndex = testRoomIndex;
                                bestDoorIndex = j;
                                bestDoorDistanceSquared = testDistanceSquared;
                                if (debug)
                                {
                                    Console.WriteLine("   new closest door is "
                                       + j + " to room " + testRoomIndex +
                                       " from room " + i);
                                }
                            }
                        }
                    }
                    if (bestDoorIndex != -1)
                    {
                        // run a BFS corridor from door bestDoor of 
                        // roomsToInclude[i] to any existing corridor or 
                        // any door of roomsToInclude[bestTargetRoomIndex]

                        Clear2DIntArray(fromDirectionMap, -1); // unvisited

                        // set all of the rooms interiors and walls to 
                        // impassable

                        Clear2DIntArray(distanceMap, 100000000);
                        foreach (MapRoom r in roomsToInclude)
                        {
                            for (int j = 1; j < r.bounds.w - 1; j++)
                            {
                                int dx = r.bounds.x + j - fillRegion.x;
                                for (int k = 1; k < r.bounds.h - 1; k++)
                                {
                                    int dy = r.bounds.y + k - fillRegion.y;
                                    distanceMap[dx][dy] = -1;
                                }
                            }
                        }

                        // set up the goals

                        Clear2DBoolArray(goalMap);
                        MapRoom targetRoom = roomsToInclude[
                           bestTargetRoomIndex];
                        for (int j = 1; j < targetRoom.bounds.w - 1; j++)
                        {
                            for (int k = 1; k < targetRoom.bounds.h - 1; k++)
                            {
                                goalMap[j + targetRoom.bounds.x - fillRegion.x][
                                   k + targetRoom.bounds.y - fillRegion.y] = 
                                   true;
                            }
                        }
                        for (int j = 0; j < targetRoom.doors.Count; j++)
                        {
                            int dx = targetRoom.doors[j].x - fillRegion.x;
                            int dy = targetRoom.doors[j].y - fillRegion.y;
                            goalMap[dx][dy] = true;
                            distanceMap[dx][dy] = 1000000;
                        }

                        // also load the existing cooridors into the goal map
                        for (int j = 0; j < fillRegion.w; j++)
                        {
                            for (int k = 0; k < fillRegion.h; k++)
                            {
                                if (map[fillRegion.x + j][fillRegion.y + k] ==
                                   corridorPixelType)
                                {
                                    goalMap[j][k] = true;
                                }
                            }
                        }

                        bool success = ExtendBFSCorridor(goalMap, distanceMap,
                           extraCostMap, fromDirectionMap,
                           thisRoom.doors[bestDoorIndex].x - fillRegion.x,
                           thisRoom.doors[bestDoorIndex].y - fillRegion.y,
                           map, fillRegion, corridorPixelType, debug);
                        if (success)
                        {
                            alreadyConnectedRoomIndices.Add(i);
                        }
                        if (debug)
                        {
                            Console.WriteLine("   ran route from door " +
                               bestDoorIndex + " success = " + success);
                        }
                    }
                }
            }

            // convert corridorPixelType to pixelTypes[1]

            SearchAndReplace2DIntArray(map, corridorPixelType, pixelTypes[1]);

            return roomsToInclude;
        }

        private int[] step_dx = new int[] {0, 1, 0, -1};

        private int[] step_dy = new int[] {-1, 0, 1, 0};

        private int[] inverseDirection = new int[] {2, 3, 0, 1};

        private bool ExtendBFSCorridor(bool[][] isGoal, int[][] bestDistance,
           int[][] extraCost, int[][] fromDirection, int x0, int y0, 
           int[][] map, MapRectangle fillRegion, int open, bool debug)
        {
            bool debugVerbose = false;
            if (debug)
            {
                Console.WriteLine("BFS map with goal (O) and start (S) marked");
                for (int i = 0; i < fillRegion.h; i++)
                {
                    for (int j = 0; j < fillRegion.w; j++) 
                    {
                        if (i == y0 && j == x0)
                        {
                            Console.Write("O");
                        }
                        else if (bestDistance[i][j] == -1)
                        {
                            Console.Write("X");
                        }
                        else if (isGoal[j][i])
                        {
                            Console.Write("$");
                        }
                        else
                        {
                            Console.Write(".");
                        }
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("");
            }
            List<int> frontX = new List<int>();
            List<int> frontY = new List<int>();
            List<int> newFrontX = new List<int>();
            List<int> newFrontY = new List<int>();
            frontX.Add(x0);
            frontY.Add(y0);
            bestDistance[x0][y0] = 0;
            int bestGoalX = 0;
            int bestGoalY = 0;
            int bestGoalDistance = -1;
            bool done = true;
            do
            {
                // expand the existing front
                done = true;
                for (int i = 0; i < frontX.Count; i++)
                {
                    if (debugVerbose)
                    {
                        Console.WriteLine("BFS: step from " + 
                           frontX[i] + ", " + frontY[i]);
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        int to_x = frontX[i] + step_dx[j];
                        int to_y = frontY[i] + step_dy[j];
                        if (debugVerbose)
                        {
                            Console.WriteLine("  test step to " + 
                               to_x + ", " + to_y);
                        }
                        if (to_x >= 0 && to_x < isGoal.Length &&
                           to_y >= 0 && to_y < isGoal[0].Length &&
                           bestDistance[to_x][to_y] != -1)
                        {
                            bool stepIsTurn = false;
                            if (frontX[i] != x0 || frontY[i] != y0)
                            {
                                stepIsTurn = (j != inverseDirection[
                                   fromDirection[frontX[i]][frontY[i]]]);
                            }
                            int newDistance = 
                               bestDistance[frontX[i]][frontY[i]] +
                               (stepIsTurn ? turnCostPenalty : 0) +
                               extraCost[to_x][to_y] + 1;
                            if (debugVerbose)
                            {
                                Console.WriteLine("  it's legit, turn = " +
                                   stepIsTurn + " cost = " + newDistance +
                                   " best cost so far = " + 
                                   bestDistance[to_x][to_y]);
                            }
                            if (newDistance < bestDistance[to_x][to_y])
                            {
                                done = false;
                                bestDistance[to_x][to_y] = newDistance;
                                fromDirection[to_x][to_y] = inverseDirection[j];
                                bool alreadyInNewFront = false;
                                if (isGoal[to_x][to_y])
                                {
                                    if (bestGoalDistance == -1 ||
                                       newDistance < bestGoalDistance)
                                    {
                                        bestGoalDistance = newDistance;
                                        bestGoalX = to_x;
                                        bestGoalY = to_y;
                                    }
                                }
                                for (int k = 0; k < newFrontX.Count; k++)
                                {
                                    if (newFrontX[k] == to_x &&
                                       newFrontY[k] == to_y)
                                    {
                                        alreadyInNewFront = true;
                                    }
                                }
                                if (!alreadyInNewFront)
                                {
                                    newFrontX.Add(to_x);
                                    newFrontY.Add(to_y);
                                }
                            }
                        }
                    }
                }

                if (!done)
                {
                    // swap the fronts if we are still advancing
                    List<int> swapFrontX = frontX;
                    List<int> swapFrontY = frontY;
                    frontX = newFrontX;
                    frontY = newFrontY;
                    newFrontX = swapFrontX;
                    newFrontY = swapFrontY;
                    newFrontX.Clear();
                    newFrontY.Clear();
                }
            }
            while (!done);

            if (debug)
            {
                Console.WriteLine("   BFS best goal distance = " + 
                   bestGoalDistance);
            }

            if (bestGoalDistance != -1)
            {
                // backward pass writing in the corridor
                int x = bestGoalX;
                int y = bestGoalY;
                while (x != x0 || y != y0)
                {
                    if (x != bestGoalX || y != bestGoalY)
                    {
                        map[fillRegion.x + x][fillRegion.y + y] = open;
                        if (debug)
                        {
                            Console.WriteLine("   corr at " + 
                               (fillRegion.x + x) + ", " + 
                               (fillRegion.y + y));
                        }
                    }
                    int fromDir = fromDirection[x][y];
                    x += step_dx[fromDir];
                    y += step_dy[fromDir];
                }
            }
            return (bestGoalDistance != -1);
        }
    }
}
