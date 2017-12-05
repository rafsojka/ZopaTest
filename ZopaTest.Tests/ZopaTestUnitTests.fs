namespace ZopaTest.Tests

open NUnit.Framework
open FsUnit
open FSharp.Data

open ZopaTest.Offers
open ZopaTest.Calculations

[<TestFixture>]
type ZopaTestUnitTests() = 

    [<TestCase( 2400)>]
    [<TestCase( 12400)>]
    member test.``calculateTheBestRateBasedOnOffers returns None`` (loanAmount) =
        let sampleCsv = """
            Lender,Rate,Available
            Bob,0.075,640
            Jane,0.069,480
            Fred,0.071,520
            Mary,0.104,170
            John,0.081,320
            Dave,0.074,140
            Angela,0.071,60 """

        let offers = Offers.Parse(sampleCsv)
        let bestRate = calculateTheBestRateBasedOnOffers offers loanAmount

        bestRate.IsNone             |> should equal true

    // test values calculated using compounded_loan_rate.xlsx
    [<TestCase( "loanAmount within best lender availability",1000, 0.069)>]
    [<TestCase( "loanAmount spread between two best lenders 50/50",2000, 0.07)>]
    [<TestCase( "loanAmount spread between two best lenders 66.(6)/33.(3)",1500, 0.069597)>]
    [<TestCase( "loanAmount spread between two best lenders 33.(3)/66.(6)",3000, 0.070263)>]
    [<TestCase( "loanAmount spread between three best lenders 25/50/25",4000, 0.07125)>]
    [<TestCase( "loanAmount spread between all six lenders 10/20/10/20/20/20",10000, 0.0805)>]
    member test.``calculateTheBestRateBasedOnOffers finds the best rate`` (testCaseDesc, loanAmount, expectedRate) =
        let sampleCsv = """
            Lender,Rate,Available
            Bob,0.075,2000
            Jane,0.069,1000
            Fred,0.071,2000
            Mary,0.104,2000
            John,0.081,2000
            Dave,0.074,1000 """

        let offers = Offers.Parse(sampleCsv)
        let bestRate = calculateTheBestRateBasedOnOffers offers loanAmount

        bestRate.IsSome             |> should equal true
        bestRate.Value              |> should (equalWithin 0.005) expectedRate
        
    // test values calculated using compounded_loan_rate.xlsx
    [<TestCase(1000, 0.07004, 12, 3, 30.88, 1111.64)>]
    [<TestCase(1100, 0.069, 12, 3, 33.91, 1220.92)>]
    [<TestCase(2000, 0.07, 12, 3, 61.75419373, 2223.150974)>]
    [<TestCase(5500, 0.05, 12, 3, 164.8399341, 5934.237627)>]
    [<TestCase(10000, 0.1, 12, 3, 322.6718719, 11616.18739)>]
    [<TestCase(15000, 0.065, 12, 3, 459.7350431, 16550.46155)>]
    member test.``calculates monthly repayment`` (A, R, ppy, y, expectedMonthlyRepayment, expectedTotalRepayment) =
        let repayments = calculateRepayments A R ppy y

        fst repayments      |> should (equalWithin 0.005) expectedMonthlyRepayment
        snd repayments      |> should (equalWithin 0.005) expectedTotalRepayment
