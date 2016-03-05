using System;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

namespace ThugSimpleGame {
    public class OverworldMapManager : MapManager {

        public delegate int TranslateToTile(int height);

        private int[][] grid;

        private int coarseToFineScalePowerOfTwo = 2;

        private class MapSpacePatch {
            public MapRectangle myRegion;
            public MapSpaceType[][] patch;
            public long lastAccessInTicks;

            public bool Matches(MapRectangle region)
            {
                return (myRegion.x == region.x && myRegion.y == region.y &&
                   myRegion.w == region.w && myRegion.h == region.h);
            }
        };

        private List<MapSpacePatch> patches = new List<MapSpacePatch>();

        private double maxAgeOfPatchToKeepInSeconds = 100.0;

        private TranslateToTile translator;

        public OverworldMapManager(int levelWidth, int levelHeight) {
            this.translator = x => (x > 0 ? 6 : 5);
            this.Bounds = new MapRectangle(0, 0, levelWidth <<
               coarseToFineScalePowerOfTwo, levelHeight <<
               coarseToFineScalePowerOfTwo);
            this.grid = new int[levelWidth][];
            for (int i = 0; i < levelWidth; i++) grid[i] = new int[levelHeight];
            MapRectangle fullArea = new MapRectangle(0, 0, levelWidth,
               levelHeight);
            ClearMapGenerator gen = new ClearMapGenerator(new int[] {0, 0},
               MapCoordinate.GenerateRandom());
            gen.Run(grid, fullArea, null);

            OpenSimplexHeightmapGenerator gen2 = new
               OpenSimplexHeightmapGenerator(new int[] {0, 0},
               MapCoordinate.GenerateRandom(), 4, levelWidth >> 1,
               1.0, 10000.0, 0.5);
            gen2.Run(grid, fullArea, null);

            AddSpaceType(key: 0, glyph: '#', passable: true, transparent: true);
            AddSpaceType(key: 1, glyph: '#', passable: true, transparent: true);
            AddSpaceType(key: 2, glyph: '#', passable: true, transparent: true);
            AddSpaceType(key: 3, glyph: '*', passable: true, transparent: true);
            AddSpaceType(key: 4, glyph: '~', passable: true, transparent: true);
            AddSpaceType(key: 5, glyph: '#', passable: true, transparent: true);
            AddSpaceType(key: 6, glyph: ' ', passable: true, transparent: true);
            AddSpaceType(key: 7, glyph: '+', passable: true, transparent: true);
        }

        // populating patches handled in here
        private MapSpaceType[][] RawGetPatch(MapRectangle region)
        {
            MapSpaceType[][] patch = new MapSpaceType[region.w][];
            for (int i = 0, x = region.x; i < region.w; i++, x++) {
                patch[i] = new MapSpaceType[region.h];
                for (int j = 0, y = region.y; j < region.h; j++, y++) {
                    patch[i][j] = this.palette[translator(this.grid[
                       x >> coarseToFineScalePowerOfTwo][
                       y >> coarseToFineScalePowerOfTwo])];
                }
            }
            return patch;
        }

        // buffering of patches handled in here
        public override MapSpaceType[][] GetPatch(MapRectangle region) {
            long currentTick = DateTime.Now.Ticks;
            MapSpacePatch store;
            for (int i = 0; i < patches.Count; i++)
            {
                 if (patches[i].Matches(region))
                 {
                     store = patches[i];
                     patches.RemoveAt(i);
                     patches.Insert(0, store);
                     patches[i].lastAccessInTicks = currentTick;
                     return patches[i].patch;
                 }
            }

            MapSpaceType[][] patch = RawGetPatch(region);

            for (int i = patches.Count - 1; i > 0 && TimeSpan.FromTicks(
               currentTick - patches[i].lastAccessInTicks).TotalSeconds >
               maxAgeOfPatchToKeepInSeconds; i--) 
            {
                patches.RemoveAt(i);
            }
            store = new MapSpacePatch();
            store.patch = patch;
            store.myRegion = region;
            store.lastAccessInTicks = currentTick;
            return patch;
        }
    }
}
