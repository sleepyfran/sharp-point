module SharpPoint.Program

open SharpPoint.DSL
open SharpPoint.Views

deck {
    title "A test"

    slide { header "Hello world!" }

    slide { header "Wow!" }

    slide { header "Much wow!" }
}
|> showPresentation
