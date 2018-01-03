module ZopaTest.CalculationsWithBorrowersData

open ZopaTest.Offers

type BorrowerData =
    { Name          : string
      Rate          : decimal
      Amount        : int}

// accumulator type to be passed through scan method when calculating the accumulated offer
// ideally would like just to extend OfferAccumulator but that is impossible in F#
// https://fslang.uservoice.com/forums/245727-f-language/suggestions/8107647-extend-with-keyword-support-to-record-definition
type OfferAccumulatorWithBorrowersData =
    { Requested     : int
      Rate          : decimal
      Available     : int
      BorrowersData : BorrowerData list  }
    static member Default =
        {   Requested = 0
            Rate = 0.0M
            Available = 0
            BorrowersData = List.empty}

let findAccumulativeRateFromCurrentLender (offerAccumulator:OfferAccumulatorWithBorrowersData) (row:Offers.Row) =
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
        Available = offerAccumulator.Available + amountFromCurrentLender
        BorrowersData = List.append offerAccumulator.BorrowersData [{Name = row.Lender; Rate = row.Rate; Amount = amountFromCurrentLender}] }

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
                    |> Seq.scan(findAccumulativeRateFromCurrentLender) { OfferAccumulatorWithBorrowersData.Default with Requested = requestedAmount }
                    // iterate sequence only if there is still work to do (we have not accumulated the sum matching the requested amount)
                    |> Seq.takeWhile(fun oa -> oa.Available <= requestedAmount)
                    // take the last element from the sequence - the final accumulated offer
                    |> Seq.last
                Some <| (finalAccumulatedRate.Rate, finalAccumulatedRate.BorrowersData)