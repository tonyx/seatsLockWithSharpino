module Tests

open seatsLockWithSharpino.Row1Context
open seatsLockWithSharpino.Seats
open seatsLockWithSharpino
open seatsLockWithSharpino.Row
open seatsLockWithSharpino.Row1
open seatsLockWithSharpino.Row2
open FsToolkit.ErrorHandling
open Expecto
open Sharpino
open Sharpino.MemoryStorage
open seatsLockWithSharpino.App
open Sharpino.Storage
open Sharpino.Cache

[<Tests>]
let tests =
    testList "singleRows tests" [
        testCase "all seats of the first row are free - Ok" <| fun _ ->
            let currentSeats = Row1.Zero
            let availableSeats = currentSeats.GetAvailableSeats()
            Expect.equal availableSeats.Length 5 "should be equal"

        testCase "book a single seat from the first row - Ok" <| fun _ ->
            let booking = { id = 1; seats = [1] }
            let row1WithOneSeatBooked = Row1.Zero.ReserveSeats booking |> Result.get
            let availableSeats = row1WithOneSeatBooked.GetAvailableSeats()
            Expect.equal availableSeats.Length 4 "should be equal"

        testCase "book a single seat from the second row - Ok" <| fun _ ->
            let booking = { id = 2; seats = [6] }
            let row2Context = RowContext(row2Seats)
            let row2WithOneSeatBooked = row2Context.ReserveSeats booking |> Result.get
            let availables = row2WithOneSeatBooked.GetAvailableSeats()
            Expect.equal availables.Length 4 "should be equal"

        testCase "book a seat that is already booked - Error" <| fun _ ->
            let booking = { id = 1; seats = [1] }
            let row1WithOneSeatBooked = Row1.Zero.ReserveSeats booking |> Result.get
            Expect.isFalse (row1WithOneSeatBooked.IsAvailable 1) "should be equal"
            let newBooking = { id = 1; seats = [1] }
            let reservedSeats' = row1WithOneSeatBooked.ReserveSeats newBooking 
            Expect.isError reservedSeats' "should be equal"

        testCase "book five seats - Ok" <| fun _ ->
            let booking = { id = 1; seats = [1;2;3;4;5] }
            let row1FullyBooked = Row1.Zero.ReserveSeats booking |> Result.get
            let availableSeats = row1FullyBooked.GetAvailableSeats()    
            Expect.equal availableSeats.Length 0 "should be equal"
    ]
    |> 
    testSequenced

[<Tests>]
let apiTests =
    testList "test api level (multi-rows) tests" [
        testCase "book seats affecting first and second row - Ok" <| fun _ ->
            // setup
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()
            let app = new App(storage)

            let booking = { id = 1; seats = [3;7] }
            let booked = app.BookSeats booking 
            Expect.isOk booked "should be equal"
            let available = app.GetAllAvailableSeats() |> Result.get
            Expect.equal available.Length 8 "should be equal"

            Expect.equal (available |> Set.ofList) ([1;2;4;5;6;8;9;10] |> Set.ofList) "should be equal"

        testCase "book seats affecting only the first row - Ok" <| fun _ ->
            // setup
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()

            let app = new App(storage)
            let booking1 = { id = 1; seats = [1;2;3;4;5] }
            let booking2 = { id = 2; seats = [] }
            let booked = app.BookSeatsTwoRows booking1 booking2 
            Expect.isOk booked "should be equal"

        testCase "book all seats on row1 - Ok" <| fun _ ->

            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()

            let app = new App(storage)
            let booking1 = { id = 1; seats = [1;2;3;4;5] }
            let booked = app.BookSeatsRow1 booking1 
            Expect.isOk booked "should be equal"
            let availableSeats = app.GetAllAvailableSeats() |> Result.get
            Expect.equal availableSeats.Length 5 "should be equal"
            Expect.equal (availableSeats |> Set.ofList) ([6;7;8;9;10] |> Set.ofList) "should be equal"

        testCase "book all row2 - Ok" <| fun _ ->
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()
            let app = new App(storage)
            let booking2 = { id = 2; seats = [6;7;8;9;10] }
            let booked = app.BookSeatsRow2 booking2 
            Expect.isOk booked "should be equal"
            let availableSeats = app.GetAllAvailableSeats() |> Result.get
            Expect.equal availableSeats.Length 5 "should be equal"
            Expect.equal (availableSeats |> Set.ofList) ([1;2;3;4;5] |> Set.ofList) "should be equal"

        testCase "book only one seat at row2 " <| fun _ ->
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()
            let app = new App(storage)
            let booking2 = { id = 2; seats = [6] }
            let booked = app.BookSeatsRow2 booking2 
            Expect.isOk booked "should be equal"
            let availableSeats = app.GetAllAvailableSeats() |> Result.get
            Expect.equal (availableSeats |> Set.ofList) ([1;2;3;4;5;7;8;9;10] |> Set.ofList) "should be equal"

        testCase "book only one seat at row1 " <| fun _ ->
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()
            let app = new App(storage)
            let booking = { id = 2; seats = [1] }
            let booked = app.BookSeatsRow1 booking 
            Expect.isOk booked "should be equal"
            let availableSeats = app.GetAllAvailableSeats() |> Result.get
            Expect.equal (availableSeats |> Set.ofList) ([2;3;4;5;6;7;8;9;10] |> Set.ofList) "should be equal"

        testCase "book seats partial row2 - Ok" <| fun _ ->
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()
            let app = new App(storage)
            let booking = { id = 2; seats = [6;7] }
            let booked = app.BookSeatsRow2 booking 
            Expect.isOk booked "should be equal"
            let availableSeats = app.GetAllAvailableSeats() |> Result.get
            Expect.equal availableSeats.Length 8 "should be equal"
            Expect.equal (availableSeats |> Set.ofList) ([1;2;3;4;5;8;9;10] |> Set.ofList) "should be equal"

        testCase "reserve places related to the second row - Ok" <| fun _ ->
            let storage = MemoryStorage()
            let app = new App(storage)
            let booking1 = { id = 2; seats = [] }
            let booking2 = { id = 1; seats = [1;2;3;4;5] }
            let booked = app.BookSeatsTwoRows booking1 booking2 
            Expect.isOk booked "should be equal"

        testCase "no bookings, all seats are available" <| fun _ ->
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()

            let app = new App(storage)
            let availableSeats = app.GetAllAvailableSeats() |> Result.get
            Expect.equal availableSeats.Length 10 "should be equal"

        testCase "try book already booked in first row - Error" <| fun _ -> 
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()

            let app = new App(storage)
            let row1FreeSeats = app.GetAllAvailableSeats() |> Result.get
            Expect.equal row1FreeSeats.Length 10 "should be equal"
            let booking1 = { id = 1; seats = [1;2;3;4;5] }
            let booking2 = { id = 2; seats = [] }
            let booked = app.BookSeatsTwoRows booking1 booking2 
            Expect.isOk booked "should be equal"
            let availableSeats = app.GetAllAvailableSeats() |> Result.get

            Expect.isTrue (availableSeats |> List.contains 6) "should be equal" 
            Expect.isFalse (availableSeats |> List.contains 1) "should be equal" 
            let booking3 = { id = 3; seats = [1]}
            let booking4 = { id = 4; seats = []}
            let booked2 = app.BookSeatsTwoRows booking1 booking2 
            Expect.isError booked2 "should be equal"

        testCase "reserve places related to already booked only in the second row and so no place is booked at all - Error" <| fun _ ->
            let storage = MemoryStorage()
            StateCache<Row1>.Instance.Clear()
            StateCache<Row2Context.Row2>.Instance.Clear()

            let app = new App(storage)
            let booking1 = { id = 1; seats = [1;2;3;4;5] }
            let booking2 = { id = 2; seats = [] }
            let booked = app.BookSeatsTwoRows booking1 booking2 
            Expect.isOk booked "should be equal"

            let newBooking1stRow = { id = 4; seats = [1]}
            let newBooking2ndRow = { id = 3; seats = [6;7;8;9;10]}
            let newBooking = app.BookSeatsTwoRows newBooking1stRow newBooking2ndRow
            Expect.isError newBooking "should be equal"

            let newBooking1stRowAgain = { id = 5; seats = []}
            let newBooking2ndRow = { id = 6; seats = [6;7;8;9;10]}
            let newBooking2 = app.BookSeatsTwoRows newBooking1stRowAgain newBooking2ndRow
            Expect.isOk newBooking2 "should be equal"
    ] 
    |> testSequenced
