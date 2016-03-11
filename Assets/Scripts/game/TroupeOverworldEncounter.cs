using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThugLib;


namespace ThugLib
{
    public class TroupeOverworldEncounter
    {

        NameUtils nameUtils = new NameUtils();

        // encounter types are listed here.  They will be picked at random from a joined list of the
        // common ones and the special ones to the zone.

        string[] commonEncounters = {
            "dead end",
            "vehicle breakdown",
            "vehicle crash",
            "bad water",
            "food spoiled",
            "bandits",
            "disease",
        };

        string[] grassEncounters = {
            "berries",
            "berries", // repeat increases likelihood
            "bad water",
            "food spoiled",
            "buffalo fight",
            "buffalo fight",
            "giant locust fight",
            "performer - musician",
            "robot - household",
            "robot - household"
        };

        string[] treeEncounters = {
            "vehicle breakdown",
            "berries",
            "berries",
            "bandits",
            "bear fight",
            "bear fight",
            "yeti fight",
            "performer - acrobat",
            "vehicle crash",
            "robot - lumberjack"
        };

        string[] mountainEncounters = {
            "bad water",
            "no water",
            "vehicle breakdown",
            "berries",
            "bandits",
            "disease",
            "bear fight",
            "yeti fight",
            "yeti fight",
            "performer - mystic",
            "vehicle crash",
            "robot - lumberjack",
            "robot - military",
            "robot - mule",
            "robot - mule",
        };

        string[] desertEncounters = {
            "bad water",
            "no water",
            "no water",
            "food spoiled",
            "disease",
            "buffalo fight",
            "giant locust fight",
            "giant locust fight",
            "performer - knife thrower",
            "robot - household",
            "robot - military",
            "robot - military",
            "robot - mule",
        };

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
                    {
                        gm.CallbackMenu(
                            "Leaving the highway is dangerous.  Are you sure?",
                            new[] {
                                new GameManagerScript.MenuCallback("Yes", delegate() {
                                    gm.playerScript.actor.entity.attrs["acknowledged_road_exit"] = "true";
                                    // also do a real encounter
                                    TroupeOverworldEncounter encounter = new TroupeOverworldEncounter(gm, newX, newY);
                                    if (encounter.callback != null)
                                        encounter.callback();
                                }),
                                new GameManagerScript.MenuCallback("No", delegate() { gm.playerScript.ForceMoveTo(oldX,oldY); })
                            }
                        );
                        return;
                    }

                };
            }
            if (newCell.terrain.shortDescription == "road" || newCell.terrain.shortDescription == "city")
                return;

            // now the regular callbacks

            List<string> callbackSelection = new List<string>();

            callbackSelection.AddRange(commonEncounters);

            switch(newCell.terrain.shortDescription)
            {
                case "grass":
                    if (Random.Range(0,5) > 0)
                        return;
                    callbackSelection.AddRange(grassEncounters);
                    break;
                case "trees":
                    if (Random.Range(0,4) > 0)
                        return;
                    callbackSelection.AddRange(treeEncounters);
                    break;
                case "desert":
                    if (Random.Range(0,3) > 0)
                        return;
                    callbackSelection.AddRange(desertEncounters);
                    break;
                case "mountain":
                    if (Random.Range(0,3) > 0)
                        return;
                    callbackSelection.AddRange(mountainEncounters);
                    break;
            }

            string selectedEncounter = callbackSelection[Random.Range(0,callbackSelection.Count)];

            PlayerScript player = gm.playerScript;

            string performerName;
            string fullName;

            switch(selectedEncounter)
            {
                case "bad water":
                    gm.Message("Bad water.  Everyone consumes an extra share of food.");
                    player.SetFood(player.GetFood() - player.FoodConsumption(), false);
                    break;
                case "no water":
                    gm.Message("You have run out of water.  Everyone consumes an extra share of food.");
                    player.SetFood(player.GetFood() - player.FoodConsumption(), true);
                    break;
                case "berries":
                    int bonusFood = player.CountPerformers() * Random.Range(2,4);
                    gm.Message("You found some wild berries!  You and the performers collected " + bonusFood + " food.");
                    player.SetFood(player.GetFood() + bonusFood);
                    break;
                case "dead end":
                    int foodDec = Mathf.Max(1, player.FoodConsumption() / 2);
                    gm.Message("You came to a dead end on the trail and had to set up camp.  " + foodDec + " food was consumed.");
                    player.SetFood(player.GetFood() - player.FoodConsumption(), false);
                    break;
                case "disease":
                    string randomPerformer = player.RandomPerformer();
                    if (player.PerformerHasDisease(randomPerformer))
                    {
                        string dp = player.GetDiseasedPerformer(randomPerformer);
                        string disease = dp.Split(new string[] {" has "}, System.StringSplitOptions.None)[1];
                        gm.Message(randomPerformer + " has died of " + disease + ".");
                        player.RemovePerformer(randomPerformer);
                    }
                    else
                    {
                        string diseaseName = nameUtils.RandomDiseaseName();
                        player.AddPerformerDisease(randomPerformer, diseaseName);
                        gm.Message(randomPerformer + " has " + diseaseName + ".");
                    }
                    break;
                case "food spoiled":
                    int food = player.GetFood();
                    if (food > 0)
                    {
                        int s = (int)Mathf.Max((float)food * 0.2f, 1f);
                        gm.Message(s + " food has spoiled.");
                        player.SetFood(food - s);
                    }
                    break;
                case "performer - acrobat":
                    performerName = nameUtils.RandomPersonName();
                    fullName      = performerName + " the acrobat";
                    player.AddPerformer(performerName, "acrobat");
                    gm.Message(fullName + " has joined your circus!");
                    break;
                case "performer - musician":
                    performerName = nameUtils.RandomPersonName();
                    fullName      = performerName + " the musician";
                    player.AddPerformer(performerName, "musician");
                    gm.Message(fullName + " has joined your circus!");
                    break;
                case "performer - mystic":
                    performerName = nameUtils.RandomPersonName();
                    fullName      = performerName + " the magician";
                    player.AddPerformer(performerName, "magician");
                    gm.Message(fullName + " has joined your circus!");
                    break;
                case "performer - knife thrower":
                    performerName = nameUtils.RandomPersonName();
                    fullName      = performerName + " the knife thrower";
                    player.AddPerformer(performerName, "knife thrower");
                    gm.Message(fullName + " has joined your circus!");
                    break;
                case "bandits":
                    break;
                case "bear fight":
                    break;
                case "buffalo fight":
                    break;
                case "vehicle breakdown":
                    break;
                case "vehicle crash":
                    break;
                case "yeti fight":
                    break;
                case "giant locust fight":
                    break;
//                case "robot - household"
//                    break;
//                case "robot - household":
//                    break;
//                case "robot - lumberjack"
//                    break;
//                case "robot - lumberjack":
//                    break;
//                case "robot - military":
//                    break;
//                case "robot - mule":
//                    break;
                default:
                    Debug.Log("Unhandled encounter: " + selectedEncounter);
                    break;
            }

        }

    }
}


