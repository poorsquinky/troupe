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

        public TerrainEntity terrain;
        public void SetTerrain(TerrainEntity t)
        {
            this.terrain = t;
        }
        public TerrainEntity GetTerrain()
        {
            return this.terrain;
        }

        public PropEntity prop;
        public void SetProp(PropEntity p)
        {
            this.prop = p;
        }
        public PropEntity GetProp()
        {
            return this.prop;
        }

        public ActorEntity actor;
        public void SetActor(ActorEntity a)
        {
            this.actor = a;
            if (a != null)
                a.SetParent(this as Entity);
        }
        public ActorEntity GetActor()
        {
            return this.actor;
        }

        public List<ItemEntity> items;
        public void AddItem(ItemEntity i)
        {
            this.items.Add(i);
        }
        public void RemoveItem(ItemEntity i)
        {
            this.items.Remove(i);
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
                actor.SetParent(this as Entity);
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

