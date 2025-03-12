using FluentAssertions;
using StationStops;

namespace StationsStops.Unit.Tests;

public class TrainAnnouncerTests
{
    [Fact]
    public void TestExpressWithStop()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.ExpressStationsWithStop());

        // ASSERT
        sut.Should().Be("This train runs express from Central to Buranda, stopping only at South Bank");
    }

    [Fact]
    public void TestExpress()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.ExpressStations());

        // ASSERT
        sut.Should().Be("This train runs express from Central to South Bank");
    }

    [Fact]
    public void TestExpressWithStopThenExpress()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.ExpressStationsThenExpress());

        // ASSERT
        sut.Should().Be("This train runs express from Central to Buranda, stopping only at South Bank then runs express from Coorparoo to Cannon Hill");
    }

    [Fact]
    public void TestOnlyStops()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.OnlyStopsStations());

        // ASSERT
        sut.Should().Be("This train stops at Central and Roma St only");
    }

    [Fact]
    public void TestExcept()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.GetExceptStations());

        // ASSERT
        sut.Should().Be("This train stops at all stations except South Brisbane");
    }

    [Fact]
    public void TestContiguousStops()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.ContiguousStationsStoppingAll());

        // ASSERT
        sut.Should().Be("This train runs from Central to Cannon Hill stopping all stations");
    }

    [Fact]
    public void TestExpressToContiguousStops()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.ExpressStationsThenContiguous());

        // ASSERT
        sut.Should().Be("This train runs express from Central to Buranda, stopping only at South Bank then runs from Coorparoo to Cannon Hill stopping all stations");
    }

    [Fact]
    public void TestJourneyWithNoEnd()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.GetJourneyWithoutEnd());

        // ASSERT
        sut.Should().Be("This train runs from Central to South Bank stopping all stations", because: "There is a station after South Bank bet we ignore it as we don't stop there");
    }

    [Fact]
    public void TestJourneyWithNoStart()
    {
        // ARRANGE
        var service = new TrainStopService();

        // ACT
        var sut = service.GetAnnouncement(DataProvider.GetJourneyWithoutStart());

        // ASSERT
        sut.Should().Be("The journey must start with a station stop.", because: "The journey contains no station stops");
    }
}