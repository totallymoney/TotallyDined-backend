module TypeExtensions

let (|>?) r f = Option.map f r

let tee f x = f x x

module Tuple =
    let first (a: 'a, _) = a

module List =
    let split length (xs: list<'T>) =
        let rec loop xs =
            [ yield List.truncate length xs
              match List.length xs <= length with
              | false -> yield! loop (List.skip length xs)
              | true -> () ]

        loop xs

    let groupByFirst (l: ('a * 'b) list) =
        l
        |> List.groupBy Tuple.first
        |> List.map (fun (k, v) -> (k, v |> List.map (fun (_, b) -> b)))

    let intersect (l1: 'a list) =
        Set.ofList
        >> Set.intersect (Set.ofList l1)
        >> List.ofSeq

module Result =
    let map = Result.map
    let bind = Result.bind

    let teeOk f = Result.map (tee f)

    let sequence (l: Result<'a, 'b> list) =
        let folder acc x =
            acc
            |> Result.bind (fun h -> x |> Result.bind (fun t -> h :: t |> Ok))

        Ok [] |> List.foldBack folder l

    let apply f xResult =
        match f, xResult with
        | Ok f, Ok x -> Ok(f x)
        | Error errs, _ -> Error errs
        | _, Error errs -> Error errs

    let lift2 f x1 x2 =
        let (<!>) = map
        let (<*>) = apply
        f <!> x1 <*> x2

    let bind2 f x1 x2 = lift2 f x1 x2 |> bind id

    let teeError f =
        function
        | Error e -> f e |> ignore
        | _ -> ()

    type ResultsBuilder() =
        member __.Bind(a, f) = Result.bind f a
        member __.Return(a) = Ok a
        member __.ReturnFrom(a) = a
        member __.Zero(a: Result<_, _>) = a

let result = Result.ResultsBuilder()

let measure f =
    let s = System.Diagnostics.Stopwatch()
    s.Start()
    f () |> (fun x -> (x, s.ElapsedMilliseconds))

let asObj x = x :> obj

let inline (||>>) (f: 'a -> ('b * 'c)) (f': 'b -> 'c -> 'd) x = f x ||> f'

let inline (>>=) f f' = Result.bind f' f

module Seq =
    let toDictionary xs =
        xs
        |> dict
        |> (fun d -> System.Collections.Generic.Dictionary(d))
