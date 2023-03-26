using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSI2_CVRP
{
    public class Ant
    {
        public int Capacity;
        public int CurrentLoad;
        public int CurrentCity;
        public List<int> Path;
        public bool[] Visited; // magazyn + miasta
        public int Length;
        public int UsedTrucks;
        private int index;
        public double[] probablities;

        public Ant (int capacity, int startCity, int cityDemand, int distance, int numberOfCities, int name)
        {
            Capacity = capacity;
            CurrentLoad = capacity;
            CurrentCity = startCity;
            Path = new ();
            Visited = new bool[numberOfCities];
            Visited[startCity] = true;
            Visited[0] = true;
            Length = 0;
            UsedTrucks = 0;
            index = name;

            Path.Add (0); 
            Path.Add (startCity);
            CurrentLoad -= cityDemand;
            Length += distance;
        }

        public Ant (int capacity, int numberOfCities)
        {
            Capacity = capacity;
            CurrentLoad = capacity;
            Path = new ();
            Visited = new bool[numberOfCities];
            Visited[0] = true;
            Length = 0;
            UsedTrucks = 0;
            Path.Add (0);
        }

        public void PrintPath ()
        {
            Console.WriteLine ("Path: ");
            foreach (int vertex in Path)
            {
                Console.Write (vertex + "->");
            }
            Console.WriteLine ("\nLength = " + Length);
            Console.WriteLine ("Current city: " + CurrentCity);
        }

        private bool CanVisitCity (int city, int[] demands)
        {
            if (Visited[city] || demands[city] > CurrentLoad)
                return false;
            else
                return true;
            
        }

        public void ChooseNextCity (int[,] distances, int[] demands, double[,] pheromones, double alpha, double beta, Random rand)
        {
            probablities = new double[demands.Length];
            double factor = 0; 

            for (int i = 1; i < demands.Length; i++)
            {
                if (CanVisitCity (i, demands))
                {
                    factor += Math.Pow (pheromones[CurrentCity, i], alpha) * Math.Pow ((double)1 / distances[CurrentCity, i], beta);
                }
            }
            if (factor < 0.0000000001)
                factor = 0.0000000001;
            else if (factor > 1000000000)
                factor = 1000000000;

            for (int i = 1; i < demands.Length; i++)
            {
                if (CanVisitCity (i, demands))
                {
                    probablities[i] += Math.Pow (pheromones[CurrentCity, i], alpha) * Math.Pow ((double)1 / distances[CurrentCity, i], beta) / factor;
                }
                else
                {
                    probablities[i] = 0;
                }
            }

            // ruletka
            for (int i = 1; i < demands.Length; i++)
            {
                probablities[i] += probablities[i - 1];
            }

            double draw = rand.NextDouble ();
            int newCity = -1;
            for (int i = 0; i < probablities.Length; i++)
            {
                if (i != CurrentCity && probablities[i] > draw)
                {
                    newCity = i;
                    break;
                }
            }

            // problem, gdy probabilities mamy mega małe, i ruletka w nic nie wpada
            if (newCity == -1 && Path.Last() == 0 && !AntDone(distances))
            {
                double maxProb = probablities.ToList ().Max ();
                int maxInd = probablities.ToList ().FindIndex (0, probablities.Length, (x => x == maxProb));

                newCity = maxInd;
            }

            if (newCity != -1)
            {
                Path.Add (newCity);
                CurrentLoad -= demands[newCity];
                Length += distances[CurrentCity, newCity];
                Visited[newCity] = true;
                CurrentCity = newCity;
            }
            else
            {
                Path.Add (0);
                Length += distances[CurrentCity, 0];
                CurrentLoad = Capacity;
                CurrentCity = 0;
                UsedTrucks++;
            }
        }

        public void ChooseNextCityWithoutSpecificCities (int[,] distances, int[] demands, double[,] pheromones, double alpha, double beta, Random rand, int wrongCity1, int wrongCity2)
        {
            probablities = new double[demands.Length];
            double factor = 0;

            for (int i = 1; i < demands.Length; i++)
            {
                if (CanVisitCity (i, demands))
                {
                    factor += Math.Pow (pheromones[CurrentCity, i], alpha) * Math.Pow ((double)1 / distances[CurrentCity, i], beta);
                }
            }
            if (factor < 0.0000000001)
                factor = 0.0000000001;
            else if (factor > 1000000000)
                factor = 1000000000;

            for (int i = 1; i < demands.Length; i++)
            {
                if (CanVisitCity (i, demands) && i != wrongCity1 && i != wrongCity2)
                {
                    probablities[i] += Math.Pow (pheromones[CurrentCity, i], alpha) * Math.Pow ((double)1 / distances[CurrentCity, i], beta) / factor;
                }
                else
                {
                    probablities[i] = 0;
                }
            }

            // ruletka
            for (int i = 1; i < demands.Length; i++)
            {
                probablities[i] += probablities[i - 1];
            }

            double draw = rand.NextDouble ();
            int newCity = -1;
            for (int i = 0; i < probablities.Length; i++)
            {
                if (i != CurrentCity && probablities[i] > draw)
                {
                    newCity = i;
                    break;
                }
            }

            // problem, gdy probabilities mamy mega małe, i ruletka w nic nie wpada
            if (newCity == -1 && Path.Last () == 0 && !AntDone (distances))
            {
                double maxProb = probablities.ToList ().Max ();
                int maxInd = probablities.ToList ().FindIndex (0, probablities.Length, (x => x == maxProb));

                newCity = maxInd;
            }

            if (newCity != -1)
            {
                Path.Add (newCity);
                CurrentLoad -= demands[newCity];
                Length += distances[CurrentCity, newCity];
                Visited[newCity] = true;
                CurrentCity = newCity;
            }
            else
            {
                Path.Add (0);
                Length += distances[CurrentCity, 0];
                CurrentLoad = Capacity;
                CurrentCity = 0;
                UsedTrucks++;
            }
        }

        public bool AntDone (int[,] distances)
        {
            bool result = true;
            for (int i = 0; i < Visited.Length; i++)
            {
                if (!Visited[i])
                {
                    result = false;
                    break;
                }
            }
            
            if (result)
            {
                //dodajemy jeszcze powrót do magazynu
                Path.Add (0);
                Length += distances[CurrentCity, 0];
                CurrentCity = 0;
                UsedTrucks++;
            }

            return result;
        }

        public bool EdgeInTrail (int start, int end)
        {
            bool result = false;
            for (int i = 0; i < Path.Count - 1; i++)
            {
                if (Path[i] == start && Path[i+1] == end)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void AddSpecificCityTOPath(int newCity, int demand, int distance, int distanceFromMagazine)
        {
            if (newCity != 0)
            {
                if (CurrentLoad > demand)
                {
                    Path.Add (0);
                    CurrentLoad = Capacity;
                    Length += distanceFromMagazine;
                    UsedTrucks++;
                }
                Path.Add (newCity);
                CurrentLoad -= demand;
                Length += distance;
                Visited[newCity] = true;
                CurrentCity = newCity;
            }
            else
            {
                Path.Add (0);
                CurrentLoad = Capacity;
                Length += distanceFromMagazine;
                UsedTrucks++;
            }
            
        }
    }
}
