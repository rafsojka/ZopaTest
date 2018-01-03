module ZopaTest.Quote

open ZopaTest.Offers
open ZopaTest.Calculations

type Quote =
    {   RequestedAmount     : int
        Rate                : decimal option
        MonthlyRepayment    : decimal option
        TotalRepayment      : decimal option
    }
    static member Init requestedAmount =
        {   RequestedAmount = requestedAmount
            Rate = None
            MonthlyRepayment = None
            TotalRepayment = None }

let getQuote (offers:Offers) requestedAmount =
    // get possibly the best rate for quote
    let quoteRate = calculateTheBestRateBasedOnOffers offers requestedAmount
    // if it is impossible to get the rate for quote, return the default object
    match quoteRate with
    | None -> Quote.Init requestedAmount
    // if we got the rate calculate the repayments
    | Some qr -> 
        let repayments = calculaterRepaymentsFor36MonthLoan requestedAmount quoteRate.Value
        { Quote.Init requestedAmount with
                    Rate = quoteRate
                    MonthlyRepayment = Some <| fst repayments
                    TotalRepayment = Some <| snd repayments }

let getQuoteFlexible (offers:Offers) requestedAmount paymentsPerYear years repaymentsCalculateFunction =
    // get possibly the best rate for quote
    let quoteRate = calculateTheBestRateBasedOnOffers offers requestedAmount
    // if it is impossible to get the rate for quote, return the default object
    match quoteRate with
    | None -> Quote.Init requestedAmount
    // if we got the rate calculate the repayments
    | Some qr -> 
        let repayments = repaymentsCalculateFunction requestedAmount qr paymentsPerYear years
        { Quote.Init requestedAmount with
                    Rate = quoteRate
                    MonthlyRepayment = Some <| fst repayments
                    TotalRepayment = Some <| snd repayments }

let printQuote (quote:Quote) =
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