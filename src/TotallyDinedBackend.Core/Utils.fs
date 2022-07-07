module Utils

open Types
open System.Text
open System

let deserialize<'a> (body: string) : Result<'a, ErrorType> =
    try
        body |> Json.JsonSerializer.Deserialize |> Ok
    with
    | (ex: Exception) -> Error(JsonSerializerError ex.Message)

let serialize = Json.JsonSerializer.Serialize
