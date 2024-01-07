
namespace seatsLockWithSharpino
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Sharpino

module Row =
    let serializer = new Utils.JsonSerializer(Utils.serSettings) :> Utils.ISerializer

    type RowContext(RowSeats: Seats.Seat list) = 
        member this.RowSeats with get() = RowSeats

        member this.IsAvailable (seatId: Seats.Id) =
            this.RowSeats
            |> List.filter (fun seat -> seat.id = seatId)
            |> List.exists (fun seat -> seat.State = Seats.SeatState.Available) 

        member this.ReserveSeats (booking: Seats.Booking) =
            result {
                let seats = this.RowSeats
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
                    RowContext(reservedSeats @ freeSeats |> List.sort)
            }

        member this.GetAvailableSeats () =
            this.RowSeats
            |> List.filter (fun seat -> seat.State = Seats.SeatState.Available)
            |> List.map (fun seat -> seat.id)

        member this.Serialize(serializer: ISerializer) =
            this
            |> serializer.Serialize