﻿using UnityEngine;
using System.Collections;
using ThugLib;

public class PlayerScript : MonoBehaviour {

    public float timeUnitsPerTurn = 1f;

    private GameManagerScript gm;
    private LevelManagerScript lm;
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

        CellEntity c = lm.entity.GetCell(x, y);

        this.actor.entity.SetParent(c);
        c.ActorForceEnter(this.actor.entity);
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
                int x = keyboardX;
                int y = keyboardY;

                if ( (x != 0) || (y != 0) )
                {
                    x += this.actor.entity.GetX();
                    y += this.actor.entity.GetY();
                    actor.MoveTo(x,y);
                }
                keyboardX = 0;
                keyboardY = 0;
            }
        }
        wasMoving = (actor == null) ? false : actor.moving;
    }

}