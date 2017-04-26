`dotnet restore && dotnet build`

run test suite with `dotnet test`

Model a timeslot as a tuple of two TimeSpan where the first is when the slot opens and the second how long the slot stays open. Take the difference, union and intersection of a single or a list of timeslots.

ex


```
#!c#

var first = new []  { (Minutes(60), Minutes(20)), (Minutes(90), Minutes(20))};
var second = new [] { (Minutes(70), Minutes(20))};

Difference(first, second) =>   { (Minutes(60), Minutes(10)), (Minutes(100), Minutes(10))}
Union(first, second) =>        { (Minutes(60), Minutes(50))                             }
Intersection(first, second) => { (Minutes(70), Minutes(10)), (Minutes(90), Minutes(10)) }
```