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
        public List<LevelEntity> levels;

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

        public PlaceEntity()
        {
            this.SetEntityType("Place");
            this.levels = new List<LevelEntity>();
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

