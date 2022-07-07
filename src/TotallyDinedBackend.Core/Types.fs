module Types

type ErrorType =
    | JsonSerializerError of string
    | DynamoDBPutError of string
    | DynamoDBQueryError of string

type RatingDynamoDbDto = { Name: string; Rating: int }

type Cuisine =
    | Unknown
    | English
    | French // todo: finish

module Cuisine =
    let toString cuisine =
        match cuisine with
        | English -> "English"
        | French -> "French"
        | Unknown -> "Unknown"

    let fromString cuisine =
        match cuisine with
        | "English" -> English
        | "French" -> French
        | "Unknown" -> Unknown
        | _ -> failwith "unrecognised cuisine string"

type DietaryRequirements =
    | Vegan
    | Vegetarian
    | Halal

module DietaryRequirements =
    let fromString value =
        match value with
        | "Vegan" -> Vegan
        | "Vegetarian" -> Vegetarian
        | "Halal" -> Halal
        | _ -> failwith "unrecognised dierary requirements string"

type Restaurant =
    { Name: string
      Cuisine: Cuisine
      DietaryRequirements: DietaryRequirements list
      AverageRating: int
      NumberOfRatings: int }

type Review =
    { Name: string
      Rating: int
      Comment: string }
