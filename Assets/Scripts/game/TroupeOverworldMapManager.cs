using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;
using UnityEngine;

namespace ThugSimpleGame {
    public class TroupeOverworldMapManager : MapManager {

        private LevelManagerScript lm;

        public TroupeOverworldMapManager(LevelManagerScript l) {
            lm = l;
            Bounds = new MapRectangle(0, 0, lm.levelWidth, lm.levelHeight);
        }

        public class MapNode
        {
            public int x, y, r, w, h;
            public MapNode(int x, int y, int r, int w, int h)
            {
                this.x = x; // x coord
                this.y = y; // y coord
                this.r = r; // radius of repel
                this.w = w; // max width of map
                this.h = h; // max height of map
            }

            public float DistanceToNode(MapNode n)
            {
                float deltaX = Mathf.Abs(this.x - n.x);
                float deltaY = Mathf.Abs(this.y - n.y);
                return Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
            }

            public MapNode FindNearest(List<MapNode> list)
            {
                float d = 999f;
                MapNode candidate = null;
                foreach (MapNode n in list)
                {
                    if (x != n.x || y != n.y)
                    {
                        float dd = DistanceToNode(n);
                        if (dd < d)
                        {
                            d = dd;
                            candidate = n;
                        }
                    }
                }
                return candidate;
            }

            public void Repel(MapNode n)
            {
                float d = DistanceToNode(n);
                if (d < (float)r)
                {
                    int moveAmt = (int)r - (int)d + 1;
                    if (Random.Range(0,2) == 0)
                    {
                        x = x + ((x > n.x)? moveAmt : 0 - moveAmt);
                    }
                    else
                    {
                        y = y + ((y > n.y)? moveAmt : 0 - moveAmt);
                    }
                }
                while (x < 5)
                    x += 1;
                while (x + 5 >= h)
                    x -= 1;
                while (y < 5)
                    y += 1;
                while (y + 5 >= h)
                    y -= 1;
            }
        }

        List<MapNode> questPOIs     = new List<MapNode>();
        List<MapNode> secondaryPOIs = new List<MapNode>();


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

            int poiCount = 4;
            questPOIs.Add(
                // I
                new MapNode(
                            Random.Range(lm.levelWidth/2,lm.levelWidth),
                            Random.Range(lm.levelHeight/2,lm.levelHeight),
                            20,
                            lm.levelWidth,
                            lm.levelHeight
            ));
            questPOIs.Add(
                // II
                new MapNode(
                            Random.Range(0,lm.levelWidth/2),
                            Random.Range(lm.levelHeight/2,lm.levelHeight),
                            20,
                            lm.levelWidth,
                            lm.levelHeight
            ));
            questPOIs.Add(
                // III
                new MapNode(
                            Random.Range(0,lm.levelWidth/2),
                            Random.Range(0,lm.levelHeight/2),
                            20,
                            lm.levelWidth,
                            lm.levelHeight
            ));
            questPOIs.Add(
                // IV
                new MapNode(
                            Random.Range(lm.levelWidth/2,lm.levelWidth),
                            Random.Range(0,lm.levelHeight/2),
                            20,
                            lm.levelWidth,
                            lm.levelHeight
            ));
            while (questPOIs.Count < poiCount)
            {
                questPOIs.Add(new MapNode(
                            Random.Range(0,lm.levelWidth),
                            Random.Range(0,lm.levelHeight),
                            20,
                            lm.levelWidth,
                            lm.levelHeight
                ));
            }

            float minDistance = 0;
            int tries = 0; // safety valve

            while (minDistance < 20 && tries < 100)
            {
                tries++;
                minDistance = 9999;
                for (int i = 0; i < questPOIs.Count; i++)
                {
                    for (int j = 0; j < questPOIs.Count; j++)
                    {
                        if (i != j)
                        {
                            MapNode a = questPOIs[i];
                            MapNode b = questPOIs[j];
                            if (i != j)
                            {
                                a.Repel(b);
                                float d = a.DistanceToNode(b);
                                if (d < minDistance)
                                    minDistance = d;
                            }
                        }
                    }
                }
            }

            List<MapNode> secpoi = new List<MapNode>();
            for (int i = 0; i < 3; i++)
            {
                secpoi.Add(new MapNode(
                            Random.Range(0,lm.levelWidth),
                            Random.Range(0,lm.levelHeight),
                            20,
                            lm.levelWidth,
                            lm.levelHeight
                ));
            }
            minDistance = 0;
            tries = 0;

            while (minDistance < 20 && tries < 100)
            {
                tries++;
                minDistance = 9999;
                for (int i = 0; i < secpoi.Count; i++)
                {
                    for (int j = 0; j < secpoi.Count; j++)
                    {
                        if (i != j)
                        {
                            MapNode a = secpoi[i];
                            MapNode b = secpoi[j];
                            if (i != j)
                            {
                                a.Repel(b);
                                float d = a.DistanceToNode(b);
                                if (d < minDistance)
                                    minDistance = d;
                            }
                        }
                    }
                }
                for (int i = 0; i < secpoi.Count; i++)
                {
                    for (int j = 0; j < questPOIs.Count; j++)
                    {
                        MapNode a = secpoi[i];
                        MapNode b = questPOIs[j];
                        if (i != j)
                        {
                            a.Repel(b);
                            float d = a.DistanceToNode(b);
                            if (d < minDistance)
                                minDistance = d;
                        }
                    }
                }
            }





            for (int i = 0; i < questPOIs.Count; i++)
            {
                map[questPOIs[i].x,questPOIs[i].y] = 2;
            }

            for (int i = 0; i < secpoi.Count; i++)
            {
                map[secpoi[i].x,secpoi[i].y] = 1;
            }

            foreach (MapNode a in questPOIs)
            {
                MapNode b = a.FindNearest(secpoi);
                // this is going to be a bit on the silly side as far as algorithms go, but time is of the essence
                int x = a.x;
                int y = a.y;
                if (Random.Range(0,2) == 0)
                {
                    tries = 0;
                    while (y != b.y && tries < 1000)
                    {
                        tries++;
                        y += ((y < b.y) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                    tries = 0;
                    while (x != b.x && tries < 1000)
                    {
                        tries++;
                        x += ((x < b.x) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                }
                else
                {
                    tries = 0;
                    while (x != b.x && tries < 1000)
                    {
                        tries++;
                        x += ((x < b.x) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                    tries = 0;
                    while (y != b.y && tries < 1000)
                    {
                        tries++;
                        y += ((y < b.y) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                }
            }

            for (int i = 0; i < secpoi.Count - 1; i++)
            {
                MapNode a = secpoi[i];
                MapNode b = secpoi[i + 1];
                int x = a.x;
                int y = a.y;
                if (Random.Range(0,2) == 0)
                {
                    tries = 0;
                    while (y != b.y && tries < 1000)
                    {
                        tries++;
                        y += ((y < b.y) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                    tries = 0;
                    while (x != b.x && tries < 1000)
                    {
                        tries++;
                        x += ((x < b.x) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                }
                else
                {
                    tries = 0;
                    while (x != b.x && tries < 1000)
                    {
                        tries++;
                        x += ((x < b.x) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                    tries = 0;
                    while (y != b.y && tries < 1000)
                    {
                        tries++;
                        y += ((y < b.y) ? 1 : -1);
                        if (map[x,y] == 0)
                            map[x,y] = 1;
                    }
                }
            }

            // erode roads
            bool eroded = true;
            while (eroded == true)
            {
                eroded = false;
                for (int x = 1; x < lm.levelWidth-1; x++)
                {
                    for (int y = 1; y < lm.levelHeight-1; y++)
                    {
                        if (map[x,y] == 1)
                        {
                            int roadcount = 0;
                            for (int x1 = x-1; x1 < x+2; x1++)
                            {
                                for (int y1 = y-1; y1 < y+2; y1++)
                                {
                                    if (map[x1,y1] == 1 || map[x1,y1] == 2)
                                        roadcount++;
                                }
                            }
                            if (roadcount > 5 || roadcount < 3)
                            {
                                map[x,y] = 0;
                                eroded = true;
                            }
                            else if (roadcount <= 4)
                            {
                                int orthoroadcount = 0;
                                if (map[x-1,y] == 1)
                                    orthoroadcount++;
                                if (map[x+1,y] == 1)
                                    orthoroadcount++;
                                if (map[x,y-1] == 1)
                                    orthoroadcount++;
                                if (map[x,y+1] == 1)
                                    orthoroadcount++;
                                if (map[x-1,y] == 2)
                                    orthoroadcount++;
                                if (map[x+1,y] == 2)
                                    orthoroadcount++;
                                if (map[x,y-1] == 2)
                                    orthoroadcount++;
                                if (map[x,y+1] == 2)
                                    orthoroadcount++;
                                if (orthoroadcount < 2)
                                {
                                    map[x,y] = 0;
                                    eroded = true;
                                }
                            }
                        }
                    }
                }
            }

            // now scatter some seeds
            for (int i = 0; i < 6; i++)
            {
                int x = Random.Range(0,lm.levelWidth);
                int y = Random.Range(0,lm.levelHeight);
                if (map[x,y] == 0)
                    map[x,y] = 3;
            }
            for (int i = 0; i < 8; i++)
            {
                int x = Random.Range(0,lm.levelWidth);
                int y = Random.Range(0,lm.levelHeight);
                if (map[x,y] == 0)
                    map[x,y] = 4;
            }
            for (int i = 0; i < 4; i++)
            {
                int x = Random.Range(0,lm.levelWidth);
                int y = Random.Range(0,lm.levelHeight);
                if (map[x,y] == 0)
                    map[x,y] = 5;
            }
            for (int i = 0; i < 3; i++)
            {
                int x = Random.Range(0,lm.levelWidth);
                int y = Random.Range(0,lm.levelHeight);
                if (map[x,y] == 0)
                    map[x,y] = 6;
            }

            // and let our weaksauce CA do its job
            for (int pass = 0; pass < 8; pass++)
            {
                int[,] newmap = (int[,]) map.Clone();
                for (int x = 0; x < lm.levelWidth; x++)
                {
                    for (int y = 0; y < lm.levelHeight; y++)
                    {
                        if (map[x,y] == 0)
                        {
                            int treecount     = 0;
                            int watercount    = 0;
                            int citycount     = 0;
                            int desertcount   = 0;
                            int mountaincount = 0;
                            for (int x1 = x - 1; x1 < x + 2; x1++)
                            {
                                for (int y1 = y - 1; y1 < y + 2; y1++)
                                {
                                    if (x1 > 0 && x1 < lm.levelWidth && y1 > 0 && y1 < lm.levelHeight)
                                    {
                                        switch (map[x1,y1])
                                        {
                                            case 2:
                                                citycount += 1;
                                                break;
                                            case 3:
                                                watercount += 1;
                                                break;
                                            case 4:
                                                treecount += 1;
                                                break;
                                            case 5:
                                                desertcount += 1;
                                                break;
                                            case 6:
                                                mountaincount += 1;
                                                break;
                                        }
                                    }
                                }
                            }
                            if (watercount > 2)
                                newmap[x,y] = 3;
                            else if (desertcount > 2)
                                newmap[x,y] = 5;
                            else if (treecount > 2)
                                newmap[x,y] = 4;
                            else if (mountaincount > 2)
                                newmap[x,y] = 6;
                            else if (desertcount > 0 && Random.Range(0,4) == 0)
                                newmap[x,y] = 5;
                            else if (mountaincount > 0 && Random.Range(0,4) == 0)
                                newmap[x,y] = 6;
                            else if (treecount > 0 && Random.Range(0,3) == 0)
                                newmap[x,y] = 4;
                            else if (watercount > 0 && Random.Range(0,4) == 0)
                                newmap[x,y] = 3;
                            else if (citycount > 0 && Random.Range(0,50) == 0)
                                newmap[x,y] = 4;
                            else if (citycount > 0 && Random.Range(0,50) == 0)
                                newmap[x,y] = 3;
                            else if (citycount > 0 && Random.Range(0,75) == 0)
                                newmap[x,y] = 5;
                            else if (citycount > 0 && Random.Range(0,100) == 0)
                                newmap[x,y] = 6;
                        }
                    }
                }
                map = (int[,]) newmap.Clone();
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
                            terrain.shortDescription = "grass";
                            terrain.longDescription  = "Grass";
                            break;
                        case 1:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "road";
                            terrain.longDescription  = "A crumbling but still-functional stretch of road";
                            break;
                        case 2:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "city";
                            terrain.longDescription  = "A walled city";
                            break;
                        case 3:
                            terrain.hindrance        = 1f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "water";
                            terrain.longDescription  = "A large impassible body of water";
                            break;
                        case 4:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "trees";
                            terrain.longDescription  = "Dense woodlands";
                            break;
                        case 5:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "desert";
                            terrain.longDescription  = "A barren desert";
                            break;
                        case 6:
                            terrain.hindrance        = 0f;
                            terrain.opacity          = 0f;
                            terrain.shortDescription = "mountains";
                            terrain.longDescription  = "Treacherous mountains";
                            break;
                    }
                }
            }

        }
        public override void PostProcess()
        {
            foreach (MapNode poi in questPOIs)
            {
                lm.entity.GetCell(poi.x,poi.y).AddActionCallback("_enter", delegate(Entity e)
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
