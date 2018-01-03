// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open FSharp.Data

open ZopaTest.Quote
open ZopaTest.Orchestration

[<EntryPoint>]
let main argv = 
    try
        let offersFilePath = argv.[0]
        let loanAmount = Convert.ToInt32(argv.[1])

        let interestTypeSymbol =
            match argv.Length with
            | len when len >= 3 -> 
                Some <| argv.[2].ToLower()
            | _ -> None

        let years = 
            match argv.Length with
            | len when len >= 4 -> 
                Some <| Convert.ToInt32(argv.[3])
            | _ -> None

        let paymentsPerYear = 
            match argv.Length with
            | len when len >= 5 -> 
                Some <| Convert.ToInt32(argv.[4])
            | _ -> None

        printQuote <| getQuoteOrchestrated offersFilePath loanAmount interestTypeSymbol paymentsPerYear years
    with
        | :? System.IO.FileNotFoundException as fnfex -> printfn "Market file \"%s\" was not found." argv.[0]
        | :? System.Exception as ex -> printfn "%s" ex.Message

    0 // return an integer exit code
