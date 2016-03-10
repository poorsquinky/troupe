using UnityEngine;
using System.Collections;
using ThugLib;

public class PlayerScript : MonoBehaviour {

    public float timeUnitsPerTurn = 1f;

    private GameManagerScript gm;
    public LevelManagerScript lm;
    public ActorScript actor;

    private bool wasMoving;

    public int keyboardX = 0;
    public int keyboardY = 0;

    void Awake ()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
        lm = gm.lm;
        this.actor = GetComponent<ActorScript>();
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

    void Moved()
    {
        int x = this.actor.entity.GetX();
        int y = this.actor.entity.GetY();
        if (gm.IsOverworldActive())
            gm.UpdateOverworldCoords(x,y);
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

        this.actor.entity.stats["mhp"] = 8;
        this.actor.entity.stats["hp"]  = 4;

    }

    void FixedUpdate()
    {
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
                                    "Talk to the " + a.shortDescription,
                                    d
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
                        actor.MoveTo(x,y);
                        Moved();
                    }
                }
                keyboardX = 0;
                keyboardY = 0;
            }
        }
        wasMoving = (actor == null) ? false : actor.moving;
    }

}
