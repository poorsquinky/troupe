using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class TerrainEntity : Entity
    {
        public float hindrance = 0.0f;  // 0f: move at normal speed; 1f: impassable
        public float opacity   = 0.0f;  // 0f: transparent; 1f: opaque

        public TerrainEntity()
        {
            this.SetEntityType("Terrain");
        }

        public void SetCell(CellEntity location)
        {
            this.parent = location as Entity;
            location.SetTerrain(this);

            this.parent.AddActionCallback("_enter", delegate(Entity actor)
            {
                // TODO: apply movement speed hindrance to actor
                // TODO: confirm player movement into slow space
                if (this.hindrance < 1f)
                    return true;

                return false;
            });

        }

    }
}

