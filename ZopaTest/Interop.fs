module ZopaTest.Interop

open ZopaTest.Offers
open ZopaTest.Quote
open ZopaTest.Validation

// providing C# friendly interface as exposing FSharp.Data.CsvProvider class is not an option 
// https://stackoverflow.com/questions/24274760/f-data-csvprovider-use-from-a-c-sharp-application-interop

let getQuote (marketFile:string) loanAmount =
    validateLoanAmount loanAmount

//    let marketFilePath =
//        match marketFile with
//        | null -> "..\..\market_offers\Market Data for Exercise.csv"
//        | _ -> marketFile

    let currentOffers = Offers.Load(marketFile)

    getQuote currentOffers loanAmount

