using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class CADecayMapGenerator : MapGenerator
    {
        private int seedCorridorAverageSpacing;

        private int seedCorridorAverageLength;

        private int numberOfDecays;

        private int decayPercentPerDecayedNeighbor;

        // pixel types: 
        //    [0] = wall (filled)
        //    [1] = floor (empty)
        // custom params:
        //    seedCorridorAverageSpacing - average x and y distance between
        //       parallel seed corridors
        //    seedCorridorAverageLength - average length of seed corridors;
        //       length from 50% to 150% of that.  Should be somewhat less than
        //       smallest map dim.  Seed corridors are truncated at inviolable
        //       rectangles
        //    numberOfDecays - number of CA decay steps to run
        //    decayPercentPerDecayedNeighbor - chance of a filled square 
        //       decaying to empty = # of decayed neighbors * this percent
        public CADecayMapGenerator(int[] pixelTypes,
           MapCoordinate coordinate,
           int seedCorridorAverageSpacing,
           int seedCorridorAverageLength,
           int numberOfDecays,
           int decayPercentPerDecayedNeighbor) : base(pixelTypes, coordinate)
        {
            this.seedCorridorAverageSpacing = seedCorridorAverageSpacing;
            this.seedCorridorAverageLength = seedCorridorAverageLength;
            this.numberOfDecays = numberOfDecays;
            this.decayPercentPerDecayedNeighbor = decayPercentPerDecayedNeighbor;
        }

        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            bool[][] pixelIsProtected = BuildProtectedMap(roomsToInclude,
               map.Length, map[0].Length);
          
            // clear to solid
            for (int i = fillRegion.x; i <= fillRegion.x2; i++)
            {
                for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                {
                    if (!pixelIsProtected[i][j])
                    {
                        map[i][j] = pixelTypes[0];
                    }
                }
            }

            // put in the horizontal seed corridors
            int n = fillRegion.h / seedCorridorAverageSpacing;
            for (int i = 0; i < n; i++)
            {
                int l = NextRandom(seedCorridorAverageLength / 2,
                   3 * seedCorridorAverageLength / 2 + 1);
                int offset = 0;
                if (l >= fillRegion.w)
                {
                    l = fillRegion.w;
                }
                else
                {
                    offset = NextRandom(0, fillRegion.w - l + 1);
                }
                int crossOffset = NextRandom(fillRegion.y, fillRegion.y2 + 1);
                for (int j = offset; j < offset + l; j++)
                {
                    if (!pixelIsProtected[j + fillRegion.x][crossOffset])
                    {
                        map[j + fillRegion.x][crossOffset] = pixelTypes[1];
                    }
                }
            }

            // put in the vertical seed corridors
            n = fillRegion.w / seedCorridorAverageSpacing;
            for (int i = 0; i < n; i++)
            {
                int l = NextRandom(seedCorridorAverageLength / 2,
                   3 * seedCorridorAverageLength / 2 + 1);
                int offset = 0;
                if (l >= fillRegion.h)
                {
                    l = fillRegion.h;
                }
                else
                {
                    offset = NextRandom(0, fillRegion.h - l + 1);
                }
                int crossOffset = NextRandom(fillRegion.x, fillRegion.x2 + 1);
                for (int j = offset; j < offset + l; j++)
                {
                    if (!pixelIsProtected[crossOffset][j + fillRegion.y])
                    {
                        map[crossOffset][j + fillRegion.y] = pixelTypes[1];
                    }
                }
            }

            for (int k = 0; k < numberOfDecays; k++)
            {
                for (int i = fillRegion.x; i <= fillRegion.x2; i++)
                {
                    for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                    {
                        if (map[i][j] == pixelTypes[0] && !pixelIsProtected[i][j])
                        {
                            int nNeighbors = 0;
                            if (i > 0)
                            {
                               if (j > 0)
                               {
                                   if (map[i - 1][j - 1] == pixelTypes[1])
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (j < map[i].Length - 1)
                               {
                                   if (map[i - 1][j + 1] == pixelTypes[1])
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (map[i - 1][j] == pixelTypes[1])
                               {
                                   nNeighbors++;
                               }
                            }
                            if (i < map.Length - 1)
                            {
                               if (j > 0)
                               {
                                   if (map[i + 1][j - 1] == pixelTypes[1])
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (j < map[i].Length - 1)
                               {
                                   if (map[i + 1][j + 1] == pixelTypes[1])
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (map[i + 1][j] == pixelTypes[1])
                               {
                                   nNeighbors++;
                               }
                            }
                            if (j > 0)
                            {
                                if (map[i][j - 1] == pixelTypes[1])
                                {
                                    nNeighbors++;
                                }
                            }
                            if (j < map[i].Length - 1)
                            {
                                if (map[i][j + 1] == pixelTypes[1])
                                {
                                    nNeighbors++;
                                }
                            }
                            int flipChance = nNeighbors * 
                               decayPercentPerDecayedNeighbor;
                            if (NextRandom(1, 101) < flipChance)
                            {
                                map[i][j] = -1;
                            }
                        }
                    }
                }
                for (int i = fillRegion.x; i <= fillRegion.x2; i++)
                {
                    for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                    {
                        if (map[i][j] == -1 && !pixelIsProtected[i][j])
                        {
                            map[i][j] = pixelTypes[1];
                        }
                    }
                }
            }

            return new List<MapRoom>(new MapRoom[0]);
        }
    }
}
