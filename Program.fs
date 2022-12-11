module SharpPoint.Program

open SharpPoint.DSL
open SharpPoint.Views

deck {
    title "SharpPoint: Presentations made sharper"

    slide {
        header "This is the first slide"
        text "oh wow!"
        image "https://funnygif.com/thatgif"
    }

    slide {
        header "...Wow, this is the second"
    }

    slide {
        header "NO WAY, a third?!"
    }
}
|> showPresentation
