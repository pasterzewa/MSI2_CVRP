using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSI2_CVRP
{
    internal class FileReader
    {
        public int n; // cities count
        public int c; // capacity
        //int m; // trucks
        public int[,] distances;
        public int[] demands;

        public FileReader(string filePath)
        {
            string text = System.IO.File.ReadAllText (filePath);

            text = text.Substring (text.IndexOf("DIMENSION"));
            text = text.Substring (text.IndexOf (":"));

            string nInText = text.Substring (2, text.IndexOf("\r") - 2);
            n = Int32.Parse (nInText);

            text = text.Substring (text.IndexOf ("CAPACITY"));
            text = text.Substring (text.IndexOf(":"));

            string cInText = text.Substring (2, text.IndexOf ("\r") - 2);
            c = Int32.Parse (cInText);

            text = text.Substring (text.IndexOf ("SECTION"));
            string coords = text.Substring (text.IndexOf ("1"), text.IndexOf("\r\nDEMAND") - text.IndexOf ("1"));
            StringCoordsToDists (coords);
            
            string demnads = text.Substring (text.IndexOf ("DEMAND"));
            demnads = demnads.Substring (demnads.IndexOf ("1"), demnads.IndexOf ("DEPOT") - demnads.IndexOf ("1"));
            DemnadsToDemands (demnads);
        }

        private void StringCoordsToDists (string coords)
        {
            List<(int, int)> coordinates = new List<(int, int)>();

            string[] numbers = coords.Split ();
            int ind = 3;
            while (numbers[ind].Equals("") || numbers[ind].Equals (" "))
            {
                ind++;
            }

            for (int i = 0; i < numbers.Length; i+=ind)
            {
                coordinates.Add ((Int32.Parse (numbers[i + 1]), Int32.Parse (numbers[i+2])));
            }

            // teraz możemy policzyć odległości
            distances = new int[coordinates.Count, coordinates.Count];
            for (int k = 0; k < coordinates.Count; k++)
            {
                for (int j = k; j < coordinates.Count; j++)
                {
                    // d=√((x_2-x_1)²+(y_2-y_1)²)
                    distances[k, j] = (int)Math.Round(Math.Sqrt (Math.Pow (coordinates[k].Item1 - coordinates[j].Item1, 2) + Math.Pow(coordinates[k].Item2 - coordinates[j].Item2, 2)));
                    distances[j, k] = distances[k, j];
                }
            }
        }

        private void DemnadsToDemands (string str)
        {
            List<int> dems = new List<int> ();

            string[] numbers = str.Split ();
            int ind = 3;
            while (numbers[ind].Equals ("") || numbers[ind].Equals (" "))
            {
                ind++;
            }

            for (int i = 0; i < numbers.Length - 1; i += ind)
            {
                dems.Add (Int32.Parse (numbers[i + 1]));
            }
            demands = dems.ToArray ();
        }
    }
}
