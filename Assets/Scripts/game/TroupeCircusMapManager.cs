using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

// FIXME unity stuff is here because I'm lazy/sloppy
using UnityEngine;

namespace ThugSimpleGame {
    public class TroupeCircusMapManager : MapManager {

        private NameUtils nameUtils = new NameUtils();

        private LevelManagerScript lm;

        public TroupeCircusMapManager(LevelManagerScript l) {
            lm = l;
            Bounds = new MapRectangle(0, 0, lm.levelWidth, lm.levelHeight);

        }

        public override void Generate()
        {
            int[,] prefabCircus = new int[,] {
                {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1},
                {1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1},
                {1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1},
                {1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1},
                {1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            };
            for (int x = 0; x < lm.levelWidth; x++)
            {
                for (int y = 0; y < lm.levelHeight; y++)
                {
                    CellEntity       cell = lm.entity.GetCell(x,y);
                    TerrainEntity terrain = new TerrainEntity();
                    terrain.index         = lm.gm.entity.RegisterEntity(terrain);
                    terrain.SetCell(cell);
                    switch (prefabCircus[lm.levelHeight - y - 1,x])
                    {
                        case 0:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "grass";
                            terrain.longDescription  = "Grass";
                            break;
                        case 1:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "dirt";
                            terrain.longDescription  = "A patch of bare dirt";
                            break;
                        case 2:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "road";
                            terrain.longDescription  = "A crumbling but still-functional stretch of road";
                            break;
                        case 3:
                            terrain.hindrance        = 1f;
                            terrain.opacity          = 1f;
                            terrain.shortDescription = "wall";
                            terrain.longDescription  = "A city wall cobbled together from junk and debris";
                            break;
                    }
                }
            }

            // add the signpost
            int xx = 25;
            int yy = 42;
            PropEntity signpostEntity = new PropEntity();
            signpostEntity.shortDescription = "a signpost";
            signpostEntity.SetCell(lm.entity.GetCell(xx,yy));

            // fake npcs
            xx = 26;
            yy = 46;
            PropEntity rightFakeNpcEntity = new PropEntity();
            rightFakeNpcEntity.shortDescription = "a city gate guard";
            rightFakeNpcEntity.SetCell(lm.entity.GetCell(xx,yy));
            xx = 24;
            yy = 46;
            PropEntity leftFakeNpcEntity = new PropEntity();
            leftFakeNpcEntity.shortDescription = "a city gate guard";
            leftFakeNpcEntity.SetCell(lm.entity.GetCell(xx,yy));

            // door
            xx = 25;
            yy = 47;
            PropEntity doorEntity = new PropEntity();
            doorEntity.shortDescription = "a locked city gate";
            doorEntity.SetCell(lm.entity.GetCell(xx,yy));

            for (int x = 20; x < 31; x += 4)
            {
                // one monkey
                ActorEntity monkey = new ActorEntity();
                monkey.shortDescription = "monkey";
                monkey.longDescription  = nameUtils.RandomPersonName() + " the monkey";
                monkey.attrs["sprite"]  = "monkey";
                monkey.attrs["hostile"] = "false";
                monkey.attrs["npcType"] = "monkey";
                lm.entity.GetCell(x,38).ActorForceEnter(monkey);
            }

            // some children
            ActorEntity c = new ActorEntity();
            c.shortDescription = "child";
            c.longDescription  = nameUtils.RandomPersonName();
            c.attrs["sprite"]  = "child";
            c.attrs["hostile"] = "false";
            c.attrs["npcType"] = "child";
            lm.entity.GetCell(25,41).ActorForceEnter(c);

        }

        public bool exitDelegate(Entity e, Entity self)
        {
            ActorEntity actor = e as ActorEntity;
            if (actor.isPlayer != true)
                return false;

            lm.gm.CallbackMenu(
                "Do you really want to exit and return to the world map?",
                new[] {
                    new GameManagerScript.MenuCallback("Yes", delegate() { lm.gm.ActivateOverworld(); }),
                    new GameManagerScript.MenuCallback("No", delegate() { return; })
                }
            );
            return false;
        }

        public override void PostProcess()
        {

            foreach (ActorEntity actor in lm.entity.GetAllActors())
            {
                if (actor.attrs.ContainsKey("npcType"))
                {
                    switch(actor.attrs["npcType"])
                    {
                        case "child":
                            actor.AddActionCallback("talk", delegate(Entity e, Entity self)
                            {
                                string[] messages = {
                                    "\"Wow, are you with the circus?\"",
                                    "\"Mom says I shouldn't talk to strangers.\"",
                                    "\"Are you a bandit?\"",
                                    "\"Are you a mutant?\"",
                                    "\"Are you a mutant bandit?\"",
                                    "\"My name is " + self.longDescription + ".  What's yours?\"",
                                    "\"The virus happened before I was born.  Everyone says things were better then.\""
                                };
                                lm.gm.Message(messages[Random.Range(0,messages.Length)]);
                                return true;
                            });
                            break;
                    }
                }
            }

            CellEntity gateTriggerCell = lm.entity.GetCell(25,42);
            gateTriggerCell.AddActionCallback("_enter", delegate(Entity e, Entity self)
                    {
                        ActorEntity actor = e as ActorEntity;
                        if (actor.isPlayer != true)
                            return false;
                        lm.gm.CallbackMenu(
                            "This is the signpost.  It will be used for fast travel.",
                            new[] {
                                new GameManagerScript.MenuCallback("Exit to world map", delegate() { lm.gm.ActivateOverworld(); }),
                                new GameManagerScript.MenuCallback("Try a nearby office building", delegate() { lm.gm.ActivateOffice("foo"); }),
                                new GameManagerScript.MenuCallback("Never mind", delegate() { return; })
                            }
                        );
                        return false;
                    });


            // entryway trigger
            gateTriggerCell = lm.entity.GetCell(25,46);
            gateTriggerCell.AddActionCallback("_enter", delegate(Entity e, Entity self)
                    {
                        ActorEntity actor = e as ActorEntity;
                        if (actor.isPlayer == true)
                        {
                            string[] messages = {
                                "\"That's far enough!  You can't enter the city, vagrant!\"",
                                "\"Only citizens or travelers with a visa may enter.\"",
                                "\"Buzz off, entertainer!\"",
                                "\"None shall pass!\"",
                                "The city guard glares at you and shakes his head."
                            };
                            lm.gm.Message(messages[Random.Range(0,messages.Length)]);
                            return false;
                        }
                        return true;
                    });

            for (int x = 0; x < lm.levelWidth; x++)
            {
                lm.entity.GetCell(x,0).AddActionCallback("_enter", exitDelegate);
            }
            for (int y = 0; y < lm.levelHeight; y++)
            {
                lm.entity.GetCell(0,y).AddActionCallback("_enter", exitDelegate);
                lm.entity.GetCell(lm.levelWidth - 1,y).AddActionCallback("_enter", exitDelegate);
            }

        }

        public override CellEntity[][] GetPatch(MapRectangle region)
        {
            return lm.entity.cells;
        }

        /*
        public override MapSpaceType[][] GetPatch(MapRectangle region) {
            MapSpaceType[][] patch = new MapSpaceType[region.w][];
            for (int i = 0; i < region.w; i++) {
                patch[i] = new MapSpaceType[region.h];
                for (int j = 0; j < region.h; j++) {
                    patch[i][j] = this.palette[this.grid[i][j]];
                }
            }
            return patch;
        }
        */
    }
}
