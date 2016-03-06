using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

namespace ThugSimpleGame {
    public class TroupeOverworldMapManager : MapManager {

        private int[][] grid;

        public TroupeOverworldMapManager(LevelEntity entity) {
            int levelWidth  = entity.GetW();
            int levelHeight = entity.GetH();
            AddSpaceType(key: 0, glyph: '.', passable: true, transparent: true);
            AddSpaceType(key: 1, glyph: '-', passable: true, transparent: true);
            AddSpaceType(key: 2, glyph: '+', passable: true, transparent: true);
            AddSpaceType(key: 3, glyph: '~', passable: false, transparent: true);
            AddSpaceType(key: 4, glyph: '*', passable: true, transparent: true);
            Bounds = new MapRectangle(0, 0, levelWidth, levelHeight);
            this.grid = new int[levelWidth][];
            for (int i = 0; i < levelWidth; i++)
            {
                grid[i] = new int[levelHeight];
                for (int j = 0; j < levelHeight; j++)
                    grid[i][j] = 0;
            }

/*
            for (int i = 10; i < levelWidth - 10; i++)
                grid[i][levelHeight - 10] = 1;
            for (int i = 10; i <= levelHeight - 10; i++)
                grid[levelWidth - 10][i] = 1;
*/

            grid[levelWidth - 12][levelHeight - 10] = 2;

            int[,] prefabOutdoor = new int[,] {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,4,4,4,4,4,4,4,4,3,3,3,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,4,4,4,4,4,4,4,3,3,3,3,0,0,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,3,3,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,2,1,1,1,1,1,1,1,1,1,1,0,3,3,3,3,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,3,3,3,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,4,4,0,0,0,0,0,0,0,1,0,3,3,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,4,4,4,4,4,4,4,4,4,0,0,1,0,3,3,3,0,0,1,1,1,1,1,1,1,1,2,0,0,0,0,4,0,0,0,0,0},
                {0,0,0,0,0,4,4,4,4,4,4,4,0,0,1,0,3,3,3,0,0,1,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,0,0},
                {0,0,0,0,0,0,4,4,4,4,4,4,0,0,1,0,3,3,3,0,0,1,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,4,0,0},
                {0,0,0,0,0,0,0,4,4,4,4,4,0,0,1,0,3,3,3,0,0,1,0,0,4,4,4,4,4,4,4,4,4,4,4,4,4,4,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,3,3,3,0,0,1,0,4,4,4,4,4,0,0,0,0,4,4,4,4,4,4,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,3,3,0,0,0,1,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,0,0},
                {0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,0,3,3,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,0},
                {0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,3,3,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,4,4,3,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,0,0,0,0,4,4,4,4,4,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,0,0,0,4,4,0,4,4,4,0,0,1,0,0,0,0,0,0,0,0,0,0,3,3,3,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,3,3,3,3,3,3,3,3,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0,0},
                {0,0,0,0,0,0,0,1,1,1,1,2,1,1,1,1,1,1,1,1,0,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,3,3,3,3,3,3,3,0,3,3,0,0,0,0,3,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,3,3,3,3,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,4,4,4,4,4,4,4,4,4,4,0,0,1,0,0,0,0,0,0,3,3,3,3,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,0,0,0,1,0,0,0,0,0,0,3,3,3,3,0,0,0,2,0,0,0,0,0,0},
                {0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,4,0,0,0,1,0,0,0,0,0,0,0,3,3,4,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,4,0,0,0,1,0,0,0,0,0,0,0,4,4,4,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,0,0,0,1,0,0,0,0,0,0,0,4,4,0,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,0,0,0,1,1,1,1,1,1,0,0,4,4,0,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,1,0,0,0,4,0,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            };
            for (int x = 0; x < 40; x++)
            {
                for (int y = 0; y < 40; y++)
                {
                    grid[levelWidth - 41 + x][levelHeight - y - 10] = prefabOutdoor[y,x];
                }
            }
            // width - 46, height - 7

            /*
            MapRectangle fullArea = new MapRectangle(0, 0, levelWidth,
               levelHeight);
            ClearMapGenerator gen = new ClearMapGenerator(new int[] {0, 0},
               MapCoordinate.GenerateRandom());

            ClearMapGenerator gen2 = new ClearMapGenerator(new int[] {2, 1},
               MapCoordinate.GenerateRandom());
            gen2.Run(grid, new MapRectangle(10, 10, 10, 10), null);
            List<MapRoom> blockedList = new List<MapRoom>();
            blockedList.Add(new MapRoom(new MapRectangle(10, 10, 10, 10)));
            gen.Run(grid, fullArea, blockedList);

            DungeonRoomMapGenerator drmg = new DungeonRoomMapGenerator(new
               int[] {5, 6, 7}, MapCoordinate.GenerateRandom(),
               5, 12, 10, 3);
            List<MapRoom> allRooms = drmg.Run(grid, fullArea, blockedList);
            DungeonCorridorMapGenerator dcmg = new
               DungeonCorridorMapGenerator(
               new int[] {5, 6, 7}, MapCoordinate.GenerateRandom(), 2,
               new int[] {0, 100000, 100000, 0, 0, 100000, 0, 0});
            dcmg.Run(grid, fullArea, allRooms);

            AddSpaceType(key: 0, glyph: '#', passable: false, transparent: false);
            AddSpaceType(key: 1, glyph: '#', passable: false, transparent: false);
            AddSpaceType(key: 2, glyph: '#', passable: false, transparent: false);
            AddSpaceType(key: 3, glyph: '*', passable: true, transparent: true);
            AddSpaceType(key: 4, glyph: '~', passable: true, transparent: true);
            AddSpaceType(key: 5, glyph: '#', passable: false, transparent: false);
            AddSpaceType(key: 6, glyph: ' ', passable: true, transparent: true);
            AddSpaceType(key: 7, glyph: '+', passable: true, transparent: false);
            */
        }

        public override MapSpaceType[][] GetPatch(MapRectangle region) {
            MapSpaceType[][] patch = new MapSpaceType[region.w][];
            for (int i = 0; i < region.w; i++) {
                patch[i] = new MapSpaceType[region.h];
                for (int j = 0; j < region.h; j++) {
                    patch[i][j] = this.palette[this.grid[i][j]];
                }
            }
            return patch;
        }
    }
}
