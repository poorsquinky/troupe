using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class TranslateFromHeightmapMapGenerator : MapGenerator
    {
        public delegate int TranslateToTile(int height);

        private TranslateToTile translate;

        public TranslateFromHeightmapMapGenerator(int[] pixelTypes,
           MapCoordinate coordinate, TranslateToTile translate) :
           base(pixelTypes, coordinate)
        {
            this.translate = translate;
        }

        // pixel types: 
        // custom params:
        //    translate - a delegate that takes the map value and produces
        //        a tile from my tile set
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
                        map[i][j] = translate(map[i][j]);
                    }
                }
            }
            return new List<MapRoom>(new MapRoom[0]);
        }
    }
}
