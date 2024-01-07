
namespace seatsLockWithSharpino
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Sharpino

module Commons =
    let isAvailable (seatId: Seats.Id) (seats: Seats.Seat list) =
        seats
        |> List.filter (fun seat -> seat.id = seatId)
        |> List.exists (fun seat -> seat.State = Seats.SeatState.Available) 

    let reserveSeats (booking: Seats.Booking) (seats: Seats.Seat list) =
        result {
            let seatsInvolved =
                seats
                |> List.filter (fun seat -> booking.seats |> List.contains seat.id)
            let! check = 
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
                reservedSeats @ freeSeats |> List.sort 
        }
