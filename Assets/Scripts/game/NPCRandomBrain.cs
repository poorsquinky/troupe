using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

namespace ThugSimpleGame {
    public class NPCRandomBrain: NPCBrain {

        public NPCRandomBrain(NPCScript body) : base(body)
        {
        }

        public override void Run()
        {
            bool canMove = false;
            int x = 0;
            int y = 0;
            if (body.currentPath == null ||
                body.currentPathStep >= body.currentPath.Steps.Length)
            {
                int attempts = 0;
                while ((canMove == false) && (attempts < 20))
                {
                    x = 0;
                    y = 0;
                    attempts += 1;
                    if (Random.Range(0,1) == 0)
                        x += Random.Range(-1,2);
                    if (Random.Range(0,1) == 0)
                        y += Random.Range(-1,2);
                    if (x != 0 || y != 0)
                    {
                        x += (int)body.transform.position.x;
                        y += (int)body.transform.position.y;
                        if (body.actor.CanMoveTo(x,y))
                            canMove = true;
                        else
                            body.currentPath = null;
                    }
                }
            }
            if (canMove) body.actor.MoveTo(x, y);
        }
    }
}
