using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public struct MapRectangle
    {
        public int x, y; /* upper left corner */
        public int w, h;

        public override String ToString()
        {
            return "[ " + x + ", " + y + " - " + x2 + ", " + y2 + " ]";
        }

        public int x2
        {
           get
           {
              return x + w - 1;
           }
        }
        public int y2
        {
            get
            {
                return y + h - 1;
            }
        }
        public int xCenter
        {
            get
            {
                return x + w / 2;
            }
        }
        public int yCenter
        {
            get
            {
                return y + h / 2;
            }
        }
        public MapRectangle(int x, int y, int w, int h)
        {
            this.x = x; this.y = y; this.w = w; this.h = h;
        }
        public bool Contains(int x1, int y1)
        {
            return (x1 >= x && y1 >= y && x1 <= x2 && y1 <= y2);
        }
    };

    public struct MapRoomDoor
    {
        public int side; /* 0 1 2 3 = N E S W */
        public int offset; /* from W or N end */
        public MapRoom parent;
        public int x
        {
            get
            {
                if (side == 0 || side == 2)
                {
                    return parent.bounds.x + offset;
                }
                else if (side == 1)
                {
                    return parent.bounds.x2;
                }
                else
                {
                    return parent.bounds.x;
                }
            }
        }
        public int y
        {
            get
            {
                if (side == 1 || side == 3)
                {
                    return parent.bounds.y + offset;
                }
                else if (side == 0)
                {
                    return parent.bounds.y;
                }
                else
                {
                    return parent.bounds.y2;
                }
            }
        }
        public MapRoomDoor(int side, int offset, MapRoom parent)
        {
            this.side = side;
            this.offset = offset;
            this.parent = parent;
        }
    };

    public struct MapRoom
    {
        public MapRectangle bounds;
        public List<MapRoomDoor> doors;
        public bool doNotConnect;
        public MapRoom(MapRectangle bounds)
        {
            this.bounds = bounds;
            this.doors = new List<MapRoomDoor>();
            this.doNotConnect = false;
        }
    };

    // represents the map's coordinate in the world; used to seed the RNG
    // if desired.
    public struct MapCoordinate
    {
        public long guid;

        public static long nextGuid = 0L;

        public static MapCoordinate GenerateRandom()
        {
            MapCoordinate newCoord = new MapCoordinate();
            newCoord.guid = nextGuid++;
            return newCoord;
        }
    };

    public abstract class MapGenerator
    {
        protected int[] pixelTypes;

        protected MapCoordinate coordinate;

        protected Random random;

        public void UseCoordinateBasedRandom()
        {
            random = new Random((Int32)coordinate.guid);
        }

        public MapGenerator(int[] pixelTypes, MapCoordinate coordinate)
        {
            this.pixelTypes = pixelTypes;
            this.coordinate = coordinate;
            this.random = new Random();
        }

        protected int NextRandom(int low, int highPlusOne)
        {
            return random.Next(low, highPlusOne);
        }

        public abstract List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude);

        public virtual void FillHeightmap(int[][] heightmap,
           MapRectangle fillRegion)
        {
            throw new Exception("This MapGenerator doesn't have height!");
        }

        public bool[][] BuildProtectedMap(List<MapRoom> roomsToInclude,
           int w, int h) 
        {
            bool[][] mask = new bool[w][];
            for (int i = 0; i < w; i++)
            {
                mask[i] = new bool[h];
            }
            if (roomsToInclude != null)
            {
                for (int i = 0; i < roomsToInclude.Count; i++)
                {
                    for (int x = roomsToInclude[i].bounds.x; x <=
                       roomsToInclude[i].bounds.x2; x++)
                    {
                        for (int y = roomsToInclude[i].bounds.y; y <=
                           roomsToInclude[i].bounds.y2; y++)
                        {
                            mask[x][y] = true;
                        }
                    }
                }
            }
            return mask;
        }

        public bool[][] Allocate2DBoolArray(int w, int h)
        {
            bool[][] retval = new bool[w][];
            for (int i = 0; i < w; i++)
            {
                retval[i] = new bool[h];
            }
            return retval;
        }

        public void Clear2DBoolArray(bool[][] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[0].Length; j++)
                {
                    a[i][j] = false;
                }
            }
        }

        public int[][] Allocate2DIntArray(int w, int h)
        {
            int[][] retval = new int[w][];
            for (int i = 0; i < w; i++)
            {
                retval[i] = new int[h];
            }
            return retval;
        }

        public void SearchAndReplace2DIntArray(int[][] a, int fromVal, int toVal)
        {
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[0].Length; j++)
                {
                    if (a[i][j] == fromVal)
                    {
                        a[i][j] = toVal;
                    }
                }
            }
        }

        public void Clear2DIntArray(int[][] a, int clearVal)
        {
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[0].Length; j++)
                {
                    a[i][j] = clearVal;
                }
            }
        }
    }
}
