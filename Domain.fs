module SharpPoint.Domain

type SlideContent = Header of string
type Slide = SlideContent list

type Deck = { Title: string; Slides: Slide list }
