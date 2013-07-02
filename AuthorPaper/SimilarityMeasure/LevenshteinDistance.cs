using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimilarityMeasure
{
    public static class LevenshteinDistance
    {
        public static int CalculateDistance(string first, string second)
        {
            int firstLength = first.Length;
            int seconLength = second.Length;

            if (firstLength == 0 || second.Length == 0)
            {
                return Math.Max(firstLength, seconLength);
            }

            int[,] distances = new int[firstLength+1,seconLength+1];

            //Distances from a string of size [0,firstLength] to a empty string. 
            for (int i = 0; i <= firstLength; i++)
            {
                distances[i, 0] = i;
            }

            for (int i = 0; i <= seconLength; i++)
            {
                distances[0, i] = i;
            }

            for (int i = 1; i <= firstLength; i++)
            {
                for (int j = 1; j <= seconLength; j++)
                {
                    int distance = (first[i - 1] == second[j - 1]) ? 0 : 1;

                    //original Levenshtein distance:
                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + distance);
                }
            }
            return distances[firstLength, seconLength];
        }
    }
}
