using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSI2_CVRP
{
    public class Greedy
    {
        private List<int> bestPath;
        public int bestPathLength;
        private int capacity;
        private int numberOfTrucks;
        public int[] demands; // indeksy miast od 1, 0 to indeks magazynu i jego demand jest 0
        public int[,] distances;
        public int numberOfCities; // z magazynem
        private bool[] visited;
        private int currentCity;
        private int curretnLoad;
        private int usedTrucks = 0;

        public Greedy (int citiesCount, int numberOfTrucks, int capacityOfTruck, int[,] dists, int[] needs)
        {
            numberOfCities = citiesCount;
            this.numberOfTrucks = numberOfTrucks;
            capacity = capacityOfTruck;
            distances = dists;
            demands = needs;
            visited = new bool[citiesCount];
            bestPath = new List<int> ();
            bestPathLength = 0;
        }

        public void StartAlgorithm()
        {
            // zaczynamy w warehouse
            visited[0] = true;
            bestPath.Add (0);
            currentCity = 0;
            curretnLoad = capacity;

            while (!visited.All (x => x))
            {
                // szukamy najbliżeszego punktu, który możemy odwiedzić
                List<(int distance, int city)> possibleNextCities = new List<(int, int)> ();
                int nextCity = 0;
                int distance = int.MaxValue;
                for (int i = 0; i < numberOfCities; i++)
                {
                    if (i != currentCity && !visited[i] && curretnLoad >= demands[i] && distances[currentCity, i] < distance)
                    {
                        nextCity = i;
                        distance = distances[currentCity, i];
                    }
                }

                if (nextCity != 0)
                {
                    visited[nextCity] = true;
                    curretnLoad -= demands[nextCity];
                    bestPath.Add (nextCity);
                    bestPathLength += distance;
                    currentCity = nextCity;
                }
                else
                {
                    // wracamy do hangaru
                    curretnLoad = capacity;
                    bestPath.Add (0);
                    bestPathLength += distances[currentCity, 0];
                    currentCity = 0;
                    usedTrucks++;
                }
            }

            // wracamy do hangaru na koniec
            curretnLoad = capacity;
            bestPath.Add (0);
            bestPathLength += distances[currentCity, 0];
            currentCity = 0;
            usedTrucks++;

            PrintResult ();
        }

        private void PrintResult ()
        {
            Console.WriteLine ("\n-------------------");
            Console.WriteLine ("Best path");
            foreach (var i in bestPath)
            {
                Console.Write (i + "->");
            }
            Console.WriteLine ("\nLength: " + bestPathLength);

            Console.WriteLine ("-------------------");
            Console.WriteLine ("Number of trucks used: " + usedTrucks);
            Console.WriteLine ("Expected number of trucks: " + numberOfTrucks);

            if (numberOfTrucks != -1)
            {
                if (numberOfTrucks == usedTrucks)
                    Console.WriteLine ("Algorithm achived expected number of trucks!");
                else if (numberOfTrucks > usedTrucks)
                    Console.WriteLine ("Algorithm used less trucks than expected.");
                else if (numberOfTrucks < usedTrucks)
                    Console.WriteLine ("Algorithm used more trucks than expected.");
            }
        }
    }
}
