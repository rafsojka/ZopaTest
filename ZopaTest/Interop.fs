namespace ZopaTest.Interop

module InteropModule =

    open ZopaTest.Offers
    open ZopaTest.Quote
    open ZopaTest.Validation

    // providing C# friendly interface as exposing FSharp.Data.CsvProvider class is not an option 
    // https://stackoverflow.com/questions/24274760/f-data-csvprovider-use-from-a-c-sharp-application-interop

    let getQuote (marketFile:string) loanAmount =
        validateLoanAmount loanAmount

        let currentOffers = Offers.Load(marketFile)

        getQuote currentOffers loanAmount

    let sprintfQuote (quote:Quote) =
        let requestedAmountString = [sprintf "Requested amount: £%d" quote.RequestedAmount]
        let outputList =
            match quote.Rate with
            | None ->
                // If the market does not have sufficient offers from lenders to satisfy the loan
                // then the system should inform the borrower that it is not possible to provide a quote at that time.
                ["It is not possible to provide a quote at this time."] |> List.append requestedAmountString 
            | Some _ ->
                [sprintf "Total repayment: £%.2f" quote.TotalRepayment.Value]
                |> List.append [sprintf "Monthly repayment: £%.2f" quote.MonthlyRepayment.Value]
                |> List.append [sprintf "Rate: %.1f%%" (quote.Rate.Value * 100.0M)]
                |> List.append requestedAmountString
        String.concat "\r\n" outputList
    
    // based on https://fsharpforfunandprofit.com/posts/type-extensions/#optional-extensions
    // does not work from C# - Message = 'ZopaTest.Quote.Quote' does not contain a definition for 'PrintView'
    type ZopaTest.Quote.Quote with 
        member this.PrintView = 
            sprintfQuote this

// based on http://langexplr.blogspot.co.uk/2008/06/using-f-option-types-in-c.html
[<System.Runtime.CompilerServices.Extension>]
module ExtensionsModule =
    
    [<System.Runtime.CompilerServices.Extension>]
    let PrintView(quote : ZopaTest.Quote.Quote) =
        InteropModule.sprintfQuote quote

    [<System.Runtime.CompilerServices.Extension>]
    let Exists(opt : decimal option) =
                match opt with
                  | Some _ -> true
                  | None -> false