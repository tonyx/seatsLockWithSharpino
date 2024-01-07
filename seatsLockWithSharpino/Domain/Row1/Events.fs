
namespace seatsLockWithSharpino 
open FsToolkit.ErrorHandling
open Sharpino.Utils
open Sharpino.Core
open Sharpino.Definitions
open Sharpino.Utils
open Row1

module Row1Events =
    type Row1Events =
        | SeatsReserved of Seats.Booking
            interface Event<Row1Context.Row1> with
                member this.Process (x: Row1Context.Row1) =
                    match this with
                    | SeatsReserved booking ->
                        x.ReserveSeats booking

        member this.Serialize(serializer: ISerializer) =
            this
            |> serializer.Serialize

        static member Deserialize (serializer: ISerializer, json: Json) =
            serializer.Deserialize<Row1Events> json




