using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public class Lake
    {
        public bool dead = false;
        public bool visited; // no-loops hack
        public int level;
        public int drainToX, drainToY;
        public int lowestBorderX, lowestBorderY;
        public int index;
        public int successorIndex;
        public List<int[]> squares = new List<int[]>();
        public List<int[]> borderSquares = new List<int[]>();
    }

    public class ErosionHeightmapGenerator : MapGenerator
    {
        int nErosionPasses;

        bool initialLakePass;

        bool finalLakePass;

        public ErosionHeightmapGenerator(int[] pixelTypes,
           MapCoordinate coordinate, int nErosionPasses, bool
           initialLakePass, bool finalLakePass) : base(pixelTypes, coordinate)
        {
            this.nErosionPasses = nErosionPasses;
            this.initialLakePass = initialLakePass;
            this.finalLakePass = finalLakePass;
        }

        // pixel types: 
        // custom params:
        public override List<MapRoom> Run(int[][] map, MapRectangle fillRegion,
           List<MapRoom> roomsToInclude)
        {
            List<Lake> lakes = new List<Lake>();
            bool debug = false;
            int maxHeight = 0;
            for (int i = fillRegion.x; i <= fillRegion.x2; i++)
            {
                for (int j = fillRegion.y; j <= fillRegion.y2; j++)
                {
                    if (map[i][j] > maxHeight) 
                    {
                        maxHeight = map[i][j];
                    }
                }
            }
            int erodeWeight = maxHeight / (fillRegion.w * nErosionPasses);
            if (erodeWeight < 1) erodeWeight = 1;
            int[][] particles = MathUtils.Int2DArray(fillRegion.w, fillRegion.h,
               0);
            int[][] riverLength = MathUtils.Int2DArray(fillRegion.w,
               fillRegion.h, 0);
            int[][] lakeIndex = MathUtils.Int2DArray(fillRegion.w, fillRegion.h,
               -1);
            int maxParticlesPerChannel = 100;
            for (int pass = 0; pass < nErosionPasses; pass++) {
                for (int i = fillRegion.x; i <= fillRegion.x2; i++) {
                   for (int j = fillRegion.y; j <= fillRegion.y2; j++) {
                       particles[i - fillRegion.x][j - fillRegion.y] = 0;
                       riverLength[i - fillRegion.x][j - fillRegion.y] = 0;
                   }
                }
                bool isLakePass = (pass == 0 && initialLakePass) ||
                   (pass == nErosionPasses - 1 && finalLakePass);
                if (isLakePass)
                {
                    ClearLakes(lakes, lakeIndex);
                }
                if (debug)
                {
                    Console.WriteLine("INFO: starting erosion pass " + pass +
                       " (lakes = " + isLakePass + ")");
                }
                for (int i = fillRegion.x; i <= fillRegion.x2; i++) {
                   for (int j = fillRegion.y; j <= fillRegion.y2; j++) {
                        RunParticle(i, j, map, particles, lakes, lakeIndex,
                            isLakePass, riverLength, false, fillRegion);
                    }
                    if (debug && (i % 100 == 0))
                    {
                        Console.WriteLine("INFO: done with row " + i + " pass "
                           + pass + " # lakes = " + lakes.Count);
                    }
                }
                PruneLakes(lakes, lakeIndex, fillRegion.x, fillRegion.y);
                if (!isLakePass)
                {
                    for (int i = fillRegion.x; i <= fillRegion.x2; i++) {
                        int di = i - fillRegion.x;
                        for (int j = fillRegion.y; j <= fillRegion.y2; j++) {
                            int dj = j - fillRegion.y;
                            if (particles[di][dj] > 0 &&
                               lakeIndex[di][dj] == -1)
                            {
                                map[i][j] -= erodeWeight * (particles[di][dj] >
                                   maxParticlesPerChannel ?
                                   maxParticlesPerChannel : particles[di][dj]);
                            }
                        }
                    }
                }
            }

            // zero out the lakes
            for (int i = 0; i < lakes.Count; i++)
            {
                for (int j = 0; j < lakes[i].squares.Count; j++)
                {
                    map[lakes[i].squares[j][0]][lakes[i].squares[j][1]] = 0;
                }
            }

            // zero out the long rivers

            return new List<MapRoom>(new MapRoom[0]);
        }

        private void ClearLakes(List<Lake> lakes, int[][] lakeIndex)
        {
            lakes.Clear();
            for (int i = 0; i < lakeIndex.Length; i++)
            {
                for (int j = 0; j < lakeIndex[i].Length; j++)
                {
                    lakeIndex[i][j] = -1;
                }
            }
        }

        private List<Lake> newLakes = new List<Lake>();

        private void PruneLakes(List<Lake> lakes, int[][] lakeIndex,
           int x0, int y0)
        {
            newLakes.Clear();
            for (int i = 0; i < lakes.Count; i++)
            {
                if (!lakes[i].dead) newLakes.Add(lakes[i]);
            }
            for (int i = 0; i < newLakes.Count; i++)
            {
                for (int j = 0; j < newLakes[i].squares.Count; j++)
                {
                    lakeIndex[newLakes[i].squares[j][0] - x0][
                       newLakes[i].squares[j][1] - y0] = i;
                }
                newLakes[i].index = i;
            }
            lakes.Clear();
            lakes.AddRange(newLakes);
        }

        private int RiverStep(int x1, int y1, int x2, int y2,
           int[][] particles, int[][] riverLength, int currentRiverLength,
           MapRectangle bounds)
        {
            int xoff = x1 - bounds.x;
            int yoff = y1 - bounds.y;
            particles[xoff][yoff]++;
            if (riverLength[xoff][yoff] <= currentRiverLength)
            {
                riverLength[xoff][yoff] = currentRiverLength;
            }
            else
            {
                currentRiverLength = riverLength[xoff][yoff];
            }
            currentRiverLength += (x1 != x2 && y1 != y2) ? 141 : 100;
            return currentRiverLength;
        }

        private int FindLargestBorderValue(Lake lake,
           int[][] lakeIndex, int[][] values, MapRectangle bounds)
        {
            int largestBorderValue = 0;
            for (int i = lake.borderSquares.Count - 1; i >= 0; i--)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = lake.borderSquares[i][0] + dx; // CHANGE
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx != 0 || dy != 0)
                        {
                            int y = lake.borderSquares[i][1] + dy; // CHANGE
                            if (bounds.Contains(x, y))
                            {
                                int xoff = x - bounds.x;
                                int yoff = y - bounds.y;
                                if (lakeIndex[xoff][yoff] == -1)
                                {
                                    if (values[xoff][yoff] > largestBorderValue)
                                    {
                                        largestBorderValue = values[xoff][yoff];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return largestBorderValue;
        }

        private void RunParticle(int xStart, int yStart, int[][]
           height, int[][] particles, List<Lake> lakes, int[][] lakeIndex,
           bool generateLakes, int[][] riverLength, bool debug,
           MapRectangle bounds)
        {
            int nSteps = 0;
            int x = xStart, y = yStart;
            if (debug)
            {
                Console.WriteLine("Run from " + xStart + ", " + yStart);
            }
            int currentRiverLength = 0;
            while (bounds.Contains(x, y) && height[x][y] > 0)
            {
                int xoff = x - bounds.x;
                int yoff = y - bounds.y;
                while (lakeIndex[xoff][yoff] != -1)
                {
                    Lake lake = lakes[lakeIndex[xoff][yoff]];
                    if (lake.lowestBorderX == -1)
                    {
                        return;
                    }
                    if (debug)
                    {
                        Console.WriteLine("Jump from lake " +
                           lakeIndex[xoff][yoff] + " at " + x + ", " + y +
                           " to " + lake.drainToX + ", " + lake.drainToY);
                    }
                    currentRiverLength = FindLargestBorderValue(lake,
                       lakeIndex, riverLength, bounds);
                    currentRiverLength = RiverStep(lake.lowestBorderX,
                       lake.lowestBorderY, lake.drainToX, lake.drainToY,
                       particles, riverLength, currentRiverLength, bounds);
                    x = lake.drainToX;
                    y = lake.drainToY;
                }

                int bestGradient = 0;
                int bestDX = 0, bestDY = 0;
                int z = height[x][y];
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x2 = x + dx;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx != 0 || dy != 0)
                        {
                            int y2 = y + dy;
                            if (bounds.Contains(x2, y2))
                            {
                                if (z > height[x2][y2])
                                {
                                    int gradient = z - height[x2][y2];
                                    if (dx != 0 && dy != 0) gradient *= 71;
                                    else gradient *= 100;
                                    if (gradient > bestGradient)
                                    {
                                        bestGradient = gradient;
                                        bestDX = dx;
                                        bestDY = dy;
                                    }
                                }
                            }
                        }
                    }
                }
                if (bestGradient == 0)
                {
                    if (!generateLakes)
                    {
                        if (debug)
                        {
                            Console.WriteLine("Hit a pit; no lakes");
                        }
                        return;
                    }
                    else if (x > bounds.x && y > bounds.y &&
                       x < bounds.x2 && y < bounds.y2 && height[x][y] > 0)
                    {
                        Lake lake = GrowLakeFromSeed(x, y, lakes, lakeIndex,
                           height, bounds);
                        currentRiverLength = FindLargestBorderValue(lake,
                           lakeIndex, riverLength, bounds);
                        currentRiverLength = RiverStep(lake.lowestBorderX,
                           lake.lowestBorderY, lake.drainToX, lake.drainToY,
                           particles, riverLength, currentRiverLength, bounds);
                        x = lake.drainToX;
                        y = lake.drainToY;
                        if (debug)
                        {
                            Console.WriteLine("After lake gen, now at " + x +
                               ", " + y);
                        }
                    }
                    else
                    {
                        if (debug)
                        {
                            Console.WriteLine("Ran off the map or into the sea");
                        }
                        return;
                    }
                }
                else
                {
                    currentRiverLength = RiverStep(x, y, x + bestDX, y + bestDY,
                       particles, riverLength, currentRiverLength, bounds);
                    x += bestDX;
                    y += bestDY;
                    if (debug)
                    {
                        Console.WriteLine("Run to " + x + ", " + y);
                    }
                }
                nSteps++;
            }
        }

        private void FindLowestBorderSquare(Lake lake, out int
           borderHeight, out int borderX, out int borderY, int[][] lakeIndex,
           int[][] height, bool debug, MapRectangle bounds)
        {
            // find the lowest square bordering the existing lake; set
            // the lake level to its height
            borderX = -1;
            borderY = -1;
            borderHeight = 10000000;
            if (debug || lake.dead)
            {
                Console.WriteLine("FLBS for lake " + lake.index + " dead = " +
                   lake.dead);
            }
            for (int i = lake.borderSquares.Count - 1; i >= 0; i--)
            {
                if (debug)
                {
                    Console.WriteLine("check square " + lake.squares[i][0] +
                       ", " + lake.squares[i][1]);
                }
                bool shouldBeInternal = true;
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = lake.borderSquares[i][0] + dx;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx != 0 || dy != 0)
                        {
                            int y = lake.borderSquares[i][1] + dy;
                            if (bounds.Contains(x, y))
                            {
                                int xoff = x - bounds.x;
                                int yoff = y - bounds.y;
                                if (debug)
                                {
                                    Console.WriteLine("   consider " + x +
                                       ", " + y + " lake # = " +
                                       lakeIndex[xoff][yoff]);
                                }
                                if (lakeIndex[xoff][yoff] != lake.index)
                                {
                                    shouldBeInternal = false;
                                }
                                if (lakeIndex[xoff][yoff] == -1)
                                {
                                    int testHeight = height[x][y];
                                    if (testHeight < borderHeight)
                                    {
                                        borderHeight = testHeight;
                                        borderX = x;
                                        borderY = y;
                                    }
                                }
                            }
                        }
                    }
                }
                if (shouldBeInternal)
                {
                    lake.borderSquares.RemoveAt(i);
                }
            }
        }

        private void MergeLakeIntoLake(Lake mergeLake, Lake intoLake,
           int[][] lakeIndex, MapRectangle bounds)
        {
            for (int i = 0; i < mergeLake.squares.Count; i++)
            {
                intoLake.squares.Add(mergeLake.squares[i]);
                int xoff = mergeLake.squares[i][0] - bounds.x;
                int yoff = mergeLake.squares[i][1] - bounds.y;
                lakeIndex[xoff][yoff] = intoLake.index;
            }
            for (int i = 0; i < mergeLake.borderSquares.Count; i++)
            {
                intoLake.borderSquares.Add(mergeLake.borderSquares[i]);
            }
            mergeLake.squares.Clear();
            mergeLake.borderSquares.Clear();
            mergeLake.dead = true;
            mergeLake.successorIndex = intoLake.index;
        }

        private bool FindDrainage(Lake lake, List<Lake> lakes,
           int borderX, int borderY, int borderHeight, bool debug,
           int[][] lakeIndex, int[][] height, MapRectangle bounds)
        {
            bool hasDrainage = false;
            bool done = false;
            while (!done)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int x = borderX + dx;
                        int y = borderY + dy;
                        if (bounds.Contains(x, y) && (dx != 0 || dy != 0))
                        {
                            int xoff = x - bounds.x;
                            int yoff = y - bounds.y;
                            if (lakeIndex[xoff][yoff] == -1 &&
                               height[x][y] < borderHeight)
                            {
                                hasDrainage = true;
                                lake.drainToX = x;
                                lake.drainToY = y;
                                lake.lowestBorderX = borderX;
                                lake.lowestBorderY = borderY;
                                if (debug)
                                {
                                    Console.WriteLine(lake.index +
                                       ": found drainage to " +
                                       x + ", " + y);
                                }
                            }
                            else if (lakeIndex[xoff][yoff] != -1 &&
                               lakeIndex[xoff][yoff] != lake.index &&
                               lakes[lakeIndex[xoff][yoff]].level <
                               borderHeight)
                            {
                                hasDrainage = true;
                                lake.drainToX = x;
                                lake.drainToY = y;
                                lake.lowestBorderX = borderX;
                                lake.lowestBorderY = borderY;
                                if (debug)
                                {
                                    Console.WriteLine(lake.index +
                                       ": found drainage to " +
                                       x + ", " + y + " lake index " +
                                       lakeIndex[xoff][yoff] + " level " +
                                       lakes[lakeIndex[xoff][yoff]].level);
                                }
                            }
                        }
                    }
                }
                done = true;
                // merge adjacent lakes as appropriate
                if (hasDrainage)
                {
                    int index = lakeIndex[lake.drainToX - bounds.x][
                       lake.drainToY - bounds.y];
                    if (index != -1)
                    {
                        Lake otherLake = lakes[index];
                        int otherBorderX, otherBorderY;
                        int otherBorderHeight;
                        FindLowestBorderSquare(otherLake, out otherBorderHeight,
                           out otherBorderX, out otherBorderY, lakeIndex,
                           height, false, bounds);
                        if (otherBorderX == borderX && otherBorderY == borderY
                           && borderX != -1)
                        {
                            if (debug)
                            {
                                Console.WriteLine("Merging lake " + otherLake.
                                   index + " into " + lake.index);
                            }
                            MergeLakeIntoLake(otherLake, lake, lakeIndex,
                               bounds);
                            FindLowestBorderSquare(lake, out borderHeight,
                               out borderX, out borderY, lakeIndex, height,
                               false, bounds);
                            if (debug)
                            {
                                Console.WriteLine(lake.index +
                                   ": restarting search for drainage from " +
                                   "now-merged " + lake.index);
                                Console.WriteLine(lake.index +
                                   ": new lowest border is " +
                                   borderX + ", " + borderY + " height " +
                                   borderHeight);
                            }
                            done = false; // restart search for drainage
                            hasDrainage = false;
                        }
                    }
                }
            }
            return hasDrainage;
        }

        public Lake GrowLakeFromSeed(int xSeed, int ySeed, List<Lake> lakes,
           int[][] lakeIndex, int[][] height, MapRectangle bounds)
        {
            Lake lake = new Lake();
            lake.squares.Add(new int[] {xSeed, ySeed});
            lake.borderSquares.Add(new int[] {xSeed, ySeed});
            lake.index = lakes.Count;
            lake.level = height[xSeed][ySeed];
            lakes.Add(lake);
            lakeIndex[xSeed - bounds.x][ySeed - bounds.y] = lake.index;
            bool debug = false;
            if (debug)
            {
                Console.WriteLine("Grow lake from " + xSeed + ", " + ySeed +
                   " index " + lake.index + " height " + height[xSeed][ySeed]);
            }
            GrowLake(lake, lakes, lakeIndex, height, debug, bounds);
            return lake;
        }

        public bool IsSurroundedBySameLake(Lake lake, int[][] lakeIndex,
           out List<int> borderingLakes, MapRectangle bounds)
        {
            borderingLakes = new List<int>();
            for (int i = 0; i < lake.squares.Count; i++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx != 0 || dy != 0)
                        {
                            int x = lake.squares[i][0] + dx;
                            int y = lake.squares[i][1] + dy;
                            if (bounds.Contains(x, y))
                            {
                                int xoff = x - bounds.x;
                                int yoff = y - bounds.y;
                                if (lakeIndex[xoff][yoff] == -1)
                                {
                                    borderingLakes = null;
                                    return false;
                                }
                                else if (lakeIndex[xoff][yoff] != lake.index)
                                {
                                    if (!borderingLakes.Contains(
                                       lakeIndex[xoff][yoff]))
                                    {
                                        borderingLakes.Add(
                                           lakeIndex[xoff][yoff]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (borderingLakes.Count == 1);
        }

        public void GrowLake(Lake lake, List<Lake> lakes, int[][] lakeIndex,
           int[][] height, bool debug, MapRectangle bounds)
        {
            if (debug)
            {
                Console.WriteLine(lake.index + ": grow started, " +
                   lake.squares.Count + " squares, level = " + lake.level +
                   " dead = " + lake.dead);
            }
            bool done = false;
            while (!done)
            {
                int borderX, borderY;
                int borderHeight;
                FindLowestBorderSquare(lake, out borderHeight, out borderX,
                   out borderY, lakeIndex, height, false, bounds);
                if (borderX != -1)
                {
                    // check if this square is the lowest border of any other
                    // lakes

                    int lakeToMerge = -1;
                    for (int i = 0; i < lakes.Count; i++)
                    {
                        if (lakes[i] != lake && !lakes[i].dead &&
                            lakes[i].lowestBorderX == borderX &&
                            lakes[i].lowestBorderY == borderY)
                        {
                            lakeToMerge = i;
                        }
                    }
                    if (lakeToMerge != -1)
                    {
                        if (debug)
                        {
                            Console.WriteLine(lake.index + ": merge me into " +
                               lakeToMerge);
                        }
                        MergeLakeIntoLake(lakes[lakeToMerge], lake, lakeIndex,
                           bounds);
                    }
                }
                if (borderX != -1)
                {
                    lake.level = borderHeight;
                    if (debug)
                    {
                        Console.WriteLine(lake.index + ": lowest border square"
                           + " is " + borderX + ", " + borderY + " new level = "
                           + lake.level);
                    }

                    // check if this square can be added to the lake: that is,
                    // if it has no bordering squares that are both outside the
                    // lake and lower than it is.

                    bool hasDrainage = FindDrainage(lake, lakes, borderX,
                       borderY, borderHeight, debug, lakeIndex, height, bounds);

                    if (!hasDrainage)
                    {
                        lake.squares.Add(new int[] {borderX, borderY});
                        lake.borderSquares.Add(new int[] {borderX, borderY});
                        lakeIndex[borderX - bounds.x][borderY - bounds.y] =
                           lake.index;
                        if (debug)
                        {
                            Console.WriteLine(lake.index +
                               ": add square to lake");
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }
                else
                {
                    List<int> surroundingLakes;
                    if (IsSurroundedBySameLake(lake, lakeIndex, out
                       surroundingLakes, bounds))
                    {
                        if (lake.level < lakes[surroundingLakes[0]].level)
                        {
                            MergeLakeIntoLake(lake, lakes[surroundingLakes[0]],
                               lakeIndex, bounds);
                            lake = lakes[surroundingLakes[0]];
                        }
                        else
                        {
                            Console.WriteLine("ERROR 1!");
                            return;
                        }
                    }
                    else if (surroundingLakes != null)
                    {
                        int lowestLevel = 100000000;
                        int bestLake = -1;
                        foreach (int surroundingLake in surroundingLakes)
                        {
                            if (lakes[surroundingLake].level < lowestLevel &&
                               lakes[surroundingLake].level > lake.level)
                            {
                                lowestLevel = lakes[surroundingLake].level;
                                bestLake = surroundingLake;
                            }
                        }
                        if (bestLake != -1)
                        {
                            MergeLakeIntoLake(lake, lakes[bestLake], lakeIndex,
                               bounds);
                            lake = lakes[bestLake];
                        }
                        else
                        {
                            Console.WriteLine("ERROR 2!");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR 3! Lake has no border but " +
                           "no surrounding lakes!");
                        return;
                    }
                }
            }

            // recompute drainages for any lakes that drain into this lake
            int drainsToMeIndex = lake.index;
            while (lakes[drainsToMeIndex].dead)
            {
                drainsToMeIndex = lakes[drainsToMeIndex].successorIndex;
            }
            if (lake.dead)
            {
                return;
            }
            for (int i = 0; i < lakes.Count; i++)
            {
                if (i != drainsToMeIndex && !lakes[i].dead)
                {
                    Lake testLake = lakes[i];
                    if (lakeIndex[testLake.drainToX - bounds.x][
                       testLake.drainToY - bounds.y] == drainsToMeIndex)
                    {
                        if (debug)
                        {
                            Console.WriteLine(lake.index +
                               ": lake " + i + " drains into me, it needs" +
                               " to find a new drainage; growing it");
                        }
                        GrowLake(testLake, lakes, lakeIndex, height, debug,
                           bounds);
                        if (debug)
                        {
                            Console.WriteLine(lake.index + ": back from " +
                               "recursive grow");
                        }
                        while (lakes[drainsToMeIndex].dead)
                        {
                            drainsToMeIndex = lakes[drainsToMeIndex].
                               successorIndex;
                        }
                    }
                }
            }

            // if I am now draining into a lake that's higher than me, 
            // restart
            lake = lakes[drainsToMeIndex];
            int drainToLakeIndex = lakeIndex[lake.drainToX - bounds.x][
               lake.drainToY - bounds.y];
            if (!lake.dead && drainToLakeIndex != -1 &&
               lakes[drainToLakeIndex].level > lake.level)
            {
                if (debug)
                {
                    Console.WriteLine(lake.index + ": I am now draining " +
                       "into a lake with a higher level; starting over");
                }
                GrowLake(lake, lakes, lakeIndex, height, debug, bounds);
            }

            if (debug)
            {
                Console.WriteLine(lake.index + " done growing: " +
                   lake.squares.Count + " squares, level = " + lake.level +
                   " drains to " + lake.drainToX + ", " + lake.drainToY +
                   " (lake index " + lakeIndex[lake.drainToX - bounds.x][
                   lake.drainToY - bounds.y] + ") raw level @ drain = " +
                   height[lake.drainToX][lake.drainToY] +
                   (lakeIndex[lake.drainToX - bounds.x][
                   lake.drainToY - bounds.y] != -1 ? (", lake level @ drain = "
                   + lakes[lakeIndex[lake.drainToX - bounds.x][
                   lake.drainToY - bounds.y]].level) : ""));
            }
        }
    }
}
