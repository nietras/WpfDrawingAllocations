using System;

namespace WpfDrawingAllocations.ItemsExample
{
    public static class ItemResultsHelper
    {
        public static void RandomFillResults(this Random random, ItemResult[] results)
        {
            for (int r = 0; r < results.Length; r++)
            {
                results[r] = (ItemResult)random.Next((int)ItemResult.F);
            }
        }

        public static void RandomFillStates(this Random random, bool[] states)
        {
            for (int r = 0; r < states.Length; r++)
            {
                states[r] = random.Next(4) == 0;
            }
        }

        public static void ClearStates(bool[] states)
        {
            Array.Clear(states, 0, states.Length);
        }
    }
}
