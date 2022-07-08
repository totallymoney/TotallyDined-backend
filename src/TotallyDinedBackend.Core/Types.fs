module Types

type ErrorType =
    | JsonSerializerError of string
    | DynamoDBPutError of string
    | DynamoDBQueryError of string

type RatingDynamoDbDto = { Name: string; Rating: int }

type Cuisine =
    | Unknown
    | English
    | French
    | Japanese
    | Chinese
    | Greek
    | Lebanese
    | Moroccan
    | Turkish
    | Spanish
    | Italian
    | Mediterranean
    | Indian
    | African
    | Coffee
    | Mexican
    | American
    | Thai
    | BBQ
    | Desserts
    | BubbleTea
    | Korean

module Cuisine =
    let toString cuisine =
        match cuisine with
        | Unknown -> "Unknown"
        | English -> "English"
        | French -> "French"
        | Japanese -> "Japanese"
        | Chinese -> "Chinese"
        | Greek -> "Greek"
        | Lebanese -> "Lebanese"
        | Moroccan -> "Moroccan"
        | Turkish -> "Turkish"
        | Spanish -> "Spanish"
        | Italian -> "Italian"
        | Mediterranean -> "Mediterranean"
        | Indian -> "Indian"
        | African -> "African"
        | Coffee -> "Coffee"
        | Mexican -> "Mexican"
        | American -> "American"
        | Thai -> "Thai"
        | BBQ -> "BBQ"
        | Desserts -> "Desserts"
        | BubbleTea -> "BubbleTea"
        | Korean -> "Korean"

    let fromString cuisine =
        match cuisine with
        | "Unknown" -> Unknown
        | "English" -> English
        | "French" -> French
        | "Japanese" -> Japanese
        | "Chinese" -> Chinese
        | "Greek" -> Greek
        | "Lebanese" -> Lebanese
        | "Moroccan" -> Moroccan
        | "Turkish" -> Turkish
        | "Spanish" -> Spanish
        | "Italian" -> Italian
        | "Mediterranean" -> Mediterranean
        | "Indian" -> Indian
        | "African" -> African
        | "Coffee" -> Coffee
        | "Mexican" -> Mexican
        | "American" -> American
        | "Thai" -> Thai
        | "BBQ" -> BBQ
        | "Desserts" -> Desserts
        | "BubbleTea" -> BubbleTea
        | "Korean" -> Korean
        | _ -> failwith "unrecognised cuisine string"

type DietaryRequirements =
    | Vegan
    | Vegetarian
    | Halal
    | GlutenFree

module DietaryRequirements =
    let fromString value =
        match value with
        | "Vegan" -> Vegan
        | "Vegetarian" -> Vegetarian
        | "Halal" -> Halal
        | "GlutenFree" -> GlutenFree
        | _ -> failwith "unrecognised dierary requirements string"

    let toString dietaryRequirement =
        match dietaryRequirement with
        | Vegan -> "Vegan"
        | Vegetarian -> "Vegetarian"
        | Halal -> "Halal"
        | GlutenFree -> "GlutenFree"

type Restaurant =
    { Name: string
      Cuisine: Cuisine
      DietaryRequirements: DietaryRequirements list
      Address: string
      PriceRange: int
      AverageRating: int
      NumberOfRatings: int }

type Review =
    { Name: string
      Rating: int
      Comment: string }
