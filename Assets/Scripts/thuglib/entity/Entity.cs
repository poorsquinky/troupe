using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThugLib
{
    public class Entity
    {
        [SerializeField]
        public string serialFieldsString;
        public Dictionary<string,string> serialFields = new Dictionary<string,string>();

        [SerializeField]
        public string attrsString; // this name is almost as stupid as I am
        public Dictionary<string,string> attrs = new Dictionary<string,string>();

        [SerializeField]
        public string statsString;
        public Dictionary<string,int> stats = new Dictionary<string,int>();

        [SerializeField]
        public string entityType = "NONE"; // we're abusing strings for a lot of these things for extensibility

        [SerializeField]
        public int index;

        public Entity parent;

        [SerializeField]
        public string shortDescription;
        [SerializeField]
        public string longDescription;

        // FIXME (later): unity dependency in thuglib
        [SerializeField]
        public int entitySeed = 0;
        //public int entitySeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        // For the benefit of Unity scripts, refreshDelegates are a list
        // of subscribers that will be informed whenever this Entity
        // changes at all.  This is due to C# not supporting multiple
        // inheritance because they hate freedom.

        public delegate void RefreshDelegate();
        protected List<RefreshDelegate> refreshDelegates = new List<RefreshDelegate>();

        public delegate bool CallbackDelegate(Entity subject, Entity target);
        protected Dictionary<string, List<CallbackDelegate>> actionCallbacks = new Dictionary<string, List<CallbackDelegate>>();

        [SerializeField]
        public List<string> tags = new List<String>();

        [SerializeField]
        protected List<Entity> children    = new List<Entity>();
        protected List<Entity> descendants = new List<Entity>();
        protected Dictionary<string, List<Entity>> descendantsByTag = new Dictionary<string, List<Entity>>();

        [SerializeField]
        public int parent_index = -1;
        [SerializeField]
        public List<int> children_index = new List<int>();

        public void Decommission(GameEntity g)
        {
            foreach (Entity child in this.children)
            {
                child.Decommission(g);
            }
            g.DeregisterEntity(this);
            this.GetParent().RemoveChild(this);
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
                passed = passed & d(subject, this);
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

        public virtual void SerializeFields()
        {
        }
        public virtual void DeserializeFields()
        {
        }

        // escape quotes in the stupidest possible way
        protected string CheesyEscape(string s)
        {
            string o = s.Replace("<<F","<<FF");
            return o;
        }
        protected string CheesyUnescape(string s)
        {
            string o = s.Replace("<<FF","<<F");
            return o;
        }

        // I hate C# right about now
        public string CheeseDict(Dictionary<string,string> dict)
        {
            string s = "";
            bool first = true;
            foreach (KeyValuePair<string,string> entry in dict)
            {
                if (!first)
                    s = s + "<<FL>>";
                s = s + entry.Key + "<<FD>>" + entry.Value;
                first = false;
            }
            return CheesyEscape(s);
        }

        public Dictionary<string,string> DeCheeseDict(string s)
        {
            Dictionary<string,string> d = new Dictionary<string,string>();
            string[] lines = CheesyUnescape(s).Split(new[] {"<<FL>>"}, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string[] entry = line.Split(new[] {"<<FD>>"}, StringSplitOptions.None);
                if (entry.Length > 1)
                    d[entry[0]] = entry[1];
            }
            return d;
        }

        // XXX TODO FIXME -- no cheesedict for stats

        public void Serialize()
        {
            this.SerializeFields();
            this.serialFieldsString = CheeseDict(this.serialFields);
            this.attrsString        = CheeseDict(this.attrs);
        }
        public void Deserialize()
        {
            this.serialFields = DeCheeseDict(this.serialFieldsString);
            this.attrs        = DeCheeseDict(this.attrsString);

            // re-initialize some values that don't serialize
            this.refreshDelegates = new List<RefreshDelegate>();
            this.actionCallbacks  = new Dictionary<string, List<CallbackDelegate>>();

            this.DeserializeFields();
        }


    }
}
