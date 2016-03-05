using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class CAGrowthMapGenerator : MapGenerator
    {
        private int[] openGrowthPixels;

        private int newGrowthPixel;

        private int seedPointInverseDensity;

        private int numberOfGrowPasses;

        private int growthPercentPerGrownNeighbor;

        // pixel types: 
        //    [0] = what grows here
        //    [1]..[n] = existing pixel types [0] can grow into
        // custom params:
        //    seedPointInverseDensity - on the first pass, we seed with single
        //       pixels of [0], on average one per this many growable squares
        //    numberOfGrowPasses - number of CA growth steps to run
        //    growthPercentPerGrownNeighbor - chance of an open growable square
        //       switching to [0] = # of [0] neighbors * this percent
        public CAGrowthMapGenerator(int[] pixelTypes,
           MapCoordinate coordinate,
           int seedPointInverseDensity,
           int numberOfGrowPasses,
           int growthPercentPerGrownNeighbor) : base(pixelTypes, coordinate)
        {
            this.newGrowthPixel = pixelTypes[0];
            this.openGrowthPixels = new int[pixelTypes.Length - 1];
            for (int i = 0; i < openGrowthPixels.Length; i++)
            {
                openGrowthPixels[i] = pixelTypes[i + 1];
            }
            this.seedPointInverseDensity = seedPointInverseDensity;
            this.numberOfGrowPasses = numberOfGrowPasses;
            this.growthPercentPerGrownNeighbor = growthPercentPerGrownNeighbor;
        }

        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            bool[][] pixelIsProtected = BuildProtectedMap(roomsToInclude,
               map.Length, map[0].Length);
        
            // make a map of which pixels are open for growth
            int nOpen = 0;
            bool[][] open = new bool[fillRegion.w][];
            for (int i = fillRegion.x; i <= fillRegion.x2; i++)
            {
                open[i] = new bool[fillRegion.h];
                for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                {
                    if (!pixelIsProtected[i][j])
                    {
                        for (int k = 0; k < openGrowthPixels.Length; k++)
                        {
                            if (map[i][j] == openGrowthPixels[k])
                            {
                                open[i - fillRegion.x][j - fillRegion.y] = true;
                                nOpen++;
                            }
                        }
                    }
                }
            }

            // put in the seeds
            int n = nOpen / seedPointInverseDensity;
            for (int i = 0; i < n; i++)
            {
                int offset = NextRandom(0, nOpen);
                for (int j = 0; (j < fillRegion.w) && (offset > 0); j++)
                {
                    for (int k = 0; k < fillRegion.h; k++)
                    {
                       if (open[j][k])
                       {
                           offset--;
                           if (offset == 0)
                           {
                               map[j + fillRegion.x][k + fillRegion.y] = 
                                  newGrowthPixel;
                           }
                       }
                    }
                }
            }

            for (int k = 0; k < numberOfGrowPasses; k++)
            {
                for (int i = fillRegion.x; i <= fillRegion.x2; i++)
                {
                    for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                    {
                        if (map[i][j] != newGrowthPixel &&
                           open[i - fillRegion.x][j - fillRegion.y])
                        {
                            int nNeighbors = 0;
                            if (i > 0)
                            {
                               if (j > 0)
                               {
                                   if (map[i - 1][j - 1] == newGrowthPixel)
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (j < map[i].Length - 1)
                               {
                                   if (map[i - 1][j + 1] == newGrowthPixel)
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (map[i - 1][j] == newGrowthPixel)
                               {
                                   nNeighbors++;
                               }
                            }
                            if (i < map.Length - 1)
                            {
                               if (j > 0)
                               {
                                   if (map[i + 1][j - 1] == newGrowthPixel)
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (j < map[i].Length - 1)
                               {
                                   if (map[i + 1][j + 1] == newGrowthPixel)
                                   {
                                       nNeighbors++;
                                   }
                               }
                               if (map[i + 1][j] == newGrowthPixel)
                               {
                                   nNeighbors++;
                               }
                            }
                            if (j > 0)
                            {
                                if (map[i][j - 1] == newGrowthPixel)
                                {
                                    nNeighbors++;
                                }
                            }
                            if (j < map[i].Length - 1)
                            {
                                if (map[i][j + 1] == newGrowthPixel)
                                {
                                    nNeighbors++;
                                }
                            }
                            int flipChance = nNeighbors * 
                               growthPercentPerGrownNeighbor;
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
                            map[i][j] = newGrowthPixel;
                        }
                    }
                }
            }

            return new List<MapRoom>(new MapRoom[0]);
        }
    }
}
