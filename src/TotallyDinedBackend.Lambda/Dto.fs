module Dto

[<CLIMutable>]
type Lobby = { name: string }

[<CLIMutable>]
type Response = { message: string }

[<CLIMutable>]
type ResponseItem<'a> = { message: string; item: 'a }

[<CLIMutable>]
type SendMessageRequest = { Lobby: string; Message: string }
