
namespace seatsLockWithSharpino 
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Sharpino.Core
open Sharpino.Definitions
open Sharpino.Utils
open seatsLockWithSharpino.Row1Events
open Row1

module Row1Command =
    type Row1Command =
        | ReserveSeats of Seats.Booking
            interface Command<Row1Context.Row1, Row1Events > with
                member this.Execute (x: Row1Context.Row1) =
                    match this with
                    | ReserveSeats booking ->
                        x.ReserveSeats booking
                        |> Result.map (fun ctx -> [ SeatsReserved booking ])
                member this.Undoer = None