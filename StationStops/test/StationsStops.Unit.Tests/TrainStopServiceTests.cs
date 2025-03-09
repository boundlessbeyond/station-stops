using FluentAssertions;
using StationStops;

namespace StationsStops.Unit.Tests;

public class TrainStopServiceTests
{
    [Fact]
    public void TestExpressWithStop()
    {
        // ARRANGE
        var service = new TrainStopService();
        // ACT
        var sut = service.GetAnnouncer(DataProvider.ExpressStationsWithStop());

        // ASSERT
        sut.Should().Be("This train runs express from Central to Buranda, stopping only at South Bank");
    }

    [Fact]
    public void TestExpress()
    {
        // ARRANGE
        var service = new TrainStopService();
        // ACT
        var sut = service.GetAnnouncer(DataProvider.ExpressStations());

        // ASSERT
        sut.Should().Be("This train runs express from Central to South Bank");
    }

    [Fact]
    public void TestExpressWithStopThenExpress()
    {
        // ARRANGE
        var service = new TrainStopService();
        // ACT
        var sut = service.GetAnnouncer(DataProvider.ExpressStationsThenExpress());

        // ASSERT
        sut.Should().Be("This train runs express from Central to Buranda, stopping only at South Bank then runs express from Coorparoo to Cannon Hill");
    }

    [Fact]
    public void TestOnlyStops()
    {
        // ARRANGE
        var service = new TrainStopService();
        // ACT
        var sut = service.GetAnnouncer(DataProvider.OnlyStopsStations());

        // ASSERT
        sut.Should().Be("This train stops at Central and Roma St only");
    }

    [Fact]
    public void TestExcept()
    {
        // ARRANGE
        var service = new TrainStopService();
        // ACT
        var sut = service.GetAnnouncer(DataProvider.GetExceptStations());

        // ASSERT
        sut.Should().Be("This train stops at all stations except South Brisbane");
    }

    [Fact]
    public void TestContiguousStops()
    {
        // ARRANGE
        var service = new TrainStopService();
        // ACT
        var sut = service.GetAnnouncer(DataProvider.ContiguousStationsStoppingAll());

        // ASSERT
        sut.Should().Be("This train runs from Central to Cannon Hill stopping all stations");
    }

    [Fact]
    public void TestExpressToContiguousStops()
    {
        // ARRANGE
        var service = new TrainStopService();
        // ACT
        var sut = service.GetAnnouncer(DataProvider.ExpressStationsThenContiguous());

        // ASSERT
        sut.Should().Be("This train runs express from Central to Buranda, stopping only at South Bank then runs from Coorparoo to Cannon Hill stopping all stations");
    }
}