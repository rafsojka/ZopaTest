module ZopaTest.Orchestration

open ZopaTest.Offers
open ZopaTest.Calculations
open ZopaTest.Quote
open ZopaTest.Validation

let getQuoteOrchestrated (offersFile : string) loanAmount paymentsPerYear years =
    validateLoanAmount loanAmount

    let offers = Offers.Load(offersFile)

    let paymentsPerYearVal, yearsVal =
        match (paymentsPerYear, years) with
        | (Some ppy, Some y) -> ppy, y
        | _ -> 12, 3

    getQuoteFlexible offers loanAmount paymentsPerYearVal yearsVal calculateRepayments

