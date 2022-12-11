module SharpPoint.Domain

type SlideContent =
    | Text of text: string
    | Image of url: string

type Slide = { Header: string; Content: SlideContent list }

type Deck = { Title: string; Slides: Slide list }
