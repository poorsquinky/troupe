using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

namespace ThugSimpleGame {
    public class NPCChaseBrain: NPCBrain {

        public NPCChaseBrain(NPCScript body) : base(body)
        {
        }

        public override void Run()
        {
            if (body.currentPath == null ||
                body.currentPathStep >= body.currentPath.Steps.Length)
            {
                if (body.lm == null)
                {
                    GameObject l = GameObject.Find("LevelManager");
                    body.lm = l.GetComponent<LevelManagerScript>();
                }
                body.currentPath = PathUtils.BFSPath(
                   (int)body.transform.position.x,
                   (int)body.transform.position.y,
                   (int)body.lm.GetPlayerX(), (int)body.lm.GetPlayerY(),
                   null, (mx, my) => body.lm.map[mx][my].Passable(), null,
                   (mx, my, d) => (((int)d) % 2 == 1) ? 7 : 5,
                   body.lm.fullMapBounds);
                body.currentPathStep = 0;
            }
            bool canMove = false;
            int x = 0;
            int y = 0;
            if (body.currentPath == null ||
                body.currentPathStep >= body.currentPath.Steps.Length )
            {
                // we either found him or can't find him; random walk
                body.brain["random"].active = true;
                return;
            }
            else
            {
                body.brain["random"].active = false;
                if (body.lm.entity.GetCell(x,y).GetActor() == null)
                {
                    x = (int)body.transform.position.x + PathUtils.StepDX[(int)
                       body.currentPath.Steps[body.currentPathStep]];
                    y = (int)body.transform.position.y + PathUtils.StepDY[(int)
                       body.currentPath.Steps[body.currentPathStep]];
                    canMove = body.actor.CanMoveTo(x, y);
                    if (canMove)
                    {
                        body.currentPathStep++;
                        body.actor.MoveTo(x, y);
                    }
                    else
                        body.currentPath = null;
                }
            }
        }
    }
}
