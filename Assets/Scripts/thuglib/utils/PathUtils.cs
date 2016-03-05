using System;
using System.Collections;
using System.Collections.Generic;

namespace ThugLib
{
    public enum eStep : int
    {
        North,
        Northeast,
        East,
        Southeast,
        South,
        Southwest,
        West,
        Northwest
    };

    public class Path
    {
        public eStep[] Steps;

        public int NSteps; // may be < steps.Length

        public Path(eStep[] steps)
        {
            this.Steps = steps;
            this.NSteps = Steps.Length;
        }
    }

    public partial class PathUtils
    {
        public static int[] StepDX = new int[]
           {  0,  1,  1,  1,  0, -1, -1, -1 };

        public static int[] StepDY = new int[]
           {  1,  1,  0, -1, -1, -1,  0,  1 };

        public static eStep[] ReverseStep = new eStep[]
           { eStep.South, eStep.Southwest, eStep.West, eStep.Northwest,
             eStep.North, eStep.Northeast, eStep.East, eStep.Southeast };

        public static void PrintPath(Path fillPath, int x1, int y1)
        {
            int x = x1, y = y1;
            Console.WriteLine("   " + x + ", " + y);
            for (int i = 0; i < fillPath.NSteps; i++) 
            {
                x += StepDX[(int)fillPath.Steps[i]];
                y += StepDY[(int)fillPath.Steps[i]];
                Console.WriteLine("   " + x + ", " + y);
            }
        }

        public static int[][] UnrollPath(Path fillPath, int x1, int y1)
        {
            int[][] unrolled = new int[fillPath.NSteps + 1][];
            int x = x1, y = y1;
            for (int i = 0; i < fillPath.NSteps; i++)
            {
                unrolled[i] = new int[] {x, y};
                x += StepDX[(int)fillPath.Steps[i]];
                y += StepDY[(int)fillPath.Steps[i]];
            }
            unrolled[fillPath.NSteps] = new int[] {x, y};
            return unrolled;
        }
    }
}
