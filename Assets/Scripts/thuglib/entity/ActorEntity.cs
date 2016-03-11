using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public bool isPlayer = false;

        public bool MoveTo(int x, int y)
        {
            GameObject g = GameObject.Find("GameManager");
            GameManagerScript gm = g.GetComponent<GameManagerScript>();
            LevelEntity level = gm.lm.entity;

            CellEntity oldCell = this.parent as CellEntity;
            CellEntity newCell = level.GetCell(x,y);
            if (oldCell.ActorExit(this))
            {
                if (newCell.ActorEnter(this))
                {
                    return true;
                }
//                else
//                {
//                    oldCell.ActorForceEnter(this);
//                }
            }
            return false;
        }

        public void SetHP(int hp)
        {
            this.stats["hp"] = hp;
        }

        public int GetHP()
        {
            if (!this.stats.ContainsKey("hp"))
            {
                this.stats["hp"]  = 3;
                this.stats["mhp"] = 3;
            }
            return this.stats["hp"];
        }
        public int GetMHP()
        {
            if (!this.stats.ContainsKey("mhp"))
            {
                this.stats["hp"]  = 3;
                this.stats["mhp"] = 3;
            }
            return this.stats["mhp"];
        }

        public CellEntity GetCell()
        {
            return this.parent as CellEntity;
        }

        public ActorEntity()
        {
            this.SetEntityType("Actor");
        }

        public override void DeserializeFields()
        {
            if (serialFields["isPlayer"] == "1")
                this.isPlayer = true;
            else
                this.isPlayer = false;
        }
        public override void  SerializeFields()
        {
            Dictionary<string,string> serialFields = new Dictionary<string,string>();
            serialFields["isPlayer"] = isPlayer? "1" : "0";

        }


    }
}

