namespace seatsLockWithSharpino
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Sharpino
open System
open Row1

// I call it context but it works as an aggregate. Need to fix it in library, docs ...
module Row1Context =
    open Row

    type Row1(rowContext: RowContext) =

        static member Zero =
            Row1(RowContext(row1Seats))

        static member StorageName =
            "_row1"
        static member Version =
            "_01"
        static member SnapshotsInterval =
            15
        static member Lock =
            new Object()

        member this.IsAvailable (seatId: Seats.Id) =
            rowContext.IsAvailable seatId

        member this.ReserveSeats (booking: Seats.Booking) =
            result {
                let! rowContext' = rowContext.ReserveSeats booking
                return Row1(rowContext')
            }

        member this.GetAvailableSeats () =
            rowContext.GetAvailableSeats ()
        member this.Serialize(serializer: ISerializer) =
            this
            |> serializer.Serialize
        static member Deserialize (serializer: ISerializer, json: string)=
            serializer.Deserialize<Row1> json


    // let serializer = new Utils.JsonSerializer(Utils.serSettings) :> Utils.ISerializer
    // type Row1 =
    //     { RowSeats: Seats.Seat list }
    //     static member Zero =
    //         { RowSeats = row1Seats }

    //     static member StorageName =
    //         "_row1"
    //     static member Version =
    //         "_01"
    //     static member SnapshotsInterval =
    //         15
    //     static member Lock =
    //         new Object()

    //     member this.IsAvailable (seatId: Seats.Id) =
    //         Commons.isAvailable seatId this.RowSeats
    //     member this.ReserveSeats (booking: Seats.Booking) =
    //         result {
    //             let seats = this.RowSeats
    //             let! check = 
    //                 let seatsInvolved =
    //                     seats
    //                     |> List.filter (fun seat -> booking.seats |> List.contains seat.id)
    //                 seatsInvolved
    //                     |> List.forall (fun seat -> seat.State = Seats.SeatState.Available)
    //                     |> boolToResult "Seat already booked"
                        
    //             let reservedSeats = 
    //                 seats
    //                 |> List.filter (fun seat -> booking.seats |> List.contains seat.id)
    //                 |> List.map (fun seat -> { seat with State = Seats.SeatState.Reserved })

    //             let freeSeats = 
    //                 seats
    //                 |> List.filter (fun seat -> not (booking.seats |> List.contains seat.id))
    //                 |> List.map (fun seat -> { seat with State = Seats.SeatState.Available })

    //             return 
    //                 {
    //                     this with
    //                         RowSeats = reservedSeats @ freeSeats |> List.sort 
    //                 }
    //         }

    //     member this.GetAvailableSeats () =
    //         this.RowSeats
    //         |> List.filter (fun seat -> seat.State = Seats.SeatState.Available)
    //         |> List.map (fun seat -> seat.id)

    //     member this.Serialize(serializer: ISerializer) =
    //         this
    //         |> serializer.Serialize
    //     static member Deserialize (serializer: ISerializer, json: string)=
    //         serializer.Deserialize<Row1> json