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

        var contiguousSegment = this.GetContiguousSegmentFromList(stations);
        if (contiguousSegment != null)
        {
            var processedContiguous = contiguousSegment.Stations.ToList();
            foreach (var station in processedContiguous)
            {
                stations.Remove(station);
            }
        }

        var expressSegmentsWithStops = this.GetExpressWithStopsSegment(stations);
        var processedExpressWithStopsStations = expressSegmentsWithStops.SelectMany(s => s.Stations).ToList();

        var whatsLeft = stations.Except(processedExpressWithStopsStations).ToList();
        var pureExpressSegment = this.GetExpressSegment(whatsLeft);
        var processedExpressStations = pureExpressSegment.SelectMany(s => s.Stations).ToList();

        var toProcess = whatsLeft.Except(processedExpressStations).ToList();

        if (contiguousSegment == null)
        {
            contiguousSegment = this.GetContiguousSegmentFromList(toProcess);
        }

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

            expressSegments.Add(new Segment(expressStations, true, false, false));
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
                    expressSegments.Add(new Segment(expressStations, true, true, false));
                }
            }
            i++;
        }

        return expressSegments;
    }

    public Segment? GetContiguousSegment(List<Station> stations)
    {
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
            return new Segment(stopsInList, false, false, true);
        }
        return null;
    }

    public Segment? GetContiguousSegmentFromList(List<Station> stations)
    {
        var contiguousSection = FilterAdjacentItems(stations);

        if (contiguousSection.Any())
        {
            return new Segment(contiguousSection, false, false, true);
        }

        return null;
    }

    private string ProcessSegments(List<Segment> segments)
    {
        var output = string.Empty;
        var clauses = new List<string>();

        var orderedSegments = segments.OrderBy(s => s.Order).ToList();
        SetContiguousStatus(orderedSegments);
        var index = 0;
        foreach (var segment in orderedSegments)
        {
            // TODO: if the last station of the previous section equals the first station of the next section then no need to name it in the announcement

            if (segment is { Express: true, HasIntermediateStops: true, IsPreviousContiguous: false })
            {
                clauses.Add($"runs express from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[2].StationName}, stopping only at {segment.StoppingStations[1].StationName}");
            }

            if (segment is { Express: true, HasIntermediateStops: false, IsPreviousContiguous: false })
            {
                clauses.Add($"runs express from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[1].StationName}");
            }

            if (segment is { Express: false, HasIntermediateStops: false, IsContiguous: true })
            {
                if (index == 0)
                {
                    clauses.Add(
                        $"runs from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[^1].StationName} stopping all stations");
                }

                if (index == orderedSegments.Count - 1)
                {
                    clauses.Add($"runs to {segment.StoppingStations[^1].StationName} stopping all stations");
                }
            }

            if (segment is { IsContiguous: true })
            {
                clauses.Add($"runs express to {segment.StoppingStations.Last().StationName}");
            }
            index++;
        }

        var compoundDescription = "This train " + string.Join(" then ", clauses);

        return compoundDescription;
    }

    private void SetContiguousStatus(List<Segment> orderedSegments)
    {
        for (var i = 0; i < orderedSegments.Count; i++)
        {
            try
            {
                if (orderedSegments[i - 1].IsContiguous)
                {
                    orderedSegments[i].IsPreviousContiguous = true;
                }

                if (orderedSegments[i + 1].IsContiguous)
                {
                    orderedSegments[i].IsNextContiguous = true;
                }
            }
            catch
            {
                // TODO: null checks on previous and next items in the list
            }
        }
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

    public static List<Station> FilterAdjacentItems(List<Station> stations)
    {
        var lastServedIndex = stations.FindLastIndex(s => s.StationStop);
        var truncatedStations = stations.Take(lastServedIndex + 1).ToList();
        if (!truncatedStations.Any()) return stations;

        var firstStop = truncatedStations.First(s => s.StationStop);

        List<Station> result = new List<Station>();
        var searchList = stations.Skip(truncatedStations.IndexOf(firstStop) - 1).ToList();

        for (int i = 0; i < searchList.Count; i++)
        {
            bool isAdjacentTrue = stations[i].StationStop == true && ((i > 0 && stations[i - 1].StationStop) || (i < stations.Count - 1 && stations[i + 1].StationStop));

            if (isAdjacentTrue)
            {
                result.Add(stations[i]);
            }
            else
            {
                return result;
            }
        }

        return result;
    }
}
