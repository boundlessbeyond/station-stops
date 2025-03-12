using System;
using System.IO;

namespace StationStops;

internal class Program
{
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

        var stations = Helpers.GetStations(path);

        var output = service.GetAnnouncement(stations);

        Console.WriteLine(output);
        Console.WriteLine("Press 'enter' to end program");
        Console.ReadLine();
    }
}
