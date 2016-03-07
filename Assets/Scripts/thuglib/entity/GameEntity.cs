using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class GameEntity : Entity
    {
        public LevelEntity overworld;

        public List<Entity> entityIndex = new List<Entity>();

        // TODO: serialize and deserialize oh god why

        public int RegisterEntity(Entity e)
        {
            // FIXME: unity dependency
            if (e.entitySeed == 0)
                e.entitySeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            entityIndex.Add(e);
            return entityIndex.Count - 1;
        }

        public void DeregisterEntity(Entity e)
        {
            entityIndex.Remove(e);
        }

        public Entity GetEntity(int i)
        {
            return entityIndex[i];
        }

    }
}

