using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MSI2_CVRP
{
    public class EACOLS
    {
        private Random random = new (123);
        private int maxTime = 1500;
        private double alpha = 1; // pheromone priority
        private double beta = 2; // heuristic priority
        private double rho = 0.1; // pheromone decrease factor
        private double Q = 2.0; // pheromone increase factor
        private double pheromoneStartingValue = 1;

        private int[] bestGlobalPath;
        public int bestGlobalPathLength = int.MaxValue;
        private int[] longestPath;
        private int longestPathLength = int.MinValue;
        private int[] shortestPath;
        private int shortestPathLength = int.MaxValue;
        private int usedTrucks;

        private Ant[] ants;
        private int capacity;
        private int numberOfTrucks;
        private int numberOfAnts;

        public int[] demands; // indeksy miast od 1, 0 to indeks magazynu i jego demand jest 0
        public int[,] distances; 
        public double[,] pheromones;
        public int numberOfCities; // z magazynem

        public EACOLS (int citiesCount, int numberOfTrucks, int capacityOfTruck, int[,] dists, int[] needs)
        {
            numberOfCities = citiesCount;
            demands = needs;
            this.numberOfTrucks = numberOfTrucks;
            capacity = capacityOfTruck;
            numberOfAnts = numberOfCities - 1; // mrówka na miasto, poza magazynem

            distances = dists;
            pheromones = new double[numberOfCities, numberOfCities];
            for (int i = 0; i < numberOfCities; i++)
            {
                for (int j = i; j < numberOfCities; j++)
                {
                    if (i == j)
                    {
                        pheromones[i, j] = 0;
                    }
                    else
                    {
                        pheromones[i, j] = pheromoneStartingValue;
                        pheromones[j, i] = pheromones[i, j];
                    }
                }
            }

            ants = new Ant[numberOfAnts];
            for (int i = 0; i < numberOfAnts; i++)
            {
                int city = i + 1; // miasta od 1
                ants[i] = new Ant (capacity, city, demands[city], distances[0, city], numberOfCities, i);
            }
        }

        public void ResetAnts ()
        {
            ants = new Ant[numberOfAnts];
            for (int i = 0; i < numberOfAnts; i++) 
            {
                int city = i + 1;
                ants[i] = new Ant (capacity, city, demands[city], distances[0, city], numberOfCities, i);
            }
        }

        public void PrintInformation ()
        {
            PrintPathInformation ();

            Console.WriteLine ("-------------------");
            Console.WriteLine ("Number of trucks used: " + usedTrucks);

            if (numberOfTrucks != -1)
            {
                Console.WriteLine ("Expected number of trucks: " + numberOfTrucks);
                if (numberOfTrucks == usedTrucks)
                    Console.WriteLine ("Algorithm achived expected number of trucks!");
                else if (numberOfTrucks > usedTrucks)
                    Console.WriteLine ("Algorithm used less trucks than expected.");
                else if (numberOfTrucks < usedTrucks)
                    Console.WriteLine ("Algorithm used more trucks than expected.");
            }
        }

        private void PrintPathInformation ()
        {
            Console.WriteLine ("Best path");
            for (int i = 0; i < bestGlobalPath.Length; i++)
            {
                if (i == bestGlobalPath.Length - 1)
                    Console.WriteLine (i);
                else
                    Console.Write (i + "->");
            }
            Console.WriteLine ("Length: " + bestGlobalPathLength);
            Console.WriteLine ("-------------------");
        }

        public void StartAlgorithm ()
        {
            Console.WriteLine ("Starting algorithm...");
            int loop = 0;

            while (loop < maxTime)
            {
                if (loop % 100 == 0)
                {
                    Console.WriteLine ("Still going... Loop " + loop);
                }

                ResetAnts ();
                ConstructSolutions ();
                FindBestTrail ();
                UpdatePheromones ();
                GenerateNewRoutes ();
                loop++;
            }

            Console.WriteLine ("Best solution was found");
            PrintInformation ();
        }

        private void ConstructSolutions ()
        {
            for (int i = 0; i < ants.Length; i++)
            {
                while (!ants[i].AntDone (distances))
                    ants[i].ChooseNextCity (distances, demands, pheromones, alpha, beta, random);
            }
        }

        private void FindBestTrail ()
        {
            // porównujemy znalezione ścieżki, ich długości
            longestPathLength = int.MinValue;
            shortestPathLength = int.MaxValue;

            foreach (var ant in ants)
            {
                if (ant.Length < bestGlobalPathLength)
                {
                    bestGlobalPathLength = ant.Length;
                    bestGlobalPath = ant.Path.ToArray ();
                    usedTrucks = ant.UsedTrucks;

                    Console.WriteLine ("A better path was found.");
                    PrintPathInformation ();
                }
                if (ant.Length > longestPathLength)
                {
                    longestPathLength = ant.Length;
                    longestPath = ant.Path.ToArray ();
                }
                if (ant.Length < shortestPathLength)
                {
                    shortestPathLength = ant.Length;
                    shortestPath = ant.Path.ToArray ();
                }
            }
        }

        private void FindBestTrailWithNewGeneratedRoutes (List<Ant> allAnts)
        {
            longestPathLength = int.MinValue;
            shortestPathLength = int.MaxValue;

            foreach (var ant in allAnts)
            {
                if (ant.Length < bestGlobalPathLength)
                {
                    bestGlobalPathLength = ant.Length;
                    bestGlobalPath = ant.Path.ToArray ();
                    usedTrucks = ant.UsedTrucks;
                }
                if (ant.Length > longestPathLength)
                {
                    longestPathLength = ant.Length;
                    longestPath = ant.Path.ToArray ();
                }
                if (ant.Length < shortestPathLength)
                {
                    shortestPathLength = ant.Length;
                    shortestPath = ant.Path.ToArray ();
                }
            }
        }

        private void UpdatePheromones ()
        {
            for (int i = 0; i < pheromones.GetLength (0); i++)
            {
                for (int j = i; j < pheromones.GetLength (0); j++)
                {
                    double decrease = (1 - rho) * pheromones[i, j];
                    pheromones[i, j] = decrease;

                    foreach (var ant in ants)
                    {
                        double increase = 0.0;
                        double W = (Math.Log ((ant.Length - shortestPathLength) / longestPathLength) + 0.0001) * (-1);
                        if (ant.EdgeInTrail (i, j))
                        {
                            increase = W * Q / ant.Length;
                        }
                        pheromones[i, j] += increase;
                    }

                    pheromones[j, i] = pheromones[i, j];
                }
            }
        }

        private void UpdatePheromonesAfterNewRoutesGenerated (List<Ant> allAnts)
        {
            for (int i = 0; i < pheromones.GetLength (0); i++)
            {
                for (int j = i; j < pheromones.GetLength (0); j++)
                {
                    double decrease = (1 - rho) * pheromones[i, j];
                    pheromones[i, j] = decrease;

                    foreach (var ant in allAnts)
                    {
                        double increase = 0.0;
                        double W = (Math.Log ((ant.Length - shortestPathLength) / longestPathLength) + 0.0001) * (-1);
                        if (ant.EdgeInTrail (i, j))
                        {
                            increase = W * Q / ant.Length;
                        }
                        pheromones[i, j] += increase;
                    }

                    pheromones[j, i] = pheromones[i, j];
                }
            }
        }

        private void GenerateNewRoutes ()
        {
            List<Ant> sortedAnts = ants.ToList ();
            sortedAnts.Sort ((a1, a2) => { return a1.Length > a2.Length ? 1 : 0; });

            List<Ant> twoNewRoutesGroup = sortedAnts.Take ((int)Math.Round (0.2 * sortedAnts.Count)).ToList ();
            List<Ant> oneNewRouteGroup = sortedAnts.Skip ((int)Math.Round (0.2 * sortedAnts.Count)).ToList ();

            List<Ant> allRoutes = new List<Ant> ();
            int ix = 0;

            foreach (var route in twoNewRoutesGroup)
            {
                allRoutes.Add (route);
                Ant newAnt = GenerateNewRoute (route);
                if (newAnt.AntDone(distances))
                    allRoutes.Add (newAnt);
                newAnt = GenerateNewRoute (route);
                if (newAnt.AntDone (distances))
                    allRoutes.Add (GenerateNewRoute (route));
                ix++;
            }
            foreach (var route in oneNewRouteGroup)
            {
                allRoutes.Add (route);
                Ant newAnt = GenerateNewRoute (route);
                if (newAnt.AntDone(distances))
                    allRoutes.Add (GenerateNewRoute (route));
            }

            FindBestTrailWithNewGeneratedRoutes (allRoutes);
            UpdatePheromonesAfterNewRoutesGenerated (allRoutes);
        }

        private Ant GenerateNewRoute (Ant oldAnt)
        {
            Ant newAnt = new Ant (oldAnt.Capacity, oldAnt.Visited.Length);
            int indexToSwap1 = random.Next (oldAnt.Path.Count - 1);
            int indexToSwap2 = random.Next (oldAnt.Path.Count - 1);

            if (indexToSwap1 == 0)
            {
                indexToSwap1++;
            }
            else if (indexToSwap2 == 0)
            {
                indexToSwap2++;
            }

            if (indexToSwap1 == oldAnt.Path.Count - 1) // jest ostatnim magazynem
            {
                indexToSwap1--;
            }
            else if (indexToSwap2 == oldAnt.Path.Count - 1)
            {
                indexToSwap2--;
            }

            if (indexToSwap1 > indexToSwap2)
            {
                int tmp = indexToSwap2;
                indexToSwap2 = indexToSwap1;
                indexToSwap1 = tmp;
            }

            int cityToSwap1 = oldAnt.Path[indexToSwap1];
            int cityToSwap2 = oldAnt.Path[indexToSwap2];

            int counter = 1;
            while (!newAnt.AntDone (distances))
            {
                if (counter < indexToSwap1)
                {
                    int cityToAdd = oldAnt.Path[counter];
                    newAnt.AddSpecificCityTOPath (cityToAdd, demands[cityToAdd], distances[newAnt.CurrentCity, cityToAdd], distances[0, cityToAdd]);
                }
                else if (counter == indexToSwap1 && !newAnt.Visited[cityToSwap2])
                {
                    newAnt.AddSpecificCityTOPath (cityToSwap2, demands[cityToSwap2], distances[newAnt.CurrentCity, cityToSwap2], distances[0, cityToSwap2]);
                }
                else if (counter == indexToSwap2 && !newAnt.Visited[cityToSwap1])
                {
                    newAnt.AddSpecificCityTOPath (cityToSwap1, demands[cityToSwap1], distances[newAnt.CurrentCity, cityToSwap1], distances[0, cityToSwap1]);
                }
                else // pomiędzy 1 a 2, lub po 2
                {
                    newAnt.ChooseNextCity (distances, demands, pheromones, alpha, beta, random);//, cityToSwap1, cityToSwap2);
                }

                counter++;

                if (newAnt.Path.Last() == 0 && newAnt.Path[newAnt.Path.Count - 2] == 0)
                {
                    // wchodzimy w pętlę wchodzenia do 0 - przerywamy, żeby sobie życie ułatwić
                    break;
                }
            }

            return newAnt;
        }
    }
}
