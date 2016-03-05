using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class BSPBuildingMapGenerator : MapGenerator
    {
        private int nLevels;

        private int corridorWidth;

        private int minimumRoomWidth;

        public BSPBuildingMapGenerator(int[] pixelTypes, MapCoordinate coordinate,
           int nLevels, int corridorWidth, int minimumRoomWidth) :
           base(pixelTypes, coordinate)
        {
            this.nLevels = nLevels;
            this.corridorWidth = corridorWidth;
            this.minimumRoomWidth = minimumRoomWidth;
        }

        // pixel types: 
        //    [0] = wall
        //    [1] = floor
        ///   [2] = door
        // custom params:
        //    nLevels = max levels of subdivision
        //    corridorWidth = top-level corridor width; drops by 1 at each
        //       level below til it reaches 1
        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            bool debug = false;
            bool[][] pixelIsProtected = BuildProtectedMap(roomsToInclude,
               map.Length, map[0].Length);
            for (int i = 0; i < fillRegion.w; i++)
            {
                for (int j = 0; j < fillRegion.h; j++)
                {
                    if (!pixelIsProtected[i + fillRegion.x][j + fillRegion.y])
                    {
                        map[i + fillRegion.x][j + fillRegion.y] = pixelTypes[1];
                    }
                }
            }
            List<MapRoom> rooms = BSPBlock(nLevels, corridorWidth, NextRandom(0, 2) == 1,
               pixelIsProtected, map, fillRegion);
            if (debug)
            {
                Console.WriteLine("Fill region " + fillRegion);
            }
            foreach (MapRoom room in rooms)
            {
                bool[] sideOk = new bool[] {true, true, true, true};
                int nSidesOk = 4;
                if (debug)
                {
                    Console.WriteLine("Room at " + room.bounds);
                }
                if (room.bounds.x == fillRegion.x)
                {
                    if (debug)
                    {
                        Console.WriteLine("  W wall external");
                    }
                    sideOk[3] = false;
                    nSidesOk--;
                }
                if (room.bounds.y == fillRegion.y)
                {
                    if (debug)
                    {
                        Console.WriteLine("  N wall external");
                    }
                    sideOk[0] = false;
                    nSidesOk--;
                }
                if (room.bounds.x2 == fillRegion.x2)
                {
                    if (debug)
                    {
                        Console.WriteLine("  E wall external");
                    }
                    sideOk[1] = false;
                    nSidesOk--;
                }
                if (room.bounds.y2 == fillRegion.y2)
                {
                    if (debug)
                    {
                        Console.WriteLine("  S wall external");
                    }
                    sideOk[2] = false;
                    nSidesOk--;
                }
                if (debug)
                {
                    Console.WriteLine("  # doorable sides = " + nSidesOk);
                }
                if (nSidesOk > 0)
                {
                    int sideOffset = NextRandom(0, nSidesOk);
                    int side = 0;
                    while (!sideOk[side] || sideOffset > 0)
                    {
                       if (sideOk[side]) sideOffset--;
                       side++;
                    }
                    int offset = NextRandom(2, (((side % 2) == 0) ?
                       room.bounds.w : room.bounds.h) - 2);
                    if (debug)
                    {
                        Console.WriteLine("  door in side " + side + " offset " + offset);
                    }
                    MapRoomDoor door = new MapRoomDoor(side, offset, room);
                    room.doors.Add(door);
                    map[door.x][door.y] = pixelTypes[2];
                    if (debug)
                    {
                        Console.WriteLine("  door at " + door.x + ", " + door.y);
                    }
                }
            }
            return rooms;
        }

        private List<MapRoom> BSPBlock(int levelsLeft, int corridorWidth, bool horizontalCut,
           bool[][] pixelIsProtected, int[][] map, MapRectangle fillRegion)
        {
            MapRectangle firstSubregion, secondSubregion;
            int dx = fillRegion.x;
            int dy = fillRegion.y;
            int minimumDim = corridorWidth + 2 * minimumRoomWidth + 4;
            int lowCutSpot = minimumRoomWidth + 2;
            if (horizontalCut)
            {
                if (fillRegion.h <= minimumDim)
                {
                    return new List<MapRoom>(new MapRoom[]{new MapRoom(fillRegion)});
                }
                // o r r r x c c x r r r o
                //         ^ 
                int highCutSpot = fillRegion.h - minimumRoomWidth - corridorWidth - 3;
                int cutSpot = NextRandom(lowCutSpot, highCutSpot + 1);
                for (int i = 0; i < fillRegion.w; i++) 
                {
                    map[i + dx][cutSpot + dy] = pixelTypes[0];
                    for (int j = 1; j <= corridorWidth; j++)
                    {
                        map[i + dx][cutSpot + j + dy] = pixelTypes[1];
                    }
                    map[i + dx][cutSpot + corridorWidth + 1 + dy] = pixelTypes[0];
                }
                firstSubregion = new MapRectangle(fillRegion.x, fillRegion.y,
                   fillRegion.w, cutSpot + 1);
                secondSubregion = new MapRectangle(fillRegion.x, fillRegion.y +
                   cutSpot + corridorWidth + 1, fillRegion.w, fillRegion.h - cutSpot -
                   corridorWidth - 1);
            }
            else
            {
                if (fillRegion.w <= minimumDim)
                {
                    return new List<MapRoom>(new MapRoom[]{new MapRoom(fillRegion)});
                }
                int highCutSpot = fillRegion.w - minimumRoomWidth - corridorWidth - 3;
                int cutSpot = NextRandom(lowCutSpot, highCutSpot + 1);
                for (int i = 0; i < fillRegion.h; i++) 
                {
                    map[cutSpot + dx][i + dy] = pixelTypes[0];
                    for (int j = 1; j <= corridorWidth; j++)
                    {
                        map[cutSpot + j + dx][i + dy] = pixelTypes[1];
                    }
                    map[cutSpot + corridorWidth + 1 + dx][i + dy] = pixelTypes[0];
                }
                firstSubregion = new MapRectangle(fillRegion.x, fillRegion.y,
                   cutSpot + 1, fillRegion.h);
                secondSubregion = new MapRectangle(fillRegion.x + cutSpot + 
                   corridorWidth + 1,
                   fillRegion.y, fillRegion.w - cutSpot - corridorWidth - 1,
                   fillRegion.h);
            }

            if (levelsLeft >= 1)
            {
                // split me

                int newCorridorWidth = corridorWidth - 1;
                if (newCorridorWidth <= 1)
                {
                    newCorridorWidth = 1;
                }
                List<MapRoom> list1 = BSPBlock(levelsLeft - 1, newCorridorWidth, !horizontalCut,
                   pixelIsProtected, map, firstSubregion);
                List<MapRoom> list2 = BSPBlock(levelsLeft - 1, newCorridorWidth, !horizontalCut,
                   pixelIsProtected, map, secondSubregion);
                list1.AddRange(list2);
                return list1;
            }
            else
            {
                return new List<MapRoom>(new MapRoom[]{new MapRoom(firstSubregion),
                   new MapRoom(secondSubregion)});
            }
        }
    }
}
