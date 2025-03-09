using System.Collections.Generic;
using System.Linq;

namespace StationStops;

public class Segment(
    List<Station> stations,
    bool express,
    bool hasIntermediateStops,
    bool isContiguous)
{
    public List<Station> Stations { get; } = stations;
    public List<Station> StoppingStations { get; } = stations.Where(s => s.StationStop).ToList();
    public bool Express { get; } = express;
    public bool HasIntermediateStops { get; } = hasIntermediateStops;
    public bool IsContiguous { get; } = isContiguous;
    public bool IsPreviousContiguous { get; set; }
    public bool IsNextContiguous { get; set; }
    public bool IsPreviousExpress { get; set; }
    public bool IsNextExpress { get; set; }
    public int Order => this.Stations?.FirstOrDefault()?.Index ?? 0;
}
