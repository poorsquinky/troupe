using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

namespace ThugSimpleGame {

    public class MapSpaceType {
        public readonly char glyph         = '#';
        public readonly bool passable      = false;
        public readonly bool transparent   = false;
        public readonly string description = "A wall.";
        public readonly int key = -1;
        public readonly int r = 128;
        public readonly int g = 128;
        public readonly int b = 128;
        public readonly int br = 0;
        public readonly int bg = 0;
        public readonly int bb = 0;

        public MapSpaceType(char glyph, bool passable, bool transparent,
           string description, int key, int r, int g, int b, int br, int bb,
           int bg) {
            this.glyph = glyph;
            this.passable = passable;
            this.transparent = transparent;
            this.description = description;
            this.key = key;
            this.r = r;
            this.g = g;
            this.b = b;
            this.br = br;
            this.bg = bg;
            this.bb = bb;
        }
    }
}
