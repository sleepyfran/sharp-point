module SharpPoint.Domain

type Slide = { Header: string }

type Deck = { Title: string; Slides: Slide list }
