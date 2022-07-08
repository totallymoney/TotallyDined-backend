module Dto

open Types

type RestaurantResponseDto =
    { Name: string
      Cuisine: string
      DietaryRequirements: string list
      Address: string
      PriceRange: int
      AverageRating: int
      NumberOfRatings: int }

[<CLIMutable>]
type Response = { message: string }

[<CLIMutable>]
type CreateRestaurantRequest =
    { Name: string
      Cuisine: string
      DietaryRequirements: string list
      PriceRange: int
      Address: string }

let toResponseDto (restaurant: Restaurant) : RestaurantResponseDto =
    { Name = restaurant.Name
      Address = restaurant.Address
      PriceRange = restaurant.PriceRange
      Cuisine = restaurant.Cuisine |> Cuisine.toString
      DietaryRequirements =
        restaurant.DietaryRequirements
        |> List.map DietaryRequirements.toString
      AverageRating = restaurant.AverageRating
      NumberOfRatings = restaurant.NumberOfRatings }
