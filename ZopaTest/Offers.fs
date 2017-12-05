﻿module ZopaTest.Offers

open FSharp.Data

let [<Literal>] marketCsv = """
  Lender,Rate,Available
Bob,0.075,640
Jane,0.069,480
Fred,0.071,520
Mary,0.104,170
John,0.081,320
Dave,0.074,140
Angela,0.071,60 """

type Offers = CsvProvider<marketCsv>

