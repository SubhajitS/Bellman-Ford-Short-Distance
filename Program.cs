// See https://aka.ms/new-console-template for more information
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Console1
{
    public class Edge
    {
        public string src { get; set; }
        public string dest { get; set; }
        public double weight { get; set; }
        public bool isSaturated { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var (no_of_vertices, edges, weight_increment_factor, threshold_minimum_distance, terminal_counter, edge_saturation_threshold) = GetInputs("configuration.json");

            Dictionary<string, double> distance_from_source = FindShortestDistanceUsingBellmanFord(no_of_vertices, edges);

            //Backtrack to determine the shortest path
            var shortestPath = new List<Edge>();
            FindShortestPath(edges, distance_from_source, shortestPath, no_of_vertices);

            string initialWeakPathString = string.Join("-", shortestPath.Select(e => e.dest).Append("S1"));
            Console.WriteLine($"Initial weak link {initialWeakPathString} - distance: {distance_from_source[$"S{no_of_vertices}"]}");
            string currentWeakPathString = initialWeakPathString;
            int terminalCounter = 0;
            while (terminalCounter < terminal_counter && distance_from_source[$"S{no_of_vertices}"] < threshold_minimum_distance)
            {
                Console.WriteLine("----------- New iteration ---------------");
                terminalCounter++;
                //increase the weight of weakest edge of shortest path by weight_increment_factor
                Edge weakestEdge = null;
                if (shortestPath.Any(e => IncreaseWeight(e.weight, weight_increment_factor) < edge_saturation_threshold))
                {
                    foreach (Edge edge in shortestPath.Where(e => IncreaseWeight(e.weight, weight_increment_factor) < edge_saturation_threshold))
                    {
                        if (weakestEdge == null)
                            weakestEdge = edge;
                        else if (weakestEdge?.weight > edge.weight)
                            weakestEdge = edge;
                    }
                    if (weakestEdge != null)
                        weakestEdge.weight = IncreaseWeight(weakestEdge.weight, weight_increment_factor);
                }
                else
                {
                    foreach (Edge edge in shortestPath)
                    {
                        edge.isSaturated = true;
                    }
                }
                distance_from_source = FindShortestDistanceUsingBellmanFord(no_of_vertices, edges);
                FindShortestPath(edges, distance_from_source, shortestPath, no_of_vertices);
                currentWeakPathString = string.Join("-", shortestPath.Select(e => e.dest).Append("S1"));
                Console.WriteLine($"Shortest path is - {currentWeakPathString} - distance: {distance_from_source[$"S{no_of_vertices}"]}");
            }
            if (!initialWeakPathString.Equals(currentWeakPathString))
                Console.WriteLine($"Path has changed - {currentWeakPathString} - distance: {distance_from_source[$"S{no_of_vertices}"]}");
        }

        private static double IncreaseWeight(double weight, double weightIncrementFactor) => weight + Math.Abs(weight * weightIncrementFactor);

        private static (int no_of_vertices, List<Edge> edges) GetInput()
        {
            Console.WriteLine("Please note vertices would be named from S1, S2,...Sn, where S1 would be the starting point and Sn would be the destination");
            Console.WriteLine("Total number of Vertices: ");
            if (!int.TryParse(Console.ReadLine(), out var no_of_vertices))
                Console.WriteLine("Error! invalid input provided. Integers expected.");

            Console.WriteLine("Total number of Edges: ");
            if (!int.TryParse(Console.ReadLine(), out var no_of_edges))
                Console.WriteLine("Error! invalid input provided. Integers expected.");

            Console.WriteLine("Provide the edge details as following");
            string src, dest;
            double weight = 0.0;
            var edges = new List<Edge>();
            for (int i = 0; i < no_of_edges; i++)
            {
                Console.WriteLine($"Edge {i + 1} Starting vertice (e.g. S1): ");
                src = Console.ReadLine() ?? string.Empty;
                Console.WriteLine($"Edge {i + 1} Destination vertice (e.g. S2): ");
                dest = Console.ReadLine() ?? string.Empty;
                Console.WriteLine($"Distance between {src} and {dest}: ");
                while (!Double.TryParse(Console.ReadLine(), out weight))
                {
                    Console.WriteLine("Error! Provide a valid double value");
                }
                edges.Add(new Edge
                {
                    src = src,
                    dest = dest,
                    weight = weight
                });
            }

            return (no_of_vertices, edges);
        }

        private static (int no_of_vertices, List<Edge> edges, double weight_increment_factor, double threshold_minimum_distance, int terminal_counter, double edge_saturation_threshold) GetInputs(string configurationFileName)
        {
            string executingAssemblyRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
            var path = Path.Combine(executingAssemblyRoot, configurationFileName);
            var content = JObject.Parse(File.ReadAllText(path));
            var no_of_vertices = content.SelectToken("no_of_vertices")?.Value<int>() ?? 0;
            var terminal_counter = content.SelectToken("terminal_counter")?.Value<int>() ?? 50;
            var weight_increment_factor = content.SelectToken("weight_increment_factor")?.Value<double>() ?? 0.1;
            var threshold_minimum_distance = content.SelectToken("threshold_minimum_distance")?.Value<double>() ?? 0.0;
            var edge_saturation_threshold = content.SelectToken("edge_saturation_threshold")?.Value<double>() ?? 0.0;
            var edges = JsonConvert.DeserializeObject<List<Edge>>(content.SelectToken("edges")?.Value<JArray>()?.ToString() ?? "[]") ?? new List<Edge>();
            return (no_of_vertices, edges, weight_increment_factor, threshold_minimum_distance, terminal_counter, edge_saturation_threshold);
        }

        private static void FindShortestPath(List<Edge> edges, Dictionary<string, double> distance_from_source, List<Edge> shortestPath, int no_of_vertices)
        {
            shortestPath.Clear();
            var starting_vertice = "S1";
            var destination_vertice = $"S{no_of_vertices}";
            string selectedVertices = destination_vertice;
            while (selectedVertices != starting_vertice)
            {
                var prevPath = edges.Where(x => x.dest == selectedVertices && (x.weight + distance_from_source[x.src]) == distance_from_source[x.dest]);
                shortestPath.AddRange(prevPath);
                selectedVertices = prevPath.ElementAt(0).src;
            }
        }

        private static Dictionary<string, double> FindShortestDistanceUsingBellmanFord(int no_of_vertices, List<Edge> edges)
        {
            Dictionary<string, double> distance_from_source = new Dictionary<string, double>();
            int i = 0;
            for (; i < no_of_vertices; i++)
            {
                if (i == 0) distance_from_source.Add("S1", 0);
                else distance_from_source.Add($"S{i + 1}", 999);
            }

            for (i = 0; i < no_of_vertices - 1; i++)
            {
                foreach (var edge in edges)
                {
                    var s = edge.src;
                    var d = edge.dest;
                    var w = edge.isSaturated ? 0 : edge.weight;

                    if (distance_from_source[d] > distance_from_source[s] + w)
                        distance_from_source[d] = distance_from_source[s] + w;
                }
            }

            return distance_from_source;
        }
    }
}



