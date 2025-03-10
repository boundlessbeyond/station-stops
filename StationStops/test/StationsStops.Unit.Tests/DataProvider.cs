using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StationStops;

namespace StationsStops.Unit.Tests;
public static class DataProvider
{
    public static List<Station> ExpressStationsWithStop()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", false, 1),
            new("South Brisbane", false, 2),
            new("South Bank", true, 3),
            new("Park Road", false, 4),
            new("Buranda", true, 5)
        };

        return stations;
    }

    public static List<Station> ExpressStationsThenExpress()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", false, 1),
            new("South Brisbane", false, 2),
            new("South Bank", true, 3),
            new("Park Road", false, 4),
            new("Buranda", true, 5),
            new("Coorparoo", true, 6),
            new("Norman Park", false, 7),
            new("Morningside", false, 8),
            new("Cannon Hill", true, 9)
        };

        return stations;
    }

    public static List<Station> ExpressStations()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", false, 1),
            new("South Brisbane", false, 2),
            new("South Bank", true, 3)
        };

        return stations;
    }

    public static List<Station> OnlyStopsStations()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", true, 1)
        };

        return stations;
    }

    public static List<Station> ContiguousStationsStoppingAll()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", true, 1),
            new("South Brisbane", true, 2),
            new("South Bank", true, 3),
            new("Park Road", true, 4),
            new("Buranda", true, 5),
            new("Coorparoo", true, 6),
            new("Norman Park", true, 7),
            new("Morningside", true, 8),
            new("Cannon Hill", true, 9)
        };

        return stations;
    }

    public static List<Station> ExpressStationsThenContiguous()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", false, 1),
            new("South Brisbane", false, 2),
            new("South Bank", true, 3),
            new("Park Road", false, 4),
            new("Buranda", true, 5),
            new("Coorparoo", true, 6),
            new("Norman Park", true, 7),
            new("Morningside", true, 8),
            new("Cannon Hill", true, 9)
        };

        return stations;
    }

    public static List<Station> GetExceptStations()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", true, 1),
            new("South Brisbane", false, 2),
            new("South Bank", true, 3)
        };

        return stations;
    }

    public static List<Station> GetJourneyWithoutEnd()
    {
        var stations = new List<Station>
        {
            new("Central", true, 0),
            new("Roma St", true, 1),
            new("South Brisbane", true, 2),
            new("South Bank", true, 3),
            new("Park Road", false, 4),
        };

        return stations;
    }

    public static List<Station> GetJourneyWithoutStart()
    {
        var stations = new List<Station>
        {
            new("Central", false, 0),
            new("Roma St", false, 1),
            new("South Brisbane", false, 2),
            new("South Bank", false, 3),
            new("Park Road", false, 4),
        };

        return stations;
    }
}
