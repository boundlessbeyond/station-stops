using System;
using System.Collections.Generic;
using System.Linq;

namespace StationStops;
public class TrainStopService
{
    /// <summary>
    /// Takes the list of stations in the journey and calculate where the train stops for the output string
    /// </summary>
    /// <param name="stations">List of Stations to calculate the announcers script from</param>
    /// <param name="firstPass">This method may be called recursively on more complex journeys so skip simple checks if not first pass</param>
    /// <returns>Train announcer script</returns>
    public string CalculateStops(List<Station> stations, bool firstPass = true)
    {
        if (firstPass)
        {
            var result = CheckSimpleCase(stations);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
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
        var pureExpressSegment = this.GetPureExpressSegment(whatsLeft);
        var processedExpressStations = pureExpressSegment.SelectMany(s => s.Stations).ToList();

        var toProcess = whatsLeft.Except(processedExpressStations).ToList();

        if (contiguousSegment == null)
        {
            // Check if there's a contiguous train service after the process express stops
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
                // if there are some stations left over try to reprocess from the beginning
                this.CalculateStops(leftOver, false);
            }

            segments.Add(contiguousSegment);
        }

        var output = ProcessSegments(segments);

        return output;
    }

    /// <summary>
    /// If the train journey only has a few stations with a few stops this will return a simple description
    /// </summary>
    /// <param name="stations">all stations to process</param>
    /// <returns>Simple announcer outcome from a limited list of stations or null if a more complex journey is found</returns>
    public static string? CheckSimpleCase(List<Station> stations)
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
        if (expressCountOverall is 0 && servedStops.Count is 2)
        {
            return $"This train stops at {servedStops[0].StationName} and {servedStops[1].StationName} only";
        }

        return null;
    }

    /// <summary>
    /// Create a segment of the train journey where the train runs express from start to end without stopping inbetween
    /// </summary>
    /// <param name="stations">list of stations to check for an express segment</param>
    /// <returns>Express train journey segment</returns>
    public List<Segment> GetPureExpressSegment(List<Station> stations)
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
    /// Create a segment of the train journey where there is an express section with a stop in the middle
    /// </summary>
    /// <param name="stations">list of stations to check for an express with a stop segment</param>
    /// <returns>Express train journey segment</returns>
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

    /// <summary>
    /// Converts the contiguous list of stations in to a train journey segment
    /// </summary>
    /// <param name="stations">List of stations to find the list of contiguous stopping stations in</param>
    /// <returns>Train journey segment or null if no contiguous station stops are found</returns>
    public Segment? GetContiguousSegmentFromList(List<Station> stations)
    {
        var contiguousSection = Helpers.FilterAdjacentItems(stations);

        if (contiguousSection.Any())
        {
            return new Segment(contiguousSection, false, false, true);
        }

        return null;
    }

    /// <summary>
    /// Take our list of train journey segments and produce the announcer text.
    /// </summary>
    /// <param name="segments">The train journey broken up in to segments of express segments and contiguous segments</param>
    /// <returns>Train announcer info of where the train stops</returns>
    private string ProcessSegments(List<Segment> segments)
    {
        var output = string.Empty;
        var clauses = new List<string>();

        var orderedSegments = segments.OrderBy(s => s.Order).ToList();
        SetContiguousStatus(orderedSegments);
        var index = 0;
        foreach (var segment in orderedSegments)
        {
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
                clauses.Add($"runs from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[^1].StationName} stopping all stations");
            }
            index++;
        }

        var compoundDescription = "This train " + string.Join(" then ", clauses);

        return compoundDescription;
    }

    /// <summary>
    /// Loop through all segments, mark if the previous/next segment of the train journey is contiguous (stops all stations) or express
    /// </summary>
    /// <param name="orderedSegments"></param>
    private void SetContiguousStatus(List<Segment> orderedSegments)
    {
        for (var i = 0; i < orderedSegments.Count; i++)
        {
            if (i > 0 && orderedSegments[i - 1].IsContiguous)
            {
                orderedSegments[i].IsPreviousContiguous = true;
            }

            if (i > 0 && orderedSegments[i - 1].Express)
            {
                orderedSegments[i].IsPreviousExpress = true;
            }

            var nextSegment = i + 1;
            if (nextSegment >= orderedSegments.Count) continue;

            if (orderedSegments[nextSegment].IsContiguous)
            {
                orderedSegments[i].IsNextContiguous = true;
            }

            if (orderedSegments[nextSegment].Express)
            {
                orderedSegments[i].IsNextExpress = true;
            }
        }
    }
}
