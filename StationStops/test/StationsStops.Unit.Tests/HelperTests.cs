using System.Linq;
using FluentAssertions;
using StationStops;

namespace StationsStops.Unit.Tests;
public class HelperTests
{
    [Fact]
    public void TestAdjacentStops1()
    {
        // ARRANGE
        var data = DataProvider.ContiguousStationsStoppingAll();

        // ACT
        var sut = Helpers.FilterAdjacentItems(data);

        // ASSERT
        sut.Should().BeEquivalentTo(data, because: "Train stops all stations");
    }

    [Fact]
    public void TestAdjacentStops_None()
    {
        // ARRANGE
        var data = DataProvider.ExpressStationsThenExpress();

        // ACT
        var sut = Helpers.FilterAdjacentItems(data);

        // ASSERT
        sut.Should().BeEmpty(because: "First stop is followed by an express station");
    }

    [Fact]
    public void TestAdjacentStops_FirstTwo()
    {
        // ARRANGE
        var data = DataProvider.GetExceptStations();
        var expected = data.Take(2);

        // ACT
        var sut = Helpers.FilterAdjacentItems(data);

        // ASSERT
        sut.Should().HaveCount(2);
        sut.Should().BeEquivalentTo(expected, because: "Because the first two stops are adjacent followed by an express station");
    }
}
