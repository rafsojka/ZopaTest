module ZopaTest.Validation

let validateLoanAmount loanAmount = 
    if loanAmount < 1000 || loanAmount > 15000 then
        failwith (sprintf "Loan amount requested should be between 1000 and 15000 but it was %d." loanAmount)
    if loanAmount % 100 <> 0 then
        failwith (sprintf "Loan amount requested should be a multiplication of 100 but it was %d." loanAmount)
