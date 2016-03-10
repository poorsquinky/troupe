using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;
using UnityEngine;

namespace ThugSimpleGame {
    public class TroupeOfficeMapManager : MapManager {

        private LevelManagerScript lm;

        public TroupeOfficeMapManager(LevelManagerScript l) {
            lm = l;
            Bounds = new MapRectangle(0, 0, lm.levelWidth, lm.levelHeight);
        }

        public override void Generate()
        {

            int [,] map = new int[lm.levelWidth,lm.levelHeight];

            for (int x = 0; x < lm.levelWidth; x++)
            {
                for (int y = 0; y < lm.levelHeight; y++)
                {
                    map[x,y] = 0;
                }
            }

            int leftWall   = Random.Range(5,10);
            int rightWall  = Random.Range(lm.levelWidth - 10,lm.levelWidth - 5);
            int bottomWall = Random.Range(8,12);
            int topWall    = Random.Range(lm.levelHeight - 10,lm.levelHeight - 5);

            for (int x = leftWall; x <= rightWall; x++)
            {
                map[x,topWall]    = 1;
                map[x,bottomWall] = 1;
            }
            for (int y = bottomWall; y <= topWall; y++)
            {
                map[leftWall, y]  = 1;
                map[rightWall,y] = 1;
            }
            for (int y = bottomWall + 1; y < topWall; y++)
            {
                for (int x = leftWall + 1; x < rightWall; x++)
                    map[x,y] = 2;
            }
            map[Random.Range(leftWall + 5, rightWall - 5),bottomWall] = 3;


            for (int x = 0; x < lm.levelWidth; x++)
            {
                for (int y = 0; y < lm.levelHeight; y++)
                {
                    if (map[x,y] == 0 && Random.Range(0,10) == 0)
                        map[x,y] = 4;
                }
            }

            for (int x = leftWall - 2; x <= rightWall + 2; x++)
            {
                for (int y = bottomWall - 2; y <= topWall + 2; y++)
                {
                    if (map[x,y] == 0 && Random.Range(0,10) == 0)
                        map[x,y] = 1;
                }
            }
            for (int x = 0; x < lm.levelWidth; x++)
            {
                int i = Random.Range(0,20);
                if (i < 2)
                    map[x,0] = 0;
                else if (i > 18)
                    map[x,0] = 4;
                else
                    map[x,0] = 5;
                i = Random.Range(0,20);
                if (i < 2)
                    map[x,lm.levelHeight - 1] = 0;
                else
                    map[x,lm.levelHeight - 1] = 5;
            }
            for (int y = 0; y < lm.levelHeight; y++)
            {
                int i = Random.Range(0,20);
                if (i < 2)
                    map[0,y] = 0;
                else if (i > 18)
                    map[0,y] = 4;
                else
                    map[0,y] = 5;
                i = Random.Range(0,20);
                if (i < 2)
                    map[lm.levelWidth - 1,y] = 0;
                else
                    map[lm.levelWidth - 1,y] = 5;
            }

            for (int x = 0; x < lm.levelWidth; x++)
            {
                for (int y = 0; y < lm.levelHeight; y++)
                {
                    CellEntity       cell = lm.entity.GetCell(x,y);
                    TerrainEntity terrain = new TerrainEntity();
                    terrain.index         = lm.gm.entity.RegisterEntity(terrain);
                    terrain.SetCell(cell);
                    switch (map[x,y])
                    {
                        case 0:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "dirt";
                            terrain.longDescription  = "Dirt";
                            break;
                        case 1:
                            terrain.hindrance        = 1f;
                            terrain.opacity          = 1f;
                            terrain.shortDescription = "wall";
                            terrain.longDescription  = "A Wall";
                            break;
                        case 2:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "floor";
                            terrain.longDescription  = "A floor";
                            break;
                        case 3:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 1f;
                            terrain.shortDescription = "door";
                            terrain.longDescription  = "A door";
                            break;
                        case 4:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "grass";
                            terrain.longDescription  = "Grass.";
                            break;
                        case 5:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "road";
                            terrain.longDescription  = "A crumbling road.";
                            break;
                    }
                }
            }

            // let's fill the level with baddies
            //for (int x = 0; x < Random.Range(1,5) + Random.Range(1,5); x++)
            for (int x = 0; x < 20; x++)
            {
                // one baddie
                ActorEntity person = new ActorEntity();
                person.shortDescription  = "bandit";
                person.attrs["sprite"]  = "person";
                person.attrs["hostile"] = "true";
                person.attrs["npcType"] = "person";
                lm.entity.GetCell(Random.Range(2,lm.levelWidth - 3),Random.Range(2,lm.levelHeight - 3)).ActorForceEnter(person);
            }


        }
        public override void PostProcess()
        {
            for (int x = 0; x < lm.levelWidth; x++)
            {
                lm.entity.GetCell(x,0).AddActionCallback("_enter", delegate(Entity e)
                {
                    ActorEntity actor = e as ActorEntity;
                    if (actor.isPlayer != true)
                        return false;
                    lm.gm.ActivateCircus();
                    return false;
                });
                lm.entity.GetCell(x,lm.levelHeight - 1).AddActionCallback("_enter", delegate(Entity e)
                {
                    ActorEntity actor = e as ActorEntity;
                    if (actor.isPlayer != true)
                        return false;
                    lm.gm.ActivateCircus();
                    return false;
                });
            }
            for (int y = 0; y < lm.levelHeight; y++)
            {
                lm.entity.GetCell(0,y).AddActionCallback("_enter", delegate(Entity e)
                {
                    ActorEntity actor = e as ActorEntity;
                    if (actor.isPlayer != true)
                        return false;
                    lm.gm.ActivateCircus();
                    return false;
                });
                lm.entity.GetCell(lm.levelWidth - 1,y).AddActionCallback("_enter", delegate(Entity e)
                {
                    ActorEntity actor = e as ActorEntity;
                    if (actor.isPlayer != true)
                        return false;
                    lm.gm.ActivateCircus();
                    return false;
                });
            }
        }


        public override CellEntity[][] GetPatch(MapRectangle region)
        {
            return lm.entity.cells;
        }
    }
}
