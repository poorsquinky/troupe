using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThugLib
{
    public class PlaceEntity : Entity
    {

        // FIXME: serialize levelsUp
        public bool levelsUp = true; // do levels increase in the upward direction
        // FIXME: also serialize placeType
        public string placeType;
        public List<LevelEntity> levels;

        // FIXME: not here
        GameManagerScript gm;

        public override void DeserializeFields()
        {
            this.levels = new List<LevelEntity>();
            List<string> levelList = JsonUtility.FromJson<List<string>>(serialFields["levels"]);
            foreach (string s in levelList)
            {
                this.levels.Add(JsonUtility.FromJson<LevelEntity>(s));
            }
            foreach (LevelEntity l in this.levels)
            {
                l.Deserialize();
                // XXX not implemented yet:
//                i.SetCell(this);
            }
        }
        public override void  SerializeFields()
        {
            List<string> levelList = new List<string>();
            if (this.levels != null)
            {
                foreach (LevelEntity level in this.levels)
                {
                    level.Serialize();
                    levelList.Add(JsonUtility.ToJson(level));
                }
            }
            serialFields["levels"] = JsonUtility.ToJson(levelList);
        }

        // FIXME this should be done with delegates but it's not because tick tock
        public void Activate()
        {
            gm.Message("Entering " + this.longDescription + "...");
            switch (this.placeType)
            {
                case "city":
                    gm.ActivateCircus(this);
                    break;
            }
        }

        public PlaceEntity()
        {
            GameObject g = GameObject.Find("GameManager");
            gm = g.GetComponent<GameManagerScript>();
            this.SetEntityType("Place");
            this.levels = new List<LevelEntity>();
        }

        public void AddLevel(LevelEntity l)
        {
            this.levels.Add(l);
        }

        public CellEntity GetCell()
        {
            return this.parent as CellEntity;
        }

        public void SetCell(CellEntity location)
        {
            this.parent = location as Entity;
            this.parent_index = location.index;
            location.SetPlace(this);

            // FIXME: make this the thing that activates it!
            /*
            this.parent.AddActionCallback("_enter", delegate(Entity actor, Entity self)
            {
                // TODO: apply movement speed hindrance to actor
                // TODO: confirm player movement into slow space
                if (this.hindrance < 1f)
                    return true;

                return false;
            });
            */

        }

    }
}

