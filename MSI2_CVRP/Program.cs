using System.ComponentModel;
using System;
using System.Reflection.PortableExecutable;
using System.Diagnostics;

namespace MSI2_CVRP
{
    internal class Program
    {
        static void Main (string[] args)
        {
            Console.WriteLine ("Please choose an option:");
            Console.WriteLine ("1. Start test based on data from file.");
            Console.WriteLine ("2. Start random test based on your inputs.");
            Console.Write ("Your choice (1 or 2): ");
            string userInput = Console.ReadLine ();
            switch (userInput)
            {
                case "1":
                    StartFromFile ();
                    break;
                case "2":
                    StartFromInput ();
                    break;
                default:
                    Console.WriteLine ("Incorrect input");
                    break;
            }
        }

        static void StartFromFile ()
        {
            Console.Write ("Please input full file path: ");
            string filePath = Console.ReadLine ();
            if (filePath != null)
            {
                FileReader reader = new FileReader (filePath);
                PrintInformation (reader.distances, reader.demands);
                Start (reader.n, -1, reader.c, reader.distances, reader.demands);
            }
            else
                Console.WriteLine ("Incorrect input");
        }

        static void StartFromInput ()
        {
            Console.Write ("Please input number of cities: ");
            int citiesCount = Int32.Parse (Console.ReadLine ());
            Console.Write ("Please input capacity of a truck: ");
            int capacity = Int32.Parse (Console.ReadLine ());
            Console.Write ("Please input number of trucks: ");
            int numberOfTrucks = Int32.Parse (Console.ReadLine ());
            Console.Write ("Please input seed for Random: ");
            int seed = Int32.Parse (Console.ReadLine ());

            // generowanie distances i demands
            Random random = new Random (seed);
            int[] demands = new int[citiesCount];
            for (int i = 1; i < citiesCount; i++)
            {
                demands[i] = random.Next (1, capacity + 1);
            }

            int[,] distances = new int[citiesCount, citiesCount];
            for (int i = 0; i < citiesCount; i++)
            {
                for (int j = i; j < citiesCount; j++)
                {
                    if (i == j)
                    {
                        distances[i, j] = 0;
                    }
                    else
                    {
                        distances[i, j] = random.Next (1, 50);
                        distances[j, i] = distances[i, j];
                    }
                }
            }

            PrintInformation (distances, demands);

            Start (citiesCount, numberOfTrucks, capacity, distances, demands);
        }

        static void Start(int citiesCount, int trucksCount, int capacity, int[,] distances, int[] demands)
        {
            Stopwatch stopwatch = new Stopwatch ();

            Console.WriteLine ("\n-------------ACO-------------");
            AntColony colony = new AntColony (citiesCount, trucksCount, capacity, distances, demands);
            stopwatch.Start ();
            colony.StartAlgorithm ();
            stopwatch.Stop ();
            Console.WriteLine ("Time elapsed (in miliseconds): " + stopwatch.ElapsedMilliseconds);

            Console.WriteLine ("\n-------------MMAS-------------");
            MMAS colonyMMAS = new MMAS (citiesCount, trucksCount, capacity, distances, demands);
            stopwatch = Stopwatch.StartNew ();
            stopwatch.Start ();
            colonyMMAS.StartAlgorithm ();
            stopwatch.Stop ();
            Console.WriteLine ("Time elapsed (in miliseconds): " + stopwatch.ElapsedMilliseconds);

            Console.WriteLine ("\n-------------EACOL-------------");
            EACOLS eacolColony = new EACOLS (citiesCount, trucksCount, capacity, distances, demands);
            stopwatch = Stopwatch.StartNew ();
            stopwatch.Start ();
            eacolColony.StartAlgorithm ();
            stopwatch.Stop ();
            Console.WriteLine ("Time elapsed (in miliseconds): " + stopwatch.ElapsedMilliseconds);

            Console.WriteLine ("\n-------------Greedy-------------");
            Greedy greedy = new Greedy (citiesCount, trucksCount, capacity, distances, demands);
            stopwatch = Stopwatch.StartNew ();
            stopwatch.Start ();
            greedy.StartAlgorithm ();
            stopwatch.Stop ();
            Console.WriteLine ("Time elapsed (in miliseconds): " + stopwatch.ElapsedMilliseconds);

            Console.WriteLine ("\n\nBest algorithm(s): ");
            List<int> results = new List<int> { colony.bestPathLength, colonyMMAS.bestGlobalPathLength,
            eacolColony.bestGlobalPathLength, greedy.bestPathLength};
            int bestPathLength = results.Min ();

            if (colony.bestPathLength == bestPathLength)
                Console.WriteLine ("-basic ACO");
            if (colonyMMAS.bestGlobalPathLength == bestPathLength)
                Console.WriteLine ("-MMAS");
            if (eacolColony.bestGlobalPathLength == bestPathLength)
                Console.WriteLine ("-EACOL");
            if (greedy.bestPathLength == bestPathLength)
                Console.WriteLine ("-greedy");
        }

        static void PrintInformation (int[,] distances, int[] demands)
        {
            Console.WriteLine ("Distances");
            for (int i = 0; i < distances.GetLength (0); i++)
            {
                for (int j = 0; j < distances.GetLength (1); j++)
                {
                    Console.Write (System.String.Format ("{0,4}", distances[i, j]));
                }
                Console.WriteLine ();
            }

            Console.WriteLine ("--------------------");
            Console.WriteLine ("Demands");
            for (int i = 0; i < demands.Length; i++)
            {
                Console.Write (demands[i] + " ");
            }
            Console.WriteLine ();
        }
    }

}
