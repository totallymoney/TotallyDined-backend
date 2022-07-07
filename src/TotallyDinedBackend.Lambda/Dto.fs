module Dto

[<CLIMutable>]
type Response = { message: string }

[<CLIMutable>]
type ResponseItem<'a> = { message: string; item: 'a }

[<CLIMutable>]
type CreateRestaurantRequest =
    { Name: string
      Cuisine: string
      DietaryRequirements: string list
      PriceRange: int }
