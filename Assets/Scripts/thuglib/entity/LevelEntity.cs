using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class LevelEntity : Entity
    {
        public CellEntity[][] cells;

        public CellEntity GetCell(int x, int y)
        {
            if ( x >= 0 && y >= 0
                && x < this.cells.Length
                && y < this.cells[x].Length)
                    return this.cells[x][y];
            return null;
        }

        public int GetW(){
            return cells.Length;
        }
        public int GetH(){
            return cells[0].Length;
        }

        public LevelEntity(int w, int h, GameEntity p)
        {
            this.SetEntityType("Level");
            this.parent = p;
            this.parent_index = p.index;
            cells = new CellEntity[h][];
            for (int x = 0; x < w; x++)
            {
                cells[x] = new CellEntity[h];
                for (int y = 0; y < h; y++)
                {
                    cells[x][y] = new CellEntity(
                            x: x,
                            y: y,
                            parent: this
                    );
                    cells[x][y].index = p.RegisterEntity(cells[x][y]);
                }
            }
        }

    }
}

