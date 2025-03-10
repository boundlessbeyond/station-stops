using System;
using System.Collections.Generic;
using System.Linq;
using StationStops.Validation;

namespace StationStops;
public class TrainStopService
{
    public string GetAnnouncement(List<Station> stations)
    {
        var validationResult = ValidateStationList(stations);
        if (!validationResult.Success)
        {
            return validationResult.ErrorMessage!;
        }

        var lastStopIndex = stations.FindLastIndex(s => s.StationStop);
        if (lastStopIndex > -1)
        {
            lastStopIndex += 1;
            var stationsToIgnore = stations.Count - lastStopIndex;
            if (stationsToIgnore > 0)
            {
                stations.RemoveRange(lastStopIndex, stationsToIgnore);
            }
        }

        var result = CheckSimpleCase(stations);
        if (result != null)
        {
            return result;
        }

        List<Segment> journey = new();
        while (stations.Any())
        {
            var journeySegment = this.CalculateStops(stations);
            if (journeySegment == null) continue;

            journey.Add(journeySegment);
            var processedStation = journeySegment.Stations.ToList();
            foreach (var station in processedStation)
            {
                stations.Remove(station);
            }
        }

        return this.ProcessSegments(journey);
    }

    /// <summary>
    /// Takes the list of stations in the journey and calculate where the train stops for the output string
    /// </summary>
    /// <param name="stations">List of Stations to calculate the announcers script from</param>
    /// <returns>Train journey segment</returns>
    private Segment? CalculateStops(List<Station> stations)
    {
        var validationResult = ValidateStationList(stations);
        if (!validationResult.Success)
        {
            return null;
        }

        var contiguousSegment = this.GetContiguousSegmentFromList(stations);
        if (contiguousSegment != null)
        {
            return contiguousSegment;
        }

        var expressSegmentsWithStops = this.GetExpressWithStopsSegment(stations);
        if (expressSegmentsWithStops != null)
        {
            return expressSegmentsWithStops;
        }

        var pureExpressSegment = this.GetPureExpressSegment(stations);
        return pureExpressSegment;
    }

    /// <summary>
    /// Create a segment of the train journey where the train runs express from start to end without stopping inbetween
    /// </summary>
    /// <param name="stations">list of stations to check for an express segment</param>
    /// <returns>Express train journey segment</returns>
    private Segment? GetPureExpressSegment(List<Station> stations)
    {
        var validationResult = ValidateStationList(stations);
        if (!validationResult.Success)
        {
            return null;
        }

        var lastStop = stations.Last(s => s.StationStop);
        var firstStop = stations.First(s => s.StationStop);

        var inbetweenStops = stations.Skip(1) // skip the first station
            .Take(stations.Count - stations.IndexOf(lastStop))
            .Where(s => s.StationStop);

        if (!inbetweenStops.Any())
        {
            var expressStations = stations.Skip(stations.IndexOf(firstStop)).Take(stations.IndexOf(lastStop) + 1).ToList();

            return new Segment(expressStations, true, false);
        }

        return null;
    }

    /// <summary>
    /// Create a segment of the train journey where there is an express section with a stop in the middle
    /// </summary>
    /// <param name="stations">list of stations to check for an express with a stop segment</param>
    /// <returns>Express with stop train journey segment</returns>
    private Segment? GetExpressWithStopsSegment(List<Station> stations)
    {
        var validationResult = ValidateStationList(stations);
        if (!validationResult.Success)
        {
            return null;
        }

        var lastServedIndex = stations.FindLastIndex(s => s.StationStop);

        var truncatedStations = stations.Take(lastServedIndex + 1).ToList();
        if (!truncatedStations.Any()) return null;

        var servedStopsEffective = truncatedStations.Where(s => s.StationStop).ToList();

        var i = 0;

        while (i < servedStopsEffective.Count)
        {
            // Look ahead to see if we can form an express segment.
            // We require three consecutive served stops.
            if (i <= servedStopsEffective.Count - 3)
            {
                var firstStop = servedStopsEffective[i];
                var lastStop = servedStopsEffective[i + 2];

                // If the gap from first to last stops is at least 4 (i.e. several stations between them),
                // we assume an express section.
                if (lastStop.Index - firstStop.Index >= 4)
                {
                    var expressStations = stations.Skip(firstStop.Index).Take(lastStop.Index + 1).ToList();
                    return new Segment(expressStations, true, true);
                }
            }
            i++;
        }

        return null;
    }

    /// <summary>
    /// Converts the contiguous list of stations in to a train journey segment
    /// </summary>
    /// <param name="stations">List of stations to find the list of contiguous stopping stations in</param>
    /// <returns>Train journey segment or null if no contiguous station stops are found</returns>
    private Segment? GetContiguousSegmentFromList(List<Station> stations)
    {
        var validationResult = ValidateStationList(stations);
        if (!validationResult.Success)
        {
            return null;
        }

        var contiguousSection = Helpers.FilterAdjacentItems(stations);

        if (contiguousSection.Any())
        {
            return new Segment(contiguousSection, false, false);
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
        if ((segments?.Any() ?? false) == false)
        {
            return "No train journey found to process.";
        }

        var orderedSegments = segments.OrderBy(s => s.Order).ToList();

        List<string> clauses = new();

        foreach (var segment in orderedSegments)
        {
            if (segment is { Express: true, HasIntermediateStops: true })
            {
                clauses.Add($"runs express from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[2].StationName}, stopping only at {segment.StoppingStations[1].StationName}");
            }

            if (segment is { Express: true, HasIntermediateStops: false })
            {
                clauses.Add($"runs express from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[1].StationName}");
            }

            if (segment is { Express: false, HasIntermediateStops: false })
            {
                clauses.Add($"runs from {segment.StoppingStations[0].StationName} to {segment.StoppingStations[^1].StationName} stopping all stations");
            }
        }

        var compoundDescription = "This train " + string.Join(" then ", clauses);

        return compoundDescription;
    }

    /// <summary>
    /// If the train journey only has a few stations with a few stops this will return a simple description
    /// </summary>
    /// <param name="stations">all stations to process</param>
    /// <returns>Simple announcer outcome from a limited list of stations or null if a more complex journey is found</returns>
    private static string? CheckSimpleCase(List<Station> stations)
    {
        var validationResult = ValidateStationList(stations);
        if (!validationResult.Success)
        {
            return validationResult.ErrorMessage;
        }

        // Truncate the list after the last stopping station.
        var lastStopIndex = stations.FindLastIndex(s => s.StationStop);

        var truncatedStations = stations.Take(lastStopIndex + 1).ToList();

        var servedStops = truncatedStations.Where(s => s.StationStop).ToList();
        // Count express stations in the truncated sequence.
        var expressCountOverall = truncatedStations.Count(s => !s.StationStop);

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
    /// Validate the station list to ensure the rules defined in the spec are met
    /// </summary>
    /// <param name="stations">list of stations</param>
    /// <returns>Validation Result with an error message if a rule is broken</returns>
    private static ValidationResult ValidateStationList(List<Station>? stations)
    {
        if ((stations?.Any() ?? false) == false)
        {
            return ValidationResult.Failure("No stations found in supplied list.");
        }

        var firstStation = stations.First();
        if ((firstStation?.StationStop ?? false) == false)
        {
            // TODO: assumption - that the first "station" (rather than "stop") must be a stopping station:
            // spec: "• The first stop in the stop sequence will always be a stopping station."
            return ValidationResult.Failure("The journey must start with a station stop.");
        }

        return stations.Count(s => s.StationStop) switch
        {
            < 1 => ValidationResult.Failure("No station stops found in supplied list."),
            < 2 => ValidationResult.Failure("Please supply more than one station stop."),
            _ => ValidationResult.Successful
        };
    }
}
