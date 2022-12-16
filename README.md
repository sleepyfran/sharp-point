# SharpPoint

A toy DSL for F# made with Computation Expressions to create presentations, powered by Avalonia. Made as part of the [2022 F# Advent calendar](https://sleepyfran.github.io/blog/posts/fsharp/ce-in-fsharp/).

## Why?

More like why not! I wanted to learn more about Computation Expressions to make DSLs and I liked the complete uselessness but beauty of [DeckUI](https://github.com/joshdholtz/DeckUI).

## How does it look like?

Now that's a better question! SharpPoint allows to turn this:

```fsharp
deck {
    title "SharpPoint: Presentations made sharper"

    slide {
        header "This is the first slide"
        text "Lorem ipsum dolor sit amet..."
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
```

Into this:

![final-app](https://user-images.githubusercontent.com/6024783/208136422-67dfaf8a-cf91-4766-9a70-e12b852dce1e.gif)
