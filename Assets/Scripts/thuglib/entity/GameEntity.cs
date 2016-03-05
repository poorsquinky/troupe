using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class GameEntity : Entity
    {
        public LevelEntity overworld;

        public List<Entity> entityIndex = new List<Entity>();

        public int RegisterEntity(Entity e)
        {
            // FIXME: unity dependency
            e.entitySeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            entityIndex.Add(e);
            return entityIndex.Count - 1;
        }

        public Entity GetEntity(int i)
        {
            return entityIndex[i];
        }

    }
}

