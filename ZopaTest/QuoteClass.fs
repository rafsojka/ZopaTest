module ZopaTest.QuoteClass

open ZopaTest.CalculationsWithBorrowersData

type QuoteClass(requestedAmount, rate, monthlyRepayment:decimal option, totalRepayment:decimal option) =
    member this.RequestedAmount = requestedAmount
    member this.Rate = rate
    member this.MonthlyRepayment = monthlyRepayment
    member this.TotalRepayment = totalRepayment

    abstract member PrintToStdout : unit -> unit

    default this.PrintToStdout () =
        printfn "Requested amount: £%d" this.RequestedAmount
        match (this.Rate, this.MonthlyRepayment, this.TotalRepayment) with
        | None, None, None ->
            // If the market does not have sufficient offers from lenders to satisfy the loan
            // then the system should inform the borrower that it is not possible to provide a quote at that time.
            printfn "It is not possible to provide a quote at this time."
        | Some rate, Some monthlyRepayment, Some totalRepayment ->
            printfn "Rate: %.1f%%" (rate * 100.0M)
            printfn "Monthly repayment: £%.2f" monthlyRepayment
            printfn "Total repayment: £%.2f" totalRepayment
        | _,_,_ -> failwith "Incorrect quote state"

type QuoteClassWithBorrowersData(requestedAmount, rate, monthlyRepayment, totalRepayment, borrowersData) =
    inherit QuoteClass(requestedAmount, rate, monthlyRepayment, totalRepayment)
    
    member this.BorrowersData = borrowersData

    override this.PrintToStdout () =
        base.PrintToStdout()
        match this.BorrowersData with
        | None -> ()
        | Some borrowersdata -> List.iter (fun bd -> printfn "%s %f %d" bd.Name bd.Rate bd.Amount) borrowersdata

// add QuoteFactory returning QuoteClass or QuoteClassWithBorrowersData based on quoteType parameter
