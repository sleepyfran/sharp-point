module SharpPoint.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.Themes.Fluent
open SharpPoint.Domain

(* --- Views --- *)
let root deck =
    Component(fun ctx -> TextBlock.create [ TextBlock.text deck.Title ])

(* --- Entrypoint --- *)
type MainWindow(deck: Deck) as this =
    inherit HostWindow()

    do
        base.Title <- "SharpPoint"
        base.MinWidth <- 1280.0
        base.MinHeight <- 720.0
        this.Padding <- Thickness(10, 30, 10, 0)
        this.Content <- root deck
        this.ExtendClientAreaToDecorationsHint <- true

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
