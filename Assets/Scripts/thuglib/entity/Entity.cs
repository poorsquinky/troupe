using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThugLib
{
    [Serializable]
    public class Entity
    {

        protected string entityType = "NONE"; // we're abusing strings for a lot of these things for extensibility

        public int index;

        public Entity parent;

        public string shortDescription;
        public string longDescription;

        // FIXME (later): unity dependency in thuglib
        public int entitySeed = 0;
        //public int entitySeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        // For the benefit of Unity scripts, refreshDelegates are a list
        // of subscribers that will be informed whenever this Entity
        // changes at all.  This is due to C# not supporting multiple
        // inheritance because they hate freedom.

        public delegate void RefreshDelegate();
        protected List<RefreshDelegate> refreshDelegates = new List<RefreshDelegate>();

        public delegate bool CallbackDelegate(Entity subject);
        protected Dictionary<string, List<CallbackDelegate>> actionCallbacks = new Dictionary<string, List<CallbackDelegate>>();

        public List<string> tags = new List<String>();

        protected List<Entity> children    = new List<Entity>();
        protected List<Entity> descendants = new List<Entity>();
        protected Dictionary<string, List<Entity>> descendantsByTag = new Dictionary<string, List<Entity>>();

        public int parent_index = -1;
        public List<int> children_index = new List<int>();

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        ///// Accessor methods

        // descriptions
        public string GetShortDescription()
        {
            return this.shortDescription;
        }
        public void GetShortDescription(string d)
        {
            this.shortDescription = d;
        }
        public string GetLongDescription()
        {
            return this.longDescription;
        }
        public void GetLongDescription(string d)
        {
            this.longDescription = d;
        }

        // type
        public string GetEntityType()
        {
            return this.entityType;
        }
        public void SetEntityType(string t)
        {
            this.entityType = t;
        }
        // location
        public Entity GetParent()
        {
            return this.parent;
        }
        public void SetParent(Entity parent)
        {
            this.parent = parent;
            this.parent_index = parent.index;
        }
        // refreshDelegates
        public void AddRefreshDelegate(RefreshDelegate d)
        {
            this.refreshDelegates.Add(d);
        }
        public void RemoveRefreshDelegate(RefreshDelegate d)
        {
            this.refreshDelegates.Remove(d);
        }
        // actionCallbacks
        // protected Dictionary<string, CallbackDelegate> actionCallbacks = new Dictionary<string, CallbackDelegate>();
        public void AddActionCallback(string action, CallbackDelegate d)
        {
            if (! this.actionCallbacks.ContainsKey(action))
                this.actionCallbacks[action] = new List<CallbackDelegate>();

            // TODO: check for duplicates
            this.actionCallbacks[action].Add(d);
        }
        public void RemoveActionCallback(string action, CallbackDelegate d)
        {
            if (this.actionCallbacks.ContainsKey(action))
                this.actionCallbacks[action].Remove(d);
        }
        public List<CallbackDelegate> GetActionCallbacks (string action)
        {
            if (this.actionCallbacks.ContainsKey(action))
                return actionCallbacks[action];
            return new List<CallbackDelegate>();
        }
        public bool RunActionCallbacks(Entity subject, string action)
        {
            bool passed = true;
            foreach (CallbackDelegate d in this.GetActionCallbacks(action))
            {
                passed = passed & d(subject);
            }
            return passed;
        }

        // tags
        public List<string> GetTags()
        {
            return this.tags;
        }
        protected void AddTag(string tag, Entity e)
        {
            if (! this.descendantsByTag.ContainsKey(tag))
                this.descendantsByTag.Add(tag, new List<Entity>());
            this.descendantsByTag[tag].Add(e);
        }
        protected void RemoveTag(string tag, Entity e)
        {
            if (this.descendantsByTag.ContainsKey(tag))
                this.descendantsByTag[tag].Remove(e);
        }

        // children
        public bool AddChild(Entity child)
        {
            // bomb out if this is already a child
            foreach (Entity c in this.children)
                if (c == child)
                    return false;

            // or if any parent rejects this descendant for some reason
            if (this.parent != null)
                if (! this.parent.AddDescendant(child))
                    return false;

            this.children.Add(child);
            foreach (string tag in child.GetTags())
                this.AddTag(tag, child);

            this.children_index.Add(child.index);

            this.Refresh();
            return true;
        }
        public bool RemoveChild(Entity child)
        {
            // XXX not sure if we should block remove on failure or just continue
            if (this.parent != null)
                if (! this.parent.RemoveDescendant(child))
                    return false;

            this.children.Remove(child);
            this.children_index.Remove(child.index);
            foreach (string tag in child.GetTags())
                this.RemoveTag(tag, child);
            this.Refresh();
            return true;
        }
        // descendants
        public bool AddDescendant(Entity descendant)
        {
            foreach (Entity d in this.descendants)
                if (d == descendant)
                    return false;

            if (this.parent != null)
                if (! this.parent.AddDescendant(descendant))
                    return false;

            this.descendants.Add(descendant);
            foreach (string tag in descendant.GetTags())
                this.AddTag(tag, descendant);
            this.Refresh();
            return true;
        }
        public bool RemoveDescendant(Entity descendant)
        {
            if (this.parent != null)
                if (! this.parent.RemoveDescendant(descendant))
                    return false;

            this.descendants.Remove(descendant);
            foreach (string tag in descendant.GetTags())
                this.RemoveTag(tag, descendant);
            this.Refresh();
            return true;
        }

        ///// Other methods
        public bool MoveTo(Entity newparent)
        {
            Entity oldLocation = this.parent;
            if (oldLocation != null)
            {
                if (oldLocation.RemoveChild(this))
                {
                    if (newparent.AddChild(this))
                        return true;
                }
                else // removing has failed.  Put it back.
                {
                    oldLocation.AddChild(this);
                }
            }
            else if (newparent.AddChild(this))
                    return true;
            return false;
        }

        public List<Entity> GetAllParents()
        {
            List<Entity> ret = new List<Entity>();
            if (this.parent != null)
            {
                ret.Add(this.parent);
                ret.AddRange(this.parent.GetAllParents());
            }
            return ret;
        }

        // get our POI, level, x/y coordinates, etc.

        // refresh our subscribers
        protected void Refresh() {
            foreach (RefreshDelegate d in this.refreshDelegates)
                d();
        }

    }
}
