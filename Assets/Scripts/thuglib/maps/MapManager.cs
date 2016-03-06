using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

namespace ThugSimpleGame {
    public abstract class MapManager {

        protected Dictionary<int,MapSpaceType> palette = 
           new Dictionary<int,MapSpaceType>();

        protected MapRectangle bounds;

        public MapRectangle Bounds
        {
            get
            {
                return bounds;
            }

            protected set
            {
                bounds = value;
            }
        }

        public void AddSpaceType(
                char glyph,
                int key,
                bool passable      = true,
                bool transparent   = true,
                string description = "",
                int r              = 128,
                int g              = 128,
                int b              = 128,
                int br             = 0,
                int bg             = 0,
                int bb             = 0)
        {
            MapSpaceType st = new MapSpaceType(glyph, passable,
               transparent, description, key, r, g, b, br, bg, bb);
            this.palette[key] = st;
        }

        public abstract void Generate();
        public abstract void PostProcess();

        public abstract CellEntity[][] GetPatch(MapRectangle region);
    }
}
