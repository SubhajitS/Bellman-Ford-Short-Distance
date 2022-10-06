// See https://aka.ms/new-console-template for more information
namespace Console1
{
    public class Edge 
    {
        public string src { get; set; }
        public string dest { get; set; }
        public double weight { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var no_of_vertices = 8;
            var edges = new List<Edge>{
                new Edge{
                    src = "S1",
                    dest = "S2",
                    weight = -0.35
                },
                new Edge{
                    src = "S1",
                    dest = "S3",
                    weight = -0.35
                },
                new Edge{
                    src = "S2",
                    dest = "S3",
                    weight = -0.91
                },
                new Edge{
                    src = "S2",
                    dest = "S4",
                    weight = -0.22
                },
                new Edge{
                    src = "S2",
                    dest = "S5",
                    weight = -0.91
                },
                new Edge{
                    src = "S3",
                    dest = "S5",
                    weight = -0.35
                },
                new Edge{
                    src = "S4",
                    dest = "S5",
                    weight = -0.35
                },
                new Edge{
                    src = "S5",
                    dest = "S6",
                    weight = -1.2
                },
                new Edge{
                    src = "S6",
                    dest = "S7",
                    weight = -1.2
                },
                new Edge{
                    src = "S7",
                    dest = "S8",
                    weight = -1.6
                }
            };

            
            Dictionary<string, double> distance_from_source = FindShortestDistanceUsingBellmanFord(no_of_vertices, edges);

            //Backtrack to determine the shortest path
            var shortestPath = new List<Edge>();
            FindShortestPath(edges, distance_from_source, shortestPath);

            string initialWeakLink = string.Join("-", shortestPath.Select(e => e.dest).Append("S1"));
            Console.WriteLine($"Initial weak link {initialWeakLink}");
            string currentWeakLink = initialWeakLink;
            int terminalCounter = 0;
            while(initialWeakLink.Equals(currentWeakLink) && terminalCounter < 10)
            {
                terminalCounter++;
                //reduce the weight of shortest path by 10%
                foreach (Edge edge in shortestPath)
                {
                    edge.weight = edge.weight + (edge.weight* 0.1);
                }
                distance_from_source = FindShortestDistanceUsingBellmanFord(no_of_vertices, edges);
                FindShortestPath(edges, distance_from_source, shortestPath);
                currentWeakLink = string.Join("-", shortestPath.Select(e => e.dest).Append("S1"));
                Console.WriteLine($"Path is unchanged - {currentWeakLink}");
            }
            if(!initialWeakLink.Equals(currentWeakLink))
                Console.WriteLine($"Path has changed - {currentWeakLink}");
        }

        private static void FindShortestPath(List<Edge> edges, Dictionary<string, double> distance_from_source, List<Edge> shortestPath)
        {
            shortestPath.Clear();
            var starting_vertice = "S1";
            var destination_vertice = "S8";
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
                    var w = edge.weight;

                    if (distance_from_source[d] > distance_from_source[s] + w)
                        distance_from_source[d] = distance_from_source[s] + w;
                }
                Console.WriteLine($"Distance from source: S8 - {distance_from_source["S8"]} | S7 - {distance_from_source["S7"]} | S6 - {distance_from_source["S6"]} | S5 - {distance_from_source["S5"]} | S4 - {distance_from_source["S4"]} | S3 - {distance_from_source["S3"]} | S2 - {distance_from_source["S2"]}");
            }

            return distance_from_source;
        }
    }
}



