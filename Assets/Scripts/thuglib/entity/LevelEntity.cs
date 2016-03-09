using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThugLib
{
    public class LevelEntity : Entity
    {
        public CellEntity[][] cells;

        public override void DeserializeFields()
        {
            int w = Convert.ToInt32(serialFields["cells_w"]);
            int h = Convert.ToInt32(serialFields["cells_h"]);

            this.cells = new CellEntity[w][];
            for (int x = 0; x < cells.Length; x++)
            {
                this.cells[x] = new CellEntity[h];
                for (int y = 0; y < cells[x].Length; y++)
                {
                    cells[x][y] = null;
                }
            }

            string[] lines = serialFields["cells"].Split(new[] {"<<CELLLINE>>"}, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string[] entry = line.Split(new[] {"<<CELLDELIMIT>>"}, StringSplitOptions.None);
                string[] coords = entry[0].Split(',');
                int x = Convert.ToInt32(coords[0]);
                int y = Convert.ToInt32(coords[1]);
                cells[x][y] = JsonUtility.FromJson<CellEntity>(entry[1]);
                cells[x][y].SetParent(this);
                cells[x][y].Deserialize();
            }

        }
        public override void  SerializeFields()
        {
            int w = GetW();
            int h = GetH();
            serialFields["cells_w"] = w.ToString();
            serialFields["cells_h"] = h.ToString();

            string s = "";
            bool first = true;
            for (int x = 0; x < cells.Length; x++)
            {
                for (int y = 0; y < cells[x].Length; y++)
                {
                    cells[x][y].Serialize();
                    if (!first)
                        s = s + "<<CELLLINE>>";
                    s = s + x + "," + y + "<<CELLDELIMIT>>" + JsonUtility.ToJson(cells[x][y]);
                    first = false;
                }
            }

            serialFields["cells"] = s;
        }

        public List<CellEntity> GetAllCells()
        {
            List<CellEntity> c = new List<CellEntity>();
            for (int x = 0; x < this.cells.Length; x++)
                for (int y = 0; y < this.cells[x].Length; y++)
                    c.Add(this.cells[x][y]);
            return c;
        }

        public List<ActorEntity> GetAllActors()
        {
            List<ActorEntity> actors = new List<ActorEntity>();
            foreach (CellEntity cell in this.GetAllCells())
            {
                ActorEntity actor = cell.GetActor();
                if (actor != null)
                    actors.Add(actor);
            }
            return actors;
        }

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

