module ZopaTest.Calculations

open ZopaTest.Offers

// accumulator type to be passed through scan method when calculating the accumulated offer
type OfferAccumulator =
    { Requested     : int
      Exposure      : int option
      Rate          : decimal
      Available     : int  }
    static member Default =
        {   Requested = 0
            Exposure = None
            Rate = 0.0M
            Available = 0}

let getAmountFromCurrentLender (offerAccumulator:OfferAccumulator) (row:Offers.Row) =
    // get what is available from the current lender based on exposure
    let currentLenderAvailable =
        match offerAccumulator.Exposure with
        | Some ex -> min ex row.Available
        | None -> row.Available
    // if we are still missing on accumulated total more than current lender can offer, take all his offer
    if offerAccumulator.Available + currentLenderAvailable <= offerAccumulator.Requested then
        currentLenderAvailable
    // otherwise take just what we are missing to total
    else
        offerAccumulator.Requested - offerAccumulator.Available

let findAccumulativeRateFromCurrentLender (offerAccumulator:OfferAccumulator) (row:Offers.Row) =
    // get max possible amount from the current lender
    let amountFromCurrentLender = getAmountFromCurrentLender offerAccumulator row
    // calculate partial percentage of total amount requested from the current lender
    let percentOfTotalFromCurrentLender = (decimal)amountFromCurrentLender / (decimal)offerAccumulator.Requested * 100.0M
    // calculate partial percentage of rate from current lender
    let rateFromCurrentLender = row.Rate * (decimal)percentOfTotalFromCurrentLender / 100.0M
    // update and return the totals
    {   Requested = offerAccumulator.Requested 
        Exposure = offerAccumulator.Exposure
        Rate = offerAccumulator.Rate + rateFromCurrentLender
        Available = offerAccumulator.Available + amountFromCurrentLender }

let getOffersSum (offers:Offers) exposure =
    // calculate the sum of offers from lenders based on exposure
    match exposure with
    | Some ex -> offers.Rows |> Seq.map(fun o -> min o.Available ex) |> Seq.sum
    | None _ -> offers.Rows |> Seq.sumBy(fun o -> o.Available)

let calculateTheBestRateBasedOnOffersWithExposure (offers:Offers) requestedAmount exposure =
    // calculate the sum of offers
    let offersSum = getOffersSum offers exposure

    // check if the sum is enough to match the amount requested
    match offersSum >= requestedAmount with
    | false ->  None
    | true ->   let finalAccumulatedRate = 
                    offers.Rows 
                    // sort by best lenders in terms of rate
                    |> Seq.sortBy(fun o -> o.Rate)
                    // find the rate accumulatively per lender
                    |> Seq.scan(findAccumulativeRateFromCurrentLender) { OfferAccumulator.Default with 
                                                                                    Requested = requestedAmount 
                                                                                    Exposure = exposure }
                    // iterate sequence only if there is still work to do (we have not accumulated the sum matching the requested amount)
                    |> Seq.takeWhile(fun oa -> oa.Available <= requestedAmount)
                    // take the last element from the sequence - the final accumulated offer
                    |> Seq.last
                Some <| finalAccumulatedRate.Rate

// specialised version with exposure ignored "baked in" for backwards compatibility
let calculateTheBestRateBasedOnOffers offers requestedAmount = calculateTheBestRateBasedOnOffersWithExposure offers requestedAmount None

// specialised version with standard (£200) exposure "baked in"
let calculateTheBestRateBasedOnOffersWithStandardExposure offers requestedAmount = calculateTheBestRateBasedOnOffersWithExposure offers requestedAmount (Some 200)

// formula taken from https://www.thebalance.com/loan-payment-calculations-315564
let calculateRepayments (A:int) (R:decimal) (ppy:int) (y:int) =
    let n = ppy * y
    let i = R/(decimal)ppy
    let D = ((pown (1.0M + i) n)- 1.0M) / (i*(pown (1.0M + i) n))
    let P = (decimal)A / D
    P, P * (decimal)ppy * (decimal)y

// specialised version for 36 month loans with payments per year and years "baked in"
let calculaterRepaymentsFor36MonthLoan A R = calculateRepayments A R 12 3