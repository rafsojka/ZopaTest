module ZopaTest.Orchestration

open ZopaTest.Offers
open ZopaTest.Calculations
open ZopaTest.Quote
open ZopaTest.Validation

type interestType = Compound | Simple

let getQuoteOrchestrated (offersFile : string) loanAmount interestTypeSymbol paymentsPerYear years =
    
    validateLoanAmount loanAmount

    let offers = Offers.Load(offersFile)

    let interestType =
        match interestTypeSymbol with
        | Some "s" -> Simple
        | _ -> Compound

    let yearsVal =
        match years with
        | Some y -> y
        | _ -> 3

    let paymentsPerYearVal =
        match paymentsPerYear with
        | Some ppy -> ppy
        | _ -> 12

    match interestType with
    | Compound -> getQuoteFlexible offers loanAmount paymentsPerYearVal yearsVal calculateCompoundRepayments
    | Simple -> getQuoteFlexible offers loanAmount paymentsPerYearVal yearsVal calculateSimpleRepayments

    

