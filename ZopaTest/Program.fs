// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open FSharp.Data

open ZopaTest.Offers
open ZopaTest.Calculations
open ZopaTest.Quote
open ZopaTest.Validation

[<EntryPoint>]
let main argv = 
    try
        let currentOffers = Offers.Load(argv.[0])
        let loanAmount = Convert.ToInt32(argv.[1])

        validateLoanAmount loanAmount

        printQuote <| getQuote currentOffers loanAmount
    with
        | :? System.IO.FileNotFoundException as fnfex -> printfn "Market file \"%s\" was not found." argv.[0]
        | :? System.Exception as ex -> printfn "%s" ex.Message

    0 // return an integer exit code
