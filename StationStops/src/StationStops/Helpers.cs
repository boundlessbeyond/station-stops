using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StationStops;
public static class Helpers
{
    /// <summary>
    /// Get the list of stations from a file
    /// </summary>
    /// <param name="path">path to the file</param>
    /// <returns>list of Stations and if the train stops there</returns>
    public static List<Station> GetStations(string path)
    {
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
                continue;
            }

            var stationName = lineParts[0].Trim();
            var stationStop = lineParts[1].Trim().ToLower();
            var isStop = stationStop == "true";

            stations.Add(new Station(stationName, isStop, lineNumber));
            lineNumber++;
        }

        return stations;
    }

    /// <summary>
    /// Create a list of adjacent station stops starting at the first found stop. 
    /// Bails as soon as the next station is express.
    /// </summary>
    /// <param name="stations">List of stations to get contiguous stops</param>
    /// <returns>Returns a list of stations from the first stop, where the next station is a stop.</returns>
    public static List<Station> FilterAdjacentItems(List<Station> stations)
    {
        var lastServedIndex = stations.FindLastIndex(s => s.StationStop);
        var truncatedStations = stations.Take(lastServedIndex + 1).ToList();
        if (!truncatedStations.Any()) return stations;

        var firstStop = truncatedStations.First(s => s.StationStop);

        var adjacentStations = new List<Station>();
        var searchList = stations.Skip(truncatedStations.IndexOf(firstStop) - 1).ToList();

        for (var i = 0; i < searchList.Count; i++)
        {
            var isAdjacentTrue = stations[i].StationStop == true && ((i > 0 && stations[i - 1].StationStop) || (i < stations.Count - 1 && stations[i + 1].StationStop));

            if (isAdjacentTrue)
            {
                adjacentStations.Add(stations[i]);
            }
            else
            {
                return adjacentStations;
            }
        }

        return adjacentStations;
    }

}
