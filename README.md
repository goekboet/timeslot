# Timeslot
To build the project: 

`dotnet restore && dotnet build`

To run the testsuite:

 `dotnet test`

## Description

Model a timeslot as a tuple of two TimeSpan where the first is when the slot opens and the second how long the slot stays open. Take the difference, union and intersection of a single or a list of timeslots.

These functions will assume that the timeslots are sorted in descending order with regards to the open (first item in the tuple) timespan

## Example:


```
// Minutes(x) is defined as TimeSpan.FromMinutes(x)
var first = new[] { (Minutes(60), Minutes(20)), (Minutes(90Minutes(20)) };
var second = new[] { (Minutes(70), Minutes(30)) 

// Show is a function (TimeSpan o, TimeSpan d) -> string 
var diff = Difference(first, second).Select(Show);
var union = Union(first, second).Select(Show);
var intersect = Intersection(first, second).Select(Show);

Console.WriteLine("Difference:");
foreach (var slot in diff) Console.WriteLine(slot);

Console.WriteLine("Union:");
foreach (var slot in union) Console.WriteLine(slot);

Console.WriteLine("Intersect:");
foreach (var slot in intersect) Console.WriteLine(slot);

// Will print:
// Difference:
// o: 01:00:00 d: 00:10:00
// o: 01:40:00 d: 00:10:00
// Union:
// o: 01:00:00 d: 00:50:00
// Intersect:
// o: 01:10:00 d: 00:10:00
// o: 01:30:00 d: 00:10:00

```