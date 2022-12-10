module SharpPoint.DSL

open SharpPoint.Domain

(* --- Slide --- *)
type SlideBuilder() =
    member inline _.Yield(()) = ()

    [<CustomOperation("header")>]
    member inline _.Header((), header: string) : Slide = { Header = header }

let slide = SlideBuilder()

[<RequireQualifiedAccess>]
type DeckProperty =
    | Title of string
    | Slide of Slide

let firstDefined str1 str2 =
    if System.String.IsNullOrEmpty str1 then
        str2
    else
        str1

(* --- Deck --- *)
type DeckBuilder() =
    member inline _.Yield(()) = ()
    member inline _.Yield(slide: Slide) = DeckProperty.Slide slide

    member inline _.Delay(f: unit -> DeckProperty list) = f ()
    member inline _.Delay(f: unit -> DeckProperty) = [ f () ]

    member inline _.Combine(newProp: DeckProperty, previousProps: DeckProperty list) = previousProps @ [ newProp ]

    member inline _.For(prop: DeckProperty, f: unit -> DeckProperty list) = [ prop ] @ f ()

    member inline _.Run(prop) =
        match prop with
        | DeckProperty.Title title -> { Title = title; Slides = [] }
        | DeckProperty.Slide slide -> { Title = ""; Slides = [ slide ] }

    member inline x.Run(props: DeckProperty list) =
        props
        |> List.fold
            (fun acc prop ->
                let itemDeck = x.Run(prop)

                { Title = firstDefined acc.Title itemDeck.Title
                  Slides = acc.Slides @ itemDeck.Slides })
            { Title = ""; Slides = [] }

    [<CustomOperation("title")>]
    member inline _.Title((), title: string) = DeckProperty.Title title

let deck = DeckBuilder()
