# Train announcer console application
Create the train announcer script from a list of train station stops.

## How to run
Open the solution file `StationStops\StationStops.sln` in Visual Studio (2022) on a Windows OS. Run the project by pressing F5.
Or
Compile the code in Visual Studio (2022) in Release mode and launch the executable StationStops.exe found in the `bin` directory

Once the program has started it will prompt the user for a path to a text file (with a .txt file extension) with your list of stations. E.g. "C:\train-announcer\station-stop\stations.txt"

The text file should contain the station name and a flag if the station is express, or a stop. The station name and flag are separated by a comma. e.g.
Central, True 
Roma St, False
South Brisbane, False 
South Bank, True 
Park Road, False 
Buranda, True

See example text files in the root directly of this project.

## Unit tests
There is a unit test project included in the solution. Prefer Resharper Unit Test runner from within Visual Studio (2022) to run the tests. Alternatively use Visual Studio test runner or the dotnet cli.
