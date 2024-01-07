namespace seatsLockWithSharpino
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Sharpino
open Row1

// I call it context but it works as an aggregate. Need to fix it in library, docs ...
module Row1Context =
    let serializer = new Utils.JsonSerializer(Utils.serSettings) :> Utils.ISerializer
    open System
    type Row1 =
        { Row1Seats: Seats.Seat list }
        static member Zero =
            { Row1Seats = row1Seats }

        static member StorageName =
            "_row1"
        static member Version =
            "_01"
        static member SnapshotsInterval =
            15
        static member Lock =
            new Object()

        member this.IsAvailable (seatId: Seats.Id) =
            this.Row1Seats
            |> List.filter (fun seat -> seat.id = seatId)
            |> List.exists (fun seat -> seat.State = Seats.SeatState.Available) 
        member this.ReserveSeats (booking: Seats.Booking) =
            result {
                let seats = this.Row1Seats
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
                            Row1Seats = reservedSeats @ freeSeats |> List.sort 
                    }
            }

        member this.GetAvailableSeats () =
            this.Row1Seats
            |> List.filter (fun seat -> seat.State = Seats.SeatState.Available)
            |> List.map (fun seat -> seat.id)

        member this.Serialize(serializer: ISerializer) =
            this
            |> serializer.Serialize
        static member Deserialize (serializer: ISerializer, json: string)=
            serializer.Deserialize<Row1> json