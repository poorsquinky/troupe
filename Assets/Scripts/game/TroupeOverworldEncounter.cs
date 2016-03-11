using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThugLib;


namespace ThugLib
{
    public class TroupeOverworldEncounter
    {

        GameManagerScript gm;
        public delegate void EncounterDelegate();

        public EncounterDelegate callback = null;

        public TroupeOverworldEncounter (GameManagerScript g, int newX, int newY)
        {
            this.gm = g;

            int oldX = gm.overworldX;
            int oldY = gm.overworldY;

            CellEntity oldCell = gm.overworldEntity.GetCell(oldX,oldY);
            CellEntity newCell = gm.overworldEntity.GetCell(newX,newY);
            if ( (oldCell.terrain.shortDescription == "road" || oldCell.terrain.shortDescription == "city") &&
                 (newCell.terrain.shortDescription != "road" && newCell.terrain.shortDescription != "city"))
            {
                this.callback = delegate() {
                    if (! gm.playerScript.actor.entity.attrs.ContainsKey("acknowledged_road_exit"))
                        gm.CallbackMenu(
                            "Leaving the highway is dangerous.  Are you sure?",
                            new[] {
                                new GameManagerScript.MenuCallback("Yes", delegate() { gm.playerScript.actor.entity.attrs["acknowledged_road_exit"] = "true"; }),
                                new GameManagerScript.MenuCallback("No", delegate() { gm.playerScript.ForceMoveTo(oldX,oldY); })
                            }
                        );
                };
            }


        }

    }
}


