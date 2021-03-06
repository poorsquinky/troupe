﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;

public class PlayerScript : MonoBehaviour {

    public float timeUnitsPerTurn = 1f;

    private GameManagerScript gm;
    public LevelManagerScript lm;
    public ActorScript actor;

    private bool wasMoving;

    public int keyboardX = 0;
    public int keyboardY = 0;

    public void AddPerformer(string performerType, string performerName)
    {
        List<string> performers = GetPerformers(performerType);
        performers.Add(performerName);
        actor.entity.attrs["performer_" + performerType] = System.String.Join(",", performers.ToArray());
    }

    public void RemovePerformer(string performerType, string performerName)
    {
        List<string> performers = GetPerformers(performerType);
        performers.Remove(performerName);
        if (performers.Count == 0)
            actor.entity.attrs["performer_" + performerType] = "";
        else
            actor.entity.attrs["performer_" + performerType] = System.String.Join(",", performers.ToArray());
    }

    public List<string> GetPerformers(string performerType)
    {
        List<string> performers = new List<string>();
        if (! actor.entity.attrs.ContainsKey("performer_" + performerType) || actor.entity.attrs["performer_" + performerType] == "")
            return performers;
        performers.AddRange(actor.entity.attrs["performer_" + performerType].Split(','));
        return performers;
    }

    public int CountPerformers()
    {
        return ListAllPerformers().Count;
    }

    public List<string> ListAllPerformers()
    {
        List<string> performers = new List<string>();
        foreach (string k in actor.entity.attrs.Keys)
        {
            if (k.StartsWith("performer_"))
            {
                string performerType = k.Split('_')[1];
                foreach (string name in actor.entity.attrs[k].Split(','))
                {
                    if (name != "")
                        performers.Add(name + " the " + performerType);
                }
            }
        }
//        Debug.Log(System.String.Join(", ", performers.ToArray()));
        return performers;
    }

    public string RandomPerformer()
    {
        List<string> ap = ListAllPerformers();
        return ap[Random.Range(0, ap.Count)];
    }

    public bool PerformerHasDisease(string fullPerformer)
    {
        if (GetDiseasedPerformer(fullPerformer) == null)
            return false;
        return true;
    }

    public string GetDiseasedPerformer(string fullPerformer)
    {
        List<string> d = GetAllDiseasedPerformers();
        foreach (string p in d)
        {
            if (p.StartsWith(fullPerformer + " has "))
                return p;
        }
        return null;
    }

    public List<string> GetAllDiseasedPerformers()
    {
        List<string> performers = new List<string>();
        if (actor.entity.attrs.ContainsKey("diseased_performers") && actor.entity.attrs["diseased_performers"] != "")
            performers.AddRange(actor.entity.attrs["diseased_performers"].Split(','));
//        Debug.Log("DISEASED: " + System.String.Join(", ", performers.ToArray()));
        return performers;
    }
    public void RemovePerformer(string fullPerformer)
    {
        RemovePerformerDisease(fullPerformer);
        string[] pattrs = fullPerformer.Split(new string[] {" the "}, System.StringSplitOptions.None);
        RemovePerformer(pattrs[1], pattrs[0]);
    }
    public void RemovePerformerDisease(string fullPerformer)
    {
        List<string> allDiseases = GetAllDiseasedPerformers();
        List<string> newDiseases = new List<string>();
        foreach (string p in allDiseases)
        {
            string[] pattrs = p.Split(new string[] {" has "}, System.StringSplitOptions.None);
            if (pattrs[0] != fullPerformer)
                newDiseases.Add(p);
        }
        actor.entity.attrs["diseased_performers"] = System.String.Join(",", newDiseases.ToArray());
    }
    public void AddPerformerDisease(string fullPerformer, string diseaseName)
    {
        List<string> performers = GetAllDiseasedPerformers();
        string entry = fullPerformer + " has " + diseaseName;
        performers.Add(entry);
        actor.entity.attrs["diseased_performers"] = System.String.Join(",", performers.ToArray());
    }


    public int GetFood()
    {
        return actor.entity.stats["food"];
    }
    public void SetFood(int amt, bool handleStarvation)
    {
        if (amt <= 0)
        {
            actor.entity.stats["food"] = 0;
            lm.gm.Message("You have run out of food.");
            // XXX handle starvation
            if (handleStarvation)
            {
                int needed = FoodConsumption();
                int fpp = needed / CountPerformers() + 1;
                List<string> dead = new List<string>();
                for (int i = 0; i < Mathf.Abs(needed); i += fpp)
                {
                    if (Random.Range(0,1) == 0 && CountPerformers() > 0)
                    {
                        string p = RandomPerformer();
                        RemovePerformer(p);
                        dead.Add(p);
                    }
                }
                if (dead.Count > 1)
                {
                    lm.gm.Message(dead.Count + " performers have starved to death: " + System.String.Join(", ", dead.ToArray()));
                }
                else if (dead.Count == 1)
                    lm.gm.Message(dead[0] + " has died of starvation.");
            }
            else // we'll sort of handle it anyway
            {
                if (FoodConsumption() + amt < 0)
                {
                    string p = RandomPerformer();
                    RemovePerformer(p);
                    lm.gm.Message(p + " has died of starvation.");
                }
            }
        }
        else
        {
            actor.entity.stats["food"] = amt;
        }
    }
    public void SetFood(int amt)
    {
        SetFood(amt, false);
    }

    void Awake ()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
        lm = gm.lm;

        this.actor = GetComponent<ActorScript>();
        actor.entity.isPlayer     = true;
        actor.entity.stats["hp"]  = 8;
        actor.entity.stats["mhp"] = 8;

        actor.entity.stats["food"] = 50;

        NameUtils nameUtils = new NameUtils();
        for (int i = 0; i < 3; i++)
            AddPerformer("monkey", nameUtils.RandomPersonName());
        for (int i = 0; i < 2; i++)
            AddPerformer("clown", nameUtils.RandomPersonName());
        for (int i = 0; i < 2; i++)
            AddPerformer("acrobat", nameUtils.RandomPersonName());

    }

    void Start ()
    {
        this.actor.smooth = 18f;
        this.actor.entity.AddRefreshDelegate(delegate() {
            // TODO: have this do stuff
        });
    }

    public void ForceMoveTo(int x, int y)
    {
        Vector3 pos = new Vector3(x, y, 0);
        transform.position = pos;

        // force a refresh every time, in case the level changed
        lm = gm.lm;
        this.actor.lm = lm;
        this.actor.TeleportTo(x,y);

        if (actor.entity.isPlayer && lm.entity.isOverworld)
        {
            gm.overworldX = x;
            gm.overworldY = y;
        }
    }

    public int FoodConsumption()
    {
        return CountPerformers() + GetAllDiseasedPerformers().Count;
    }

    void FoodCheck(int turns)
    {
        if (gm.turnCount % turns == 0)
        {
            int food    = GetFood();
            int foodDec = FoodConsumption();
            SetFood(food - foodDec);
            if (actor.entity.stats["food"] > 0)
            {
                lm.gm.Message("Your performers ate " + foodDec + " food.");
            }
        }
    }

    void Moved()
    {
        int x = this.actor.entity.GetX();
        int y = this.actor.entity.GetY();
        if (gm.IsOverworldActive())
        {
            gm.UpdateOverworldCoords(x,y);
            CellEntity cell = lm.entity.GetCell(x,y);
            int foodTurns = 1;
            switch(cell.terrain.shortDescription)
            {
                case "road":
                    foodTurns = 5;
                    break;
                case "city":
                    foodTurns = 99999;
                    break;
                case "grass":
                    foodTurns = 4;
                    break;
                case "trees":
                    foodTurns = 3;
                    break;
                case "mountains":
                    foodTurns = 2;
                    break;
                case "desert":
                    foodTurns = 1;
                    break;
            }
            FoodCheck(foodTurns);
        }
        // refresh nearby terrain sprites
        // FIXME: don't use hardcoded distance
        for (int i = Mathf.Max(0, x - 40); i < Mathf.Max(lm.levelWidth, x + 20); i++)
        {
            for (int j = Mathf.Max(0, y - 40); j < Mathf.Max(lm.levelHeight, y + 20); j++)
            {
                GameObject tile = lm.GetTile(i,j);
                if (tile != null)
                    tile.GetComponent<ShapeTerrainScript>().ExternalUpdate();
            }
        }
        gm.ClearActionCallbacks();
        gm.UpdateHelpDisplay();
        gm.turnCount++;
    }

    public string killedBy = "something";

    void FixedUpdate()
    {
        if (CountPerformers() < 1)
            gm.CallbackMenu(
                "You have run out of performers after " + gm.turnCount + " turns.\n"
                + "You have 0 points because we have no points yet.",
                new[] {
                    new GameManagerScript.MenuCallback("Quit", delegate() { Application.Quit(); }),
                }
            );
        if (this.actor.entity.stats.ContainsKey("hp") && this.actor.entity.stats["hp"] < 1 )
            gm.CallbackMenu(
                "You were killed by " + killedBy + " after " + gm.turnCount + " turns.\n"
                + "You have 0 points because we have no points yet.",
                new[] {
                    new GameManagerScript.MenuCallback("Quit", delegate() { Application.Quit(); }),
                }
            );
        if (actor != null && !actor.moving)
        {
            if (wasMoving)
            {
                lm.VisibilityUpdate();
                lm.playerTurn = false;
            }
            else if (lm.playerTurn && !gm.menuActive)
            {

                gm.ClearActionCallbacks();
                foreach (CellEntity cell in this.actor.entity.GetCell().GetNeighbors())
                {
                    ActorEntity a = cell.GetActor();
                    if (a != null)
                    {
                        if (a.GetActionCallbacks("talk").Count > 0)
                        {
                            foreach (Entity.CallbackDelegate d in a.GetActionCallbacks("talk"))
                            {
                                gm.AddActionCallback(
                                    "Talk to " + a.longDescription,
                                    d,
                                    a as Entity
                                );
                            }
                        }
                    }
                }
                gm.UpdateHelpDisplay();

                int x = keyboardX;
                int y = keyboardY;

                if ( (x != 0) || (y != 0) )
                {
                    x += this.actor.entity.GetX();
                    y += this.actor.entity.GetY();
                    if (actor.CanMoveTo(x,y))
                    {
                        if (lm.isOverworld)
                        {
                            TroupeOverworldEncounter encounter = new TroupeOverworldEncounter(gm, x, y);
                            if (encounter.callback != null)
                                encounter.callback();
                        }
                        actor.MoveTo(x,y);
                        Moved();
                    }
                    else if (x >= 0 && y >= 0 && x < lm.levelWidth - 1 && y < lm.levelHeight - 1)
                    {
                        CellEntity cell = lm.entity.GetCell(x,y);
                        ActorEntity foe = cell.GetActor();
                        if (foe != null)
                        {
                            if (foe.attrs.ContainsKey("hostile") && foe.attrs["hostile"] == "true")
                            {
                                lm.gm.Message("You hit the " + foe.shortDescription + "!");
                                this.actor.Attack(foe);
                                if (foe.GetHP() < 1)
                                    lm.gm.Message("You killed the " + foe.shortDescription + "!");
                                lm.playerTurn = false;
                            }
                            else
                            {
                                lm.gm.Message("You can't attack " + foe.longDescription + "!");
                            }
                        }
                    }
                }
                keyboardX = 0;
                keyboardY = 0;
            }
        }
        wasMoving = (actor == null) ? false : actor.moving;
    }

}
