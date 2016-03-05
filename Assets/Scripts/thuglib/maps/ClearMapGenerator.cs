using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class ClearMapGenerator : MapGenerator
    {
        public ClearMapGenerator(int[] pixelTypes, MapCoordinate coordinate) :
           base(pixelTypes, coordinate)
        {
        }

        // pixel types: 
        //    [0] = outer wall
        //    [1] = inner fill
        // custom params:
        //    none
        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            bool[][] pixelIsProtected = BuildProtectedMap(roomsToInclude,
               map.Length, map[0].Length);
            for (int i = fillRegion.x; i <= fillRegion.x2; i++)
            {
                for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                {
                    if (!pixelIsProtected[i][j])
                    {
                        bool isBorder = (i == fillRegion.x ||
                           i == fillRegion.x2 || j == fillRegion.y ||
                           j == fillRegion.y2);
                        map[i][j] = isBorder ? pixelTypes[1] : pixelTypes[0];
                    }
                }
            }
            return new List<MapRoom>(new MapRoom[0]);
        }
    }
}
