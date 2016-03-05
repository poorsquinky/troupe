using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThugLib
{
    public class CellEntity : Entity
    {
        public int x,y;

        public int GetX() {
            return this.x;
        }
        public int GetY() {
            return this.y;
        }

        public int terrainIndex;
        public TerrainEntity terrain;
        public void SetTerrain(TerrainEntity t)
        {
            this.terrain = t;
            terrainIndex = t.index;
            t.parent = this;
            t.parent_index = this.index;
        }
        public TerrainEntity GetTerrain()
        {
            return this.terrain;
        }

        public int propIndex;
        public PropEntity prop;
        public void SetProp(PropEntity p)
        {
            this.prop = p;
            propIndex = p.index;
            p.parent = this;
            p.parent_index = this.index;
        }
        public PropEntity GetProp()
        {
            return this.prop;
        }

        public int actorIndex;
        public ActorEntity actor;
        public void SetActor(ActorEntity a)
        {
            this.actor = a;
            if (a != null)
            {
                a.SetParent(this as Entity);
                actorIndex = a.index;
            }
            else
            {
                actorIndex = -1;
            }
        }
        public ActorEntity GetActor()
        {
            return this.actor;
        }

        public List<int> items_index;
        public List<ItemEntity> items;
        public void AddItem(ItemEntity i)
        {
            this.items.Add(i);
            this.items_index.Add(i.index);
            i.parent = this;
            i.parent_index = this.index;
        }
        public void RemoveItem(ItemEntity i)
        {
            this.items.Remove(i);
            this.items_index.Remove(i.index);
        }
        public List<ItemEntity> GetItems()
        {
            return this.items;
        }

        public bool ActorExit(ActorEntity actor)
        {
            if (this.terrain != null)
                if (! this.terrain.RunActionCallbacks(actor, "_exit"))
                    return false;
            if (this.RunActionCallbacks(actor, "_exit"))
            {
                this.SetActor(null);
                this.actorIndex=-1;
                return true;
            }
            return false;
        }
        public bool ActorEnter(ActorEntity actor)
        {
            if (this.terrain != null)
                if (! this.terrain.RunActionCallbacks(actor, "_enter"))
                    return false;
            if (this.RunActionCallbacks(actor, "_enter"))
            {
                this.SetActor(actor);
                return true;
            }
            return false;
        }
        public void ActorForceEnter(ActorEntity actor)
        {
            this.SetActor(actor);
        }

        public CellEntity(int x, int y, LevelEntity parent)
        {
            this.SetEntityType("Cell");
            this.x = x;
            this.y = y;
            this.parent = parent;
            this.parent_index = parent.index;
        }


        public float GetOpacity()
        {
            if (this.terrain != null)
                return this.terrain.opacity;
            return 0.0f;
        }

        public float GetHindrance()
        {
            if (this.terrain != null)
                return this.terrain.hindrance;
            return 0.0f;
        }

        public bool Passable()
        {
            return (GetHindrance() < 1f);
        }
        public bool Visible()
        {
            return (GetOpacity() < 1f);
        }

    }
}

