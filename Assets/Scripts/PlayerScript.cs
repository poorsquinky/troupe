using UnityEngine;
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

    void AddPerformer(string performerType, string performerName)
    {
        List<string> performers = GetPerformers(performerType);
        performers.Add(performerName);
        actor.entity.attrs["performer_" + performerType] = System.String.Join(",", performers.ToArray());
    }

    void RemovePerformer(string performerType, string performerName)
    {
        List<string> performers = GetPerformers(performerType);
        performers.Remove(performerName);
        if (performers.Count == 0)
            actor.entity.attrs["performer_" + performerType] = "";
        else
            actor.entity.attrs["performer_" + performerType] = System.String.Join(",", performers.ToArray());
    }

    List<string> GetPerformers(string performerType)
    {
        List<string> performers = new List<string>();
        if (! actor.entity.attrs.ContainsKey("performer_" + performerType) || actor.entity.attrs["performer_" + performerType] == "")
            return performers;
        performers.AddRange(actor.entity.attrs["performer_" + performerType].Split(','));
        return performers;
    }

    int CountPerformers()
    {
        int ct = 0;
        foreach (string k in actor.entity.attrs.Keys)
        {
            if (k.StartsWith("performer_"))
            {
                string performerType = k.Split('_')[1];
                ct += GetPerformers(performerType).Count;
            }
        }
        return ct;
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

        CellEntity c = lm.entity.GetCell(x, y);

        this.actor.entity.SetParent(c);
        c.ActorForceEnter(this.actor.entity);
        this.actor.TeleportTo(x,y);

    }

    void FoodCheck(int turns)
    {
        if (gm.turnCount % turns == 0)
        {
            // TODO: starvation effects?
            int foodDec = CountPerformers();
            actor.entity.stats["food"] -= foodDec;
            if (actor.entity.stats["food"] < 0)
            {
                actor.entity.stats["food"] = 0;
                lm.gm.Message("You have run out of food.");
            }
            else
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
        if (this.actor.entity.stats["hp"] <= 0)
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
                    else
                    {
                        CellEntity cell = lm.entity.GetCell(x,y);
                        ActorEntity foe = cell.GetActor();
                        if (foe != null)
                        {
                            if (foe.attrs.ContainsKey("hostile") && foe.attrs["hostile"] == "true")
                            {
                                if (foe.GetHP() == 0)
                                {
                                    lm.gm.Message("You can't attack the dead " + foe.shortDescription + "!");
                                }
                                else
                                {
                                    lm.gm.Message("You hit the " + foe.shortDescription + "!");
                                    this.actor.Attack(foe);
                                    if (foe.GetHP() < 1)
                                        lm.gm.Message("You killed the " + foe.shortDescription + "!");
                                    lm.playerTurn = false;
                                }
                            }
                            else
                            {
                                lm.gm.Message("You can't attack the " + foe.shortDescription + "!");
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
