using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class PropEntity : Entity
    {
        public override void DeserializeFields()
        {
        }
        public override void  SerializeFields()
        {
        }
        public PropEntity()
        {
            this.SetEntityType("Prop");
        }
        public void SetCell(CellEntity location)
        {
            this.parent = location as Entity;
            this.parent_index = location.index;
            location.SetProp(this);

            // for now let's say all props are impassible
            this.parent.AddActionCallback("_enter", delegate(Entity actor)
            {
                return false;
            });

        }

    }
}

