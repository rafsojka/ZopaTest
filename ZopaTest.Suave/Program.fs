// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.Threading

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Json
open Suave.RequestErrors

open Newtonsoft.Json

open ZopaTest.Interop.InteropModule

// ==============================================================================================

// based on http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/
let JSON v =
    JsonConvert.SerializeObject(v)
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

// ==============================================================================================

let app =
  choose
    [ GET >=> choose
        [ path "/json" >=> OK "Hello GET"
          path "/goodbye" >=> OK "Good bye GET" ] ]

// ==============================================================================================

let pathScanApp =
    choose [
        // string
        pathScan "/string/%d" (fun (loanAmount : int) -> OK(sprintfQuote <| getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" loanAmount ) ) 
        // json string
        pathScan "/json/%d" (fun (loanAmount : int) -> OK(JsonConvert.SerializeObject(getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" loanAmount) ) )
        // json
        pathScan "/quote/%d" (fun (loanAmount : int) -> JSON(getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" loanAmount ) )
    ]

// ==============================================================================================

// based on
// https://theimowski.gitbooks.io/suave-music-store/content/en/query_parameters.html
// http://putridparrot.com/blog/getting-restful-with-suave/
let browse =
    request (fun r ->
        match r.queryParam "loan_amount" with
        | Choice1Of2 loanAmount -> JSON (getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" (int loanAmount))
        | Choice2Of2 msg -> BAD_REQUEST msg)

let pathApp =
  choose
    [ GET >=> choose
        [ path "/quote" >=> browse ]
    ]

// ==============================================================================================

[<EntryPoint>]
let main argv = 
  let cts = new CancellationTokenSource()
  let conf = { defaultConfig with cancellationToken = cts.Token }
  let listening, server = startWebServerAsync conf pathScanApp
    
  Async.Start(server, cts.Token)
  printfn "Make requests now"
  Console.ReadKey true |> ignore
    
  cts.Cancel()

  0 // return an integer exit code
