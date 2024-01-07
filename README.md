# Example of seat bookings in event sourcing
Booking seats among multiple rows (where those rows are aggregates) in an event-sourcing way.

## Description

I have two rows of seats related to two different streams of events. Each row has 5 seats. I want to book a seat in row 1 and a seat in row 2. I want to do this in a single transaction so that if just one of the claimed seats is already booked then the entire multiple-row transaction fails and no seats are booked at all.
### Questions: 
1) can you do this in a transactional way?
Answer: yes because in-memory and Postgres event-store implementation as single sources of truth are transactional. The runTwoCommands in the Command Handler is transactional.
2) Can it handle more rows?
up to tre. (runThreeCommands in the Command Handler)
3) Is feasible to scale to thousands of seats/hundreds of rows (even though we know that few rows will be actually involved in a single booking operation)?
Not yet.
3) Is Apache Kafka integration included in this example?
No.
4) Is EventStoreDb integration included in this example?
Not yet (it will show the "undo" feature of commands to do rollback commands on multiple streams of events).


## Installation

Just clone the project and run the tests (dotnet test)

## More info:
[Sharpino (event sourcing library used in this example)](https://github.com/tonyx/Sharpino)


