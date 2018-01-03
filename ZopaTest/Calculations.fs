module ZopaTest.Calculations

open ZopaTest.Offers

// accumulator type to be passed through scan method when calculating the accumulated offer
type OfferAccumulator =
    { Requested     : int
      Rate          : decimal
      Available     : int  }
    static member Default =
        {   Requested = 0
            Rate = 0.0M
            Available = 0}

let findAccumulativeRateFromCurrentLender (offerAccumulator:OfferAccumulator) (row:Offers.Row) =
    let amountFromCurrentLender = 
        // if we are still missing on accumulated total more than current lender can offer, take all his offer
        if offerAccumulator.Available + row.Available <= offerAccumulator.Requested then
            row.Available
        // otherwise take just what we are missing to total
        else 
            offerAccumulator.Requested - offerAccumulator.Available
    // calculate partial percentage of total amount requested from the current lender
    let percentOfTotalFromCurrentLender = (decimal)amountFromCurrentLender / (decimal)offerAccumulator.Requested * 100.0M
    // calculate partial percentage of rate from current lender
    let rateFromCurrentLender = row.Rate * (decimal)percentOfTotalFromCurrentLender / 100.0M
    // update and return the totals
    {   Requested = offerAccumulator.Requested 
        Rate = offerAccumulator.Rate + rateFromCurrentLender
        Available = offerAccumulator.Available + amountFromCurrentLender }

let calculateTheBestRateBasedOnOffers (offers:Offers) requestedAmount =
    // calculate the sum of offers
    let offersSum = offers.Rows |> Seq.sumBy(fun o -> o.Available)

    // check if the sum is enough to match the amount requested
    match offersSum >= requestedAmount with
    | false ->  None
    | true ->   let finalAccumulatedRate = 
                    offers.Rows 
                    // sort by best lenders in terms of rate
                    |> Seq.sortBy(fun o -> o.Rate)
                    // find the rate accumulatively per lender
                    |> Seq.scan(findAccumulativeRateFromCurrentLender) { OfferAccumulator.Default with Requested = requestedAmount }
                    // iterate sequence only if there is still work to do (we have not accumulated the sum matching the requested amount)
                    |> Seq.takeWhile(fun oa -> oa.Available <= requestedAmount)
                    // take the last element from the sequence - the final accumulated offer
                    |> Seq.last
                Some <| finalAccumulatedRate.Rate

// formula taken from https://www.thebalance.com/loan-payment-calculations-315564
let calculateCompoundRepayments (A:int) (R:decimal) (ppy:int) (y:int) =
    let n = ppy * y
    let i = R/(decimal)ppy
    let D = ((pown (1.0M + i) n)- 1.0M) / (i*(pown (1.0M + i) n))
    let P = (decimal)A / D
    P, P * (decimal)ppy * (decimal)y

// specialised version for 36 month loans with payments per year and years "baked in"
let calculateCompundRepaymentsFor36MonthLoan A R = calculateCompoundRepayments A R 12 3

// based on from https://www.investopedia.com/ask/answers/042315/what-difference-between-compounding-interest-and-simple-interest.asp
let calculateSimpleRepayments (A:int) (R:decimal) (ppy:int) (y:int) =
    let totalRepayment = (decimal)A * R * (decimal)y + (decimal) A
    let monthlyRepayment = totalRepayment / ((decimal)ppy * (decimal)y)
    monthlyRepayment, totalRepayment