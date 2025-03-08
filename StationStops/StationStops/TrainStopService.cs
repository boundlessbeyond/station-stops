using System;
using System.Collections.Generic;
using System.Linq;

namespace StationStops;
internal class TrainStopService
{
    public string CalculateStops(List<Station> stations)
    {
        switch (stations.Count)
        {
            case < 1:
                return "No station stops found in supplied list.";
            case < 2:
                return "Please supply more than one station stop.";
        }

        // Truncate the list after the last stopping station.
        var lastStopIndex = stations.FindLastIndex(s => s.StationStop);
        if (lastStopIndex == -1)
        {
            return "There must be at least one stopping station.";
        }

        var truncatedStations = stations.Take(lastStopIndex + 1).ToList();

        var servedStops = truncatedStations.Where(s => s.StationStop).ToList();
        // Count express stations in the truncated sequence.
        int expressCountOverall = truncatedStations.Count(s => !s.StationStop);

        if (expressCountOverall is 1)
        {
            var expressStation = truncatedStations.First(s => !s.StationStop);
            return $"This train stops at all stations except {expressStation.StationName}";
        }

        if (servedStops.Count is 2)
        {
            return $"This train stops at {servedStops[0].StationName} and {servedStops[1].StationName} only";
        }

        var expressSegmentsWithStops = this.GetExpressWithStopsSegment(stations);
        var processedExpressWithStopsStations = expressSegmentsWithStops.SelectMany(s => s.Stations).ToList();

        var whatsLeft = stations.Except(processedExpressWithStopsStations).ToList();
        var pureExpressSegment = this.GetExpressSegment(whatsLeft);
        var processedExpressStations = pureExpressSegment.SelectMany(s => s.Stations).ToList();

        var toProcess = whatsLeft.Except(processedExpressStations).ToList();
        var contiguousSegment = this.GetContiguousSegment(toProcess);


        var segments = expressSegmentsWithStops
            .Concat(pureExpressSegment).ToList();
        if (contiguousSegment != null)
        {
            var contiguousStations = contiguousSegment.Stations;
            var leftOver = toProcess.Except(contiguousStations).ToList();
            if (leftOver.Any())
            {
                // TODO: there could be some stations left over I suppose
            }

            segments.Add(contiguousSegment);
        }

        var output = ProcessSegments(segments);

        return output;
    }

    public List<Segment> GetExpressSegment(List<Station> stations)
    {
        var expressSegments = new List<Segment>();
        if (!stations.Any()) return expressSegments;

        var lastStop = stations.Last(s => s.StationStop);
        var firstStop = stations.First(s => s.StationStop);

        var inbetweenStops = stations.Skip(1) // skip the first station
            .Take(stations.Count - stations.IndexOf(lastStop))
            .Where(s => s.StationStop);

        if (!inbetweenStops.Any())
        {
            var expressStations = stations.Skip(stations.IndexOf(firstStop)).Take(stations.IndexOf(lastStop) + 1).ToList();

            expressSegments.Add(new Segment(expressStations, firstStop.Index, lastStop.Index, true, false));
        }

        return expressSegments;
    }

    /// <summary>
    /// Get the start index and end index of a segment
    /// </summary>
    /// <param name="stations"></param>
    /// <returns></returns>
    public List<Segment> GetExpressWithStopsSegment(List<Station> stations)
    {
        var lastServedIndex = stations.FindLastIndex(s => s.StationStop);

        var expressSegments = new List<Segment>();
        var truncatedStations = stations.Take(lastServedIndex + 1).ToList();
        if (!truncatedStations.Any()) return expressSegments;

        var servedStopsEffective = truncatedStations.Where(s => s.StationStop).ToList();

        var i = 0;

        while (i < servedStopsEffective.Count)
        {
            // Look ahead to see if we can form an express segment.
            // We require three consecutive served stops A, B, C.
            if (i <= servedStopsEffective.Count - 3)
            {
                var firstStop = servedStopsEffective[i];
                var lastStop = servedStopsEffective[i + 2];

                // If the gap from first to last stops is at least 4 (i.e. several stations between them),
                // we assume an express section.
                if (lastStop.Index - firstStop.Index >= 4)
                {
                    i += 3;
                    var expressStations = stations.Skip(firstStop.Index).Take(lastStop.Index + 1).ToList();
                    expressSegments.Add(new Segment(expressStations, firstStop.Index, lastStop.Index, true, true));
                }
            }
            i++;
        }

        return expressSegments;
    }

    public Segment? GetContiguousSegment(List<Station> stations)
    {
        var contiguousSection = new List<Station>();

        var lastServedIndex = stations.FindLastIndex(s => s.StationStop);

        var truncatedStations = stations.Take(lastServedIndex + 1).ToList();
        if (!truncatedStations.Any()) return null;

        var firstStop = truncatedStations.First(s => s.StationStop);
        var lastStop = truncatedStations.Last(s => s.StationStop);

        var indexOfLast = truncatedStations.IndexOf(lastStop);
        var stopsInList = truncatedStations.Skip(truncatedStations.IndexOf(firstStop)) // skip the first station
            .Take(indexOfLast + 1)
            .ToList();

        var contiguous = IsContiguous(stopsInList);

        if (contiguous)
        {
            return new Segment(stopsInList, firstStop.Index, lastStop.Index, false, false);
        }
        return null;
    }

    private string ProcessSegments(List<Segment> segments)
    {
        var output = string.Empty;
        var clauses = new List<string>();

        foreach (var segment in segments)
        {
            if (segment is { Express: true, HasIntermediateStops: true })
            {
                clauses.Add($"runs express from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[2].StationName}, stopping only at {segment.StoppingStations[1].StationName}");
                continue;
            }

            if (segment is { Express: true, HasIntermediateStops: false })
            {
                clauses.Add($"runs express from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[1].StationName}");
                continue;
            }

            if (segment is { Express: false, HasIntermediateStops: false })
            {
                clauses.Add($"runs from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[^1].StationName} stopping all stations");
            }
        }

        var compoundDescription = "This train " + string.Join(" then ", clauses);

        return compoundDescription;
    }

    private static bool IsContiguous(List<Station> stops)
    {
        for (int i = 0; i < stops.Count - 1; i++)
        {
            if (stops[i + 1].Index - stops[i].Index != 1)
            {
                return false;
            }
        }
        return true;
    }

    public class Segment(List<Station> stations, int startIndex, int endIndex, bool express, bool hasIntermediateStops)
    {
        public List<Station> Stations { get; init; } = stations;
        public List<Station> StoppingStations { get; init; } = stations.Where(s => s.StationStop).ToList();
        public int StartIndex { get; init; } = startIndex;
        public int EndIndex { get; init; } = endIndex;
        public bool Express { get; init; } = express;
        public bool HasIntermediateStops { get; init; } = hasIntermediateStops;
    }
}
