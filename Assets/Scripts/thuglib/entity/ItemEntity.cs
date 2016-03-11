using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class ItemEntity : Entity
    {
        public override void DeserializeFields()
        {
        }
        public override void  SerializeFields()
        {
        }
        public ItemEntity()
        {
            this.SetEntityType("Item");
            // TODO
        }

    }
}

