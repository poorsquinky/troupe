using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

/*
namespace ThugSimpleGame {
    public class NPCPatrolBrain: NPCBrain {
        // scratch arrays
        private int[][] latestVisibility;
        private int[][] dijkstraMap;

        public NPCPatrolBrain(NPCScript body) : base(body)
        {
            parameters["sight_distance"] = 7;
            parameters["laziness"] = 2.0f;
        }

        public override void Run()
        {
            if (body.currentPath == null)
            {
                if (body.lm == null)
                {
                    GameObject l = GameObject.Find("LevelManager");
                    body.lm = l.GetComponent<LevelManagerScript>();
                }
                if (!data.ContainsKey("map"))
                {
                    data["map"] = MathUtils.Int2DArray(body.lm.levelWidth,
                       body.lm.levelHeight, 0);
                    data["tick"] = 0;
                }
            }

            // mark down what he can see right now on his map
            int x = (int)body.transform.position.x;
            int y = (int)body.transform.position.y;
            int sightDistance = (int)parameters["sight_distance"];
            int[][] map = (int [][])data["map"];
            int currentTick = (int)data["tick"];
            int xMin = x - sightDistance;
            if (xMin < 0) xMin = 0;
            int yMin = y - sightDistance;
            if (yMin < 0) yMin = 0;
            int xMax = x + sightDistance;
            if (xMax >= body.lm.levelWidth) xMax = body.lm.levelWidth - 1;
            int yMax = y + sightDistance;
            if (yMax >= body.lm.levelHeight) yMax = body.lm.levelHeight - 1;
            MapRectangle visibleRectangle = new MapRectangle(xMin, yMin,
               xMax - xMin + 1, yMax - yMin + 1);
            if (latestVisibility == null)
            {
                latestVisibility = MathUtils.Int2DArray(
                   body.lm.levelWidth, body.lm.levelHeight, 0);
                dijkstraMap = MathUtils.Int2DArray(
                   body.lm.levelWidth, body.lm.levelHeight, 0);
            }
            PathUtils.CalculateBresenhamProductsToRectangle(x, y,
               body.lm.map, visibleRectangle, (previous, tile) =>
               ((previous == 0 || !((MapSpaceType)tile).transparent) ?
               0 : 1), 1, false, true, latestVisibility);
            for (int i = xMin; i <= xMax; i++)
            {
                for (int j = yMin; j <= yMax; j++)
                {
                    if (latestVisibility[i][j] == 1)
                    {
                        map[i][j] = currentTick;
                    }
                }
            }

            // if I have reached the current path end, pick a new path
            if (body.currentPath == null ||
                body.currentPathStep >= body.currentPath.Steps.Length)
            {
                float laziness = 0.2f * ((float)parameters["laziness"]);
                PathUtils.CalculateDijkstraMap(x, y, 
                   null, (mx, my) => body.lm.map[mx][my].passable, null,
                   (mx, my, d) => (((int)d) % 2 == 1) ? 7 : 5,
                   dijkstraMap, body.lm.fullMapBounds, -1);
                int oldestTick = currentTick;
                int oldestX = -1, oldestY = -1;

                for (int i = 0; i < body.lm.levelWidth; i++)
                {
                    for (int j = 0; j < body.lm.levelHeight; j++)
                    {
                        if (body.lm.map[i][j].transparent &&
                            map[i][j] < oldestTick &&
                            dijkstraMap[i][j] != -1)
                        {
                            oldestTick = map[i][j] + (int)(laziness *
                               dijkstraMap[i][j]);
                            oldestX = i;
                            oldestY = j;
                        }
                    }
                }

                if (oldestX != -1)
                {
                    body.currentPath = PathUtils.BFSPath(x, y, oldestX, oldestY,
                       null, (mx, my) => body.lm.map[mx][my].passable, null,
                       (mx, my, d) => (((int)d) % 2 == 1) ? 7 : 5,
                       body.lm.fullMapBounds);
                    body.currentPathStep = 0;
                }
            }
            bool canMove = false;
            x = 0;
            y = 0;
            if (body.currentPath != null &&
                body.currentPathStep < body.currentPath.Steps.Length )
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
            }

            data["tick"] = currentTick + 1;
        }
    }
}
*/
