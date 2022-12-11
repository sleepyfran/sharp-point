module SharpPoint.DSL

open SharpPoint.Domain

(* --- Slide --- *)
[<RequireQualifiedAccess>]
type SlideProperty =
    | Header of header: string
    | Content of content: SlideContent

type SlideBuilder() =
    member inline _.Yield(()) = ()
    
    member inline x.Run(props: SlideProperty list) =
        props
        |> List.fold
            (fun slide prop ->
                match prop with
                | SlideProperty.Header header -> { slide with Header = header }
                | SlideProperty.Content content -> { slide with Content = content :: slide.Content })
            { Header = ""; Content = [] }
            
    [<CustomOperation("header")>]
    member inline _.Header((), header: string) = [ SlideProperty.Header header ]

    [<CustomOperation("text")>]
    member inline _.Text(prev: SlideProperty list, text: string) =
        (Text text |> SlideProperty.Content) :: prev
    
    [<CustomOperation("image")>]
    member inline _.Image(prev: SlideProperty list, url: string) =
        (Image url |> SlideProperty.Content) :: prev

let slide = SlideBuilder()

[<RequireQualifiedAccess>]
type DeckProperty =
    | Title of string
    | Slide of Slide

(* --- Deck --- *)
type DeckBuilder() =
    member inline _.Yield(()) = ()
    member inline _.Yield(slide: Slide) = DeckProperty.Slide slide

    member inline _.Delay(f: unit -> DeckProperty list) = f ()
    member inline _.Delay(f: unit -> DeckProperty) = [ f () ]

    member inline _.Combine(newProp: DeckProperty, previousProps: DeckProperty list) =
        newProp :: previousProps
    
    member inline x.For(prop: DeckProperty, f: unit -> DeckProperty list) =
        x.Combine(prop, f())

    member inline x.Run(props: DeckProperty list) =
        props
        |> List.fold
            (fun deck prop ->
                match prop with
                | DeckProperty.Title title -> { deck with Title = title }
                | DeckProperty.Slide slide -> { deck with Slides = deck.Slides @ [slide] })
            { Title = ""; Slides = [] }

    member inline x.Run(prop: DeckProperty) = x.Run([prop])
    
    [<CustomOperation("title")>]
    member inline _.Title((), title: string) = DeckProperty.Title title

let deck = DeckBuilder()
