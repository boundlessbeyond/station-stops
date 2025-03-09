using System.Collections.Generic;
using System.Linq;

namespace StationStops;
public static class Helpers
{
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
