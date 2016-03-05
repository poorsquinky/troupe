using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class DungeonRoomMapGenerator : MapGenerator
    {
        private int minSide;

        private int maxSide;

        private int maxDoorsPerRoom;

        private int openPercentage;

        public DungeonRoomMapGenerator(int[] pixelTypes, MapCoordinate
           coordinate, int minSide, int maxSide, int openPercentage,
           int maxDoorsPerRoom) :
           base(pixelTypes, coordinate)
        {
            this.minSide = minSide;
            this.maxSide = maxSide;
            this.openPercentage = openPercentage;
            this.maxDoorsPerRoom = maxDoorsPerRoom;
        }

        private bool RegionOverlapsRoom(int x, int y, int h, int w,
           MapRoom room)
        {
            if (room.bounds.x + room.bounds.w < x) return false;
            if (x + w < room.bounds.x) return false;
            if (room.bounds.y + room.bounds.h < y) return false;
            if (y + h < room.bounds.y) return false;
            return true;
        }

        // pixel types: 
        //    [0] = wall
        //    [1] = floor
        //    [2] = door
        // custom params:
        //    none
        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            List<MapRoom> generatedRooms = new List<MapRoom>();

            // generate the rooms

            int mapArea = map[0].Length * map.Length;
            int typicalRoomArea = (minSide + maxSide) * (minSide + maxSide) / 4;
            int nRooms = mapArea * openPercentage / typicalRoomArea;
            for (int i = 0; i < nRooms; i++)
            {
                int maxPlacementTries = 10;
                bool placed = false;
                int width = NextRandom(minSide, maxSide + 1);
                int height = NextRandom(minSide, maxSide + 1);
                if (width < fillRegion.w && height < fillRegion.h)
                {
                    int x0 = 0, y0 = 0;
                    for (int j = 0; (j < maxPlacementTries) && (!placed); j++)
                    {
                        x0 = NextRandom(0, fillRegion.w - width);
                        y0 = NextRandom(0, fillRegion.h - height);
                        placed = true;
                        for (int k = 0; k < roomsToInclude.Count && placed; k++)
                        {
                            if (RegionOverlapsRoom(x0, y0, height, width,
                               roomsToInclude[k]))
                            {
                                placed = false;
                            }
                        }
                        for (int k = 0; k < generatedRooms.Count && placed; k++)
                        {
                            if (RegionOverlapsRoom(x0, y0, height, width,
                               generatedRooms[k]))
                            {
                                placed = false;
                            }
                        }
                    }
                    if (placed)
                    {
                        generatedRooms.Add(new MapRoom(new MapRectangle(
                           x0, y0, width, height)));
                    }
                }
            }

            // add doors to generatedRooms
            for (int i = 0; i < generatedRooms.Count; i++)
            {
                int nDoors = NextRandom(1, maxDoorsPerRoom + 1);
                MapRoom r = generatedRooms[i];
                for (int j = 0; j < nDoors; j++)
                {
                    int side = NextRandom(0, 4);
                    int offset = NextRandom(1, ((side == 0 || side == 2) ? 
                       r.bounds.w : r.bounds.h) - 1);
                    r.doors.Add(new MapRoomDoor(side, offset, r));
                }
                // clean out any duplicated doors
                for (int j = r.doors.Count - 1; j > 0; j--)
                {
                    bool dupe = false;
                    for (int k = 0; k < j; k++)
                    {
                        if (r.doors[j].side == r.doors[k].side &&
                           r.doors[j].offset == r.doors[k].offset)
                        {
                            dupe = true;
                        }
                    }
                    if (dupe)
                    {
                        r.doors.RemoveAt(j);
                    }
                }
            }

            // now set the map tiles for doors, walls, and floors
            for (int i = 0; i < generatedRooms.Count; i++)
            {
                MapRoom r = generatedRooms[i];
                for (int j = r.bounds.x; j < r.bounds.x + r.bounds.w; j++)
                {
                    for (int k = r.bounds.y; k < r.bounds.y + r.bounds.h; k++)
                    {
                        bool isBorder = (j == r.bounds.x) ||
                           (j == r.bounds.x + r.bounds.w - 1) ||
                           (k == r.bounds.y) ||
                           (k == r.bounds.y + r.bounds.h - 1);
                        map[j][k] = pixelTypes[isBorder ? 0 : 1];
                    }
                }
                for (int j = 0; j < r.doors.Count; j++)
                {
                    int x = r.bounds.x, y = r.bounds.y;
                    switch (r.doors[j].side)
                    {
                        case 0: // N, offset from W
                           x += r.doors[j].offset;
                           break;
                        case 1: // E, offset from N
                           x += r.bounds.w - 1;
                           y += r.doors[j].offset;
                           break;
                        case 2: // S, offset from W
                           x += r.doors[j].offset;
                           y += r.bounds.h - 1;
                           break;
                        case 3: // W, offset from N
                           y += r.doors[j].offset;
                           break;
                    }
                    map[x][y] = pixelTypes[2];
                }
            }

            return generatedRooms;
        }
    }
}
