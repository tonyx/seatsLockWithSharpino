namespace seatsLockWithSharpino 
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Sharpino.Core
open Sharpino.Definitions
open Sharpino.Utils
open seatsLockWithSharpino.Row1Events
open Row2

module Row2Command =
    type Row2Command =
        | ReserveSeats of Seats.Booking
            interface Command<Row2Context.Row2, Row2Events.Row2Events > with
                member this.Execute (x: Row2Context.Row2) =
                    match this with
                    | ReserveSeats booking ->
                        x.ReserveSeats booking
                        |> Result.map (fun ctx -> [ Row2Events.SeatsReserved booking ])
                member this.Undoer = None