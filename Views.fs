module SharpPoint.Views

open System.IO
open System.Net.Http
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Types
open Avalonia.Input
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.Themes.Fluent
open SharpPoint.Domain

(* --- Global state --- *)
type private CurrentSlide =
    | Initial
    | Slide of index: int

type private GlobalState = { CurrentSlide: IWritable<CurrentSlide> }
let private state = { CurrentSlide = new State<CurrentSlide>(Initial) }

(* --- Custom hooks --- *)
type Deferred<'t> =
        | NotStartedYet
        | Pending
        | Resolved of 't
        | Failed of exn

    type IComponentContext with

        member this.useAsync<'signal>(init: Async<'signal>) : IWritable<Deferred<'signal>> =
            let state = this.useState (Deferred.NotStartedYet, true)

            this.useEffect (
                handler = (fun _ ->
                    match state.Current with
                    | Deferred.NotStartedYet ->
                        state.Set Deferred.Pending

                        Async.StartImmediate (
                            async {
                                let! result = Async.Catch init

                                match result with
                                | Choice1Of2 value -> state.Set (Deferred.Resolved value)
                                | Choice2Of2 exn -> state.Set (Deferred.Failed exn)
                            }
                        )

                    | _ ->
                        ()
                ),
                triggers = [ EffectTrigger.AfterInit ]
            )

            state

(* --- Utils --- *)
let loadImage (url: string) =
    async {
        use httpClient = new HttpClient()
        let! bytes =
            url
            |> httpClient.GetByteArrayAsync
            |> Async.AwaitTask

        use stream = new MemoryStream(bytes)
        return new Bitmap(stream)
    }

(* --- Views --- *)
let private initialSlide deck =
    Component.create (
        "initial-slide",
        fun _ ->
            StackPanel.create [
                StackPanel.horizontalAlignment HorizontalAlignment.Center
                StackPanel.verticalAlignment VerticalAlignment.Center
                StackPanel.children [
                    TextBlock.create [
                        TextBlock.fontSize 72
                        TextBlock.fontWeight FontWeight.Bold
                        TextBlock.text deck.Title
                        TextBlock.textWrapping TextWrapping.Wrap
                    ]
                ]
            ]
    ) :> IView

let private image url =
    Component.create (
        $"image-{url}",
        fun ctx ->
            let image =
                loadImage url
                |> ctx.useAsync
                
            match image.Current with
            | Deferred.Resolved bitmap ->
                Image.create [
                    Image.source bitmap
                    Image.maxHeight 300
                ]
            | Deferred.Failed e ->
                TextBlock.create [
                    TextBlock.text $"{e.Message}"
                    TextBlock.foreground Brushes.Red
                ]
            | Deferred.Pending | Deferred.NotStartedYet ->
                ProgressBar.create [
                    ProgressBar.isEnabled true
                    ProgressBar.isIndeterminate true
                ]
    )

let private slide (index: int) slide =
    Component.create(
        $"slide-{index}",
        fun _ ->
            StackPanel.create [
               StackPanel.children [
                   if System.String.IsNullOrEmpty slide.Header |> not then
                       yield TextBlock.create [
                           TextBlock.fontSize 48
                           TextBlock.fontWeight FontWeight.Bold
                           TextBlock.text slide.Header
                       ]
                       
                   yield!
                        slide.Content
                        |> List.map (fun content ->
                            match content with
                            | Text text ->
                                TextBlock.create [
                                    TextBlock.fontSize 24
                                    TextBlock.text text
                                    TextBlock.textWrapping TextWrapping.Wrap
                                ] :> IView
                            | Image url ->
                                image url
                        )
               ]
            ]
    ) :> IView

let root deck =
    Component(fun ctx ->
        let currentSlide = ctx.usePassedRead state.CurrentSlide
        
        match currentSlide.Current with
        | Initial -> initialSlide deck
        | Slide idx ->
            deck.Slides
            |> List.item idx
            |> slide idx
    )

(* --- Entrypoint --- *)
let hasSlideAvailableIn idx slides =
    slides
    |> List.tryItem idx
    |> Option.isSome

type MainWindow(deck: Deck) as this =
    inherit HostWindow()

    do
        base.Title <- "SharpPoint"
        base.MinWidth <- 1280.0
        base.MinHeight <- 720.0
        this.Padding <- Thickness(10, 30, 10, 0)
        this.Content <- root deck
        this.ExtendClientAreaToDecorationsHint <- true

#if DEBUG
        this.AttachDevTools()
#endif

    override this.OnKeyDown event =
        let current = state.CurrentSlide.Current
            
        match current, event.Key with
        | Slide 0, Key.Left ->
            (* Go back to the initial slide *)
            Initial
        | Initial, Key.Right when List.isEmpty deck.Slides |> not ->
            (* Go to the first slide (if any) *)
            Slide 0
        | Slide index, Key.Left when deck.Slides |> hasSlideAvailableIn (index - 1) ->
            (* Go to the previous slide (if any) *)
            index - 1 |> Slide
        | Slide index, Key.Right when deck.Slides |> hasSlideAvailableIn (index + 1) ->
            (* Go to the next slide (if any) *)
            index + 1 |> Slide
        | _ -> current
        |> state.CurrentSlide.Set

type App(deck: Deck) =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentTheme(baseUri = null, Mode = FluentThemeMode.Dark))

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = MainWindow(deck)
            desktopLifetime.MainWindow <- mainWindow
        | _ -> ()

let showPresentation deck =
    AppBuilder
        .Configure(fun _ -> App(deck))
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime([||])
    |> ignore
