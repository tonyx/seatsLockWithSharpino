
namespace seatsLockWithSharpino
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Row2

// I call it context but it works as an aggregate. Need to fix it in library, docs ...
module Row2Context =
    open System
    type Row2 =
        { Row2Seats: Seats.Seat list }
        static member Zero =
            { Row2Seats = row2Seats }

        static member StorageName =
            "_row2"
        static member Version =
            "_01"
        static member SnapshotsInterval =
            15
        static member Lock =
            new Object()

        member this.IsAvailable (seatId: Seats.Id) =
            this.Row2Seats
            |> List.filter (fun seat -> seat.id = seatId)
            |> List.exists (fun seat -> seat.State = Seats.SeatState.Available) 
        member this.ReserveSeats (booking: Seats.Booking) =
            result {
                let seats = this.Row2Seats
                let! check = 
                    let seatsInvolved =
                        seats
                        |> List.filter (fun seat -> booking.seats |> List.contains seat.id)
                    seatsInvolved
                        |> List.forall (fun seat -> seat.State = Seats.SeatState.Available)
                        |> boolToResult "Seat already booked"
                    
                let reservedSeats = 
                    seats
                    |> List.filter (fun seat -> booking.seats |> List.contains seat.id)
                    |> List.map (fun seat -> { seat with State = Seats.SeatState.Reserved })

                let freeSeats = 
                    seats
                    |> List.filter (fun seat -> not (booking.seats |> List.contains seat.id))
                    |> List.map (fun seat -> { seat with State = Seats.SeatState.Available })
                return 
                    {
                        this with
                            Row2Seats = reservedSeats @ freeSeats |> List.sort
                    }
            }

        member this.GetAvailableSeats () =
            this.Row2Seats
            |> List.filter (fun seat -> seat.State = Seats.SeatState.Available)
            |> List.map (fun seat -> seat.id)
        member this.Serialize(serializer: ISerializer) =
            this
            |> serializer.Serialize
        static member Deserialize (serializer: ISerializer, json: string)=
            serializer.Deserialize<Row2> json