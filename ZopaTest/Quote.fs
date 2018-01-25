module ZopaTest.Quote

open ZopaTest.Offers
open ZopaTest.Calculations

type Subquote =
    {   Rate                : decimal
        MonthlyRepayment    : decimal
        TotalRepayment      : decimal
    }
    static member Init rate monthlyRepayment totalRepayment =
        {   Rate = rate
            MonthlyRepayment = monthlyRepayment
            TotalRepayment = totalRepayment }

type Quote =
    {   RequestedAmount     : int
        Subquote            : Subquote option
    }
    static member Init requestedAmount =
        {   RequestedAmount = requestedAmount
            Subquote = None }

let getQuote' requestedAmount quoteRate =
     // if it is impossible to get the rate for quote, return the default object
    match quoteRate with
    | None -> Quote.Init requestedAmount
    // if we got the rate calculate the repayments
    | Some qr -> 
        let repayments = calculaterRepaymentsFor36MonthLoan requestedAmount qr
        { Quote.Init requestedAmount with
                    Subquote = Some <| Subquote.Init qr (fst repayments) (snd repayments) }

let getQuote (offers:Offers) requestedAmount =
    // get possibly the best rate for quote
    let quoteRate = calculateTheBestRateBasedOnOffers offers requestedAmount
    // get quote based on best rate
    getQuote' requestedAmount quoteRate

let getQuoteWithExposure (offers:Offers) requestedAmount exposure =
    // get possibly the best rate for quote
    let quoteRate = calculateTheBestRateBasedOnOffersWithExposure offers requestedAmount exposure
    // get quote based on best rate
    getQuote' requestedAmount quoteRate

let printQuote (quote:Quote) =
    printfn "Requested amount: £%d" quote.RequestedAmount
    match quote.Subquote with
    | None ->
        // If the market does not have sufficient offers from lenders to satisfy the loan
        // then the system should inform the borrower that it is not possible to provide a quote at that time.
        printfn "It is not possible to provide a quote at this time."
    | Some sq ->
        printfn "Rate: %.1f%%" (sq.Rate * 100.0M)
        printfn "Monthly repayment: £%.2f" sq.MonthlyRepayment
        printfn "Total repayment: £%.2f" sq.TotalRepayment