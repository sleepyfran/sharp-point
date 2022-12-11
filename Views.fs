module SharpPoint.Views

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
open Avalonia.Themes.Fluent
open SharpPoint.Domain

(* --- Global state --- *)
type private CurrentSlide =
    | Initial
    | Slide of index: int

type private GlobalState = { CurrentSlide: IWritable<CurrentSlide> }
let private state = { CurrentSlide = new State<CurrentSlide>(Initial) }

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
                                    TextBlock.text text
                                ] :> IView
                            | Image url ->
                                TextBlock.create [
                                    TextBlock.text $"Loading {url}"
                                ]
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
