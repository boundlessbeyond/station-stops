using System;
using System.Collections.Generic;
using System.IO;

namespace StationStops;

public class Station(string stationName, bool stationStop, int originalIndex) : IEquatable<Station>
{
    public string StationName { get; init; } = stationName;
    public bool StationStop { get; init; } = stationStop;
    //public int OriginalIndex { get; set; } = originalIndex;
    public int Index { get; set; } = originalIndex;

    public bool Equals(Station? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return StationName == other.StationName && StationStop == other.StationStop && Index == other.Index;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Station)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StationName, StationStop, Index);
    }
}

internal class Program
{
    private static readonly ExpressTrainStopService expressService = new();
    private static readonly TrainStopService service = new();

    static void Main(string[] args)
    {
        var input = "Please enter the file path: ";
        Console.Write(input);
        var path = Console.ReadLine();

        while (!File.Exists(path))
        {
            Console.WriteLine("File not found. Please try again.");
            Console.Write(input);
            path = Console.ReadLine();
        }

        var stations = new List<Station>();

        var fileLines = File.ReadAllLines(path);
        var lineNumber = 0;
        foreach (var line in fileLines)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            var lineParts = line.Split(',');
            if (lineParts.Length != 2)
            {
                Console.WriteLine($"Invalid station stop: {line}.");
                continue;
            }

            var stationName = lineParts[0].Trim();
            var stationStop = lineParts[1].Trim().ToLower();
            var isStop = stationStop == "true";

            stations.Add(new Station(stationName, isStop, lineNumber));
            lineNumber++;
        }

        var output = service.GetAnnouncer(stations);

        Console.WriteLine(output);
        Console.WriteLine("Press 'enter' to end program");
        Console.ReadLine();
    }
}
