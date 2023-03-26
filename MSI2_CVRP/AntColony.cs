using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MSI2_CVRP
{
    public class AntColony
    {
        private Random random = new (123); 
        private int maxTime = 1500;
        private double alpha = 1; // pheromone priority
        private double beta = 2; // heuristic priority
        private double rho = 0.1; // pheromone decrease factor
        private double Q = 2.0; // pheromone increase factor
        private double pheromoneStartingValue = 1;
        private int[] bestPath;
        public int bestPathLength = int.MaxValue;
        private int usedTrucks;

        private Ant[] ants;
        private int capacity;
        private int numberOfTrucks;
        private int numberOfAnts;

        public int[] demands; // indeksy miast od 1, 0 to indeks magazynu i jego demand jest 0
        public int[,] distances;
        public double[,] pheromones;
        public int numberOfCities; // z magazynem

        public AntColony (int citiesCount, int numberOfTrucks, int capacityOfTruck, int[,] dists, int[] needs)
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
            for (int i = 0; i < bestPath.Length; i++)
            {
                if (i == bestPath.Length - 1)
                    Console.WriteLine (i);
                else
                    Console.Write (i + "->");
            }
            Console.WriteLine ("Length: " + bestPathLength);
            Console.WriteLine ("-------------------");
        }

        public void StartAlgorithm ()
        {
            // parametry już mam
            // inicjalizujemy pheromones

            Console.WriteLine ("Starting algorithm...");
            int loop = 0;

            while (loop < maxTime)
            {
                if (loop % 100 == 0)
                {
                    Console.WriteLine ("Still going... Loop " + loop);
                }

                ResetAnts ();

                // construct ant solutions
                ConstructSolutions ();
                
                FindBestTrail ();

                // update pheromones
                UpdatePheromones ();

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
            foreach (var ant in ants)
            {
                if (ant.Length < bestPathLength)
                {
                    bestPathLength = ant.Length;
                    bestPath = ant.Path.ToArray ();
                    usedTrucks = ant.UsedTrucks;

                    Console.WriteLine ("A better path was found.");
                    PrintPathInformation ();
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
                        if (ant.EdgeInTrail (i, j))
                        {
                            increase = Q / ant.Length;
                        }
                        pheromones[i, j] += increase;
                    }

                    pheromones[j, i] = pheromones[i, j];
                }
            }
        }
    }
}
