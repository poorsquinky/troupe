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

        public class MapNode
        {
            public int x, y;
            public MapNode(int x, int y)
            {
                this.x = x; // x coord
                this.y = y; // y coord
            }
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
                for (int y = bottomWall; y <= topWall; y++)
                {
                    map[x,y] = 1;
                }
            }

            // put the stairs in
            MapNode upStairs   = new MapNode(Random.Range(leftWall + 5, rightWall - 5), Random.Range(bottomWall + 5, topWall - 5));
            MapNode downStairs = new MapNode(Random.Range(leftWall + 5, rightWall - 5), Random.Range(bottomWall + 5, topWall - 5));

            int doorX = Random.Range(leftWall + 5, rightWall - 5);
            map[doorX,bottomWall]     = 3;
            map[doorX - 1,bottomWall] = 3;

            MapNode midHall = new MapNode(doorX,upStairs.y - (upStairs.y - downStairs.y) / 2);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (map[upStairs.x + x, upStairs.y + y] == 1)
                        map[upStairs.x + x, upStairs.y + y] = 2;
                    if (map[downStairs.x + x, downStairs.y + y] == 1)
                        map[downStairs.x + x, downStairs.y + y] = 2;
                    if (map[midHall.x + x, midHall.y + y] == 1)
                        map[midHall.x + x, midHall.y + y] = 2;
                }
            }
            int dx1 = 0;
            int dy1 = 0;
            int dx2 = 0;
            int dy2 = 0;
            int dx3 = 0;
            int dy3 = 0;
            int wallChoice = Random.Range(0,4);
            switch (wallChoice)
            {
                case 0:
                    dx1 = 1;
                    break;
                case 1:
                    dx1 = -1;
                    break;
                case 2:
                    dy1 = 1;
                    break;
                default:
                    dy1 = -1;
                    break;
            }
            wallChoice = Random.Range(0,4);
            switch (wallChoice)
            {
                case 0:
                    dx2 = 1;
                    break;
                case 1:
                    dx2 = -1;
                    break;
                case 2:
                    dy2 = 1;
                    break;
                default:
                    dy2 = -1;
                    break;
            }
            wallChoice = Random.Range(0,4);
            switch (wallChoice)
            {
                case 0:
                    dx3 = 1;
                    break;
                case 1:
                    dx3 = -1;
                    break;
                case 2:
                    dy3 = 1;
                    break;
                default:
                    dy3 = -1;
                    break;
            }
            int hx = upStairs.x;
            int hy = upStairs.y;
            while (hx > leftWall && hx < rightWall && hy > bottomWall && hy < topWall)
            {
                if (map[hx,hy] == 1)
                    map[hx,hy] = 3;
                hx += dx1;
                hy += dy1;
            }
            hx = downStairs.x;
            hy = downStairs.y;
            while (hx > leftWall && hx < rightWall && hy > bottomWall && hy < topWall)
            {
                if (map[hx,hy] == 1)
                    map[hx,hy] = 3;
                hx += dx2;
                hy += dy2;
            }
            hx = midHall.x;
            hy = midHall.y;
            while (hx > leftWall && hx < rightWall && hy > bottomWall && hy < topWall)
            {
                if (map[hx,hy] == 1)
                    map[hx,hy] = 3;
                hx += dx3;
                hy += dy3;
            }

            map[upStairs.x,upStairs.y]     = 6;
            map[downStairs.x,downStairs.y] = 7;

            // CA hollow out
            int[,] newmap = (int[,]) map.Clone();

            for (int x = leftWall; x <= rightWall; x++)
            {
                for (int y = bottomWall; y <= topWall; y++)
                {
                    int wallcount = 0;
                    for (int x1 = x - 1; x1 < x + 2; x1++)
                    {
                        for (int y1 = y - 1; y1 < y + 2; y1++)
                        {
                            if (x1 > 0 && x1 < lm.levelWidth && y1 > 0 && y1 < lm.levelHeight)
                            {
                                switch (map[x1,y1])
                                {
                                    case 1:
                                        wallcount++;
                                        break;
                                    case 3:
                                        wallcount++;
                                        break;
                                }
                            }
                        }
                    }
                    if (wallcount == 9)
                        newmap[x,y] = 2;
                }
            }
            map = (int[,]) newmap.Clone();

            int tries = 0;

            while (tries < 20)
            {
                tries++;
                MapNode rn = new MapNode(Random.Range(leftWall + 1, rightWall - 1), Random.Range(bottomWall + 1, topWall - 1));
                if (map[rn.x,rn.y] == 2)
                {
                    // check the size of the surrounding space
                    int w = 1;
                    int h = 1;
                    hx = rn.x - 1;
                    hy = rn.y;
                    while (map[hx,hy] == 2)
                    {
                        w++;
                        hx--;
                    }
                    hx = rn.x + 1;
                    while (map[hx,hy] == 2)
                    {
                        w++;
                        hx++;
                    }
                    hx = rn.x;
                    hy = rn.y - 1;
                    while (map[hx,hy] == 2)
                    {
                        h++;
                        hy--;
                    }
                    hy = rn.y + 1;
                    while (map[hx,hy] == 2)
                    {
                        h++;
                        hy++;
                    }
                    if (w >3 && h > 3) // we are go for split
                    {
                        map[rn.x,rn.y] = 3;
                        if (w > h)
                        {
                            hx = rn.x;
                            hy = rn.y + 1;
                            while (map[hx,hy] == 2)
                            {
                                map[hx,hy] = 1;
                                hy++;
                            }
                            hy = rn.y - 1;
                            while (map[hx,hy] == 2)
                            {
                                map[hx,hy] = 1;
                                hy--;
                            }
                        }
                        else
                        {
                            hx = rn.x + 1;
                            hy = rn.y;
                            while (map[hx,hy] == 2)
                            {
                                map[hx,hy] = 1;
                                hx++;
                            }
                            hx = rn.x - 1;
                            while (map[hx,hy] == 2)
                            {
                                map[hx,hy] = 1;
                                hx--;
                            }
                        }
                    }

                }
            }


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
                    if (map[x,y] == 0 && Random.Range(0,15) == 0)
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
                        case 6:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "stairway up";
                            terrain.longDescription  = "A staircase leading up.";
                            break;
                        case 7:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "stairway down";
                            terrain.longDescription  = "A staircase leading down.";
                            break;
                    }
                }
            }

            // let's fill the level with baddies
            //for (int x = 0; x < 20; x++)
            for (int x = 0; x < Random.Range(1,5) + Random.Range(1,5); x++)
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

        public bool exitDelegate(Entity e, Entity self)
        {
            ActorEntity actor = e as ActorEntity;
            if (actor.isPlayer != true)
                return false;

            lm.gm.CallbackMenu(
                "Do you really want to leave this place?",
                new[] {
                    new GameManagerScript.MenuCallback("Yes", delegate() {
                        PlaceEntity parentEntity = lm.entity.parent.parent as PlaceEntity;
                        parentEntity.Activate();
                    }),
                    new GameManagerScript.MenuCallback("No", delegate() { return; })
                }
            );
            return false;
        }

        public override void PostProcess()
        {

            for (int x = 0; x < lm.levelWidth; x++)
            {
                lm.entity.GetCell(x,0).AddActionCallback("_enter", exitDelegate);
                lm.entity.GetCell(x,lm.levelHeight - 1).AddActionCallback("_enter", exitDelegate);
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
    }
}
