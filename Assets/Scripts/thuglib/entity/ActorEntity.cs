using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class ActorEntity : Entity
    {
        public int GetX() {
            CellEntity p = this.parent as CellEntity;
            if (p != null)
                return p.GetX();
            return -1;
        }
        public int GetY() {
            CellEntity p = this.parent as CellEntity;
            if (p != null)
                return p.GetY();
            return -1;
        }

        public bool MoveTo(int x, int y)
        {
            CellEntity oldCell = this.parent as CellEntity;
            LevelEntity level = oldCell.GetParent() as LevelEntity;
            CellEntity newCell = level.GetCell(x,y);
            if (oldCell.ActorExit(this))
            {
                if (newCell.ActorEnter(this))
                {
                    return true;
                }
                else
                {
                    oldCell.ActorForceEnter(this);
                }
            }
            return false;
        }

        public ActorEntity()
        {
            this.SetEntityType("Actor");
            // TODO
        }

    }
}

