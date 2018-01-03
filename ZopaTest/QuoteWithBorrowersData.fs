module ZopaTest.QuoteWithBorrowersData

open ZopaTest.Offers
open ZopaTest.CalculationsWithBorrowersData

type QuoteWithBorrowersData =
    {   RequestedAmount     : int
        Rate                : decimal option
        MonthlyRepayment    : decimal option
        TotalRepayment      : decimal option
        BorrowersData       : BorrowerData list option
    }
    static member Init requestedAmount =
        {   RequestedAmount = requestedAmount
            Rate = None
            MonthlyRepayment = None
            TotalRepayment = None
            BorrowersData = None }

let getQuoteFlexibleWithBorrowersData (offers:Offers) requestedAmount paymentsPerYear years repaymentsCalculateFunction =
    // get possibly the best rate for quote
    let data = calculateTheBestRateBasedOnOffers offers requestedAmount
    // if it is impossible to get the rate for quote, return the default object
    match data with
    | None -> QuoteWithBorrowersData.Init requestedAmount
    // if we got the rate calculate the repayments
    | Some (quoteRate, borrowersData) -> 
        let repayments = repaymentsCalculateFunction requestedAmount quoteRate paymentsPerYear years
        { QuoteWithBorrowersData.Init requestedAmount with
                    Rate = Some <| quoteRate
                    MonthlyRepayment = Some <| fst repayments
                    TotalRepayment = Some <| snd repayments
                    BorrowersData = Some <| borrowersData }

let printQuoteWithBorrowersData (quote:QuoteWithBorrowersData) =
    printfn "Requested amount: £%d" quote.RequestedAmount
    match quote.Rate with
    | None ->
        // If the market does not have sufficient offers from lenders to satisfy the loan
        // then the system should inform the borrower that it is not possible to provide a quote at that time.
        printfn "It is not possible to provide a quote at this time."
    | Some _ ->
        printfn "Rate: %.1f%%" (quote.Rate.Value * 100.0M)
        printfn "Monthly repayment: £%.2f" quote.MonthlyRepayment.Value
        printfn "Total repayment: £%.2f" quote.TotalRepayment.Value
        List.iter (fun bd -> printfn "%s %f %d" bd.Name bd.Rate bd.Amount) quote.BorrowersData.Value 