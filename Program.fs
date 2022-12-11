module SharpPoint.Program

open SharpPoint.DSL
open SharpPoint.Views


deck {
    title "SharpPoint: Presentations made sharper"

    slide {
        header "This is the first slide"
        text "Lorem ipsum or whatever."
        text "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        image "https://i.kym-cdn.com/photos/images/list/000/056/238/brock20110724-22047-utv7m1.jpg"
    }

    slide {
        header "...Wow, this is the second"
        text "I have nothing else to say, so please don't press next"
    }

    slide {
        header "NO WAY, a third?!"
        text "I told you, there's nothing interesting here."
    }
}
|> showPresentation
