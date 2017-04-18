`dotnet restore && dotnet build`

run test suite with `dotnet test`

Model a timeslot as a tuple of two TimeSpan where the first is when the slot opens and the second how long the slot stays open. Manipulate enumerables of timeslots by splitting slots and taking the set difference of one list over the other.