using System;

namespace StationStops;

public class Station(string stationName, bool stationStop, int originalIndex) : IEquatable<Station>
{
    public string StationName { get; init; } = stationName;
    public bool StationStop { get; init; } = stationStop;
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
        return HashCode.Combine(StationName, StationStop);
    }
}