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
open Suave.DotLiquid
open DotLiquid

open Newtonsoft.Json

open ZopaTest.Interop.InteropModule

// as per 03/01/18 Suave does NOT support content negotiation

// Giraffe does - https://dusted.codes/functional-aspnet-core-part-2-hello-world-from-giraffe
// but Giraffe is based on ASP.NET Core...

// ==============================================================================================
// DotLiquid stuff

// System.MissingFieldException: Field not found: 'DotLiquid.Template.NamingConvention'
// needed downgrade to DotLiquid 2.0.55 to make the System.MissingFieldException: Field not found: 'DotLiquid.Template.NamingConvention' go away
// https://github.com/SuaveIO/suave/issues/642 - downgrade to DotLiquid 2.0.55 breaks understanding Option type
// https://github.com/SuaveIO/suave/issues/662 - upgrading to Suave.DotLiquid 2.3.0-beta3 solved the problem (depends on .NET Framework 4.6)

// no built-in way of formatting decimal places
// custom filter needed
// https://github.com/dotliquid/dotliquid/issues/111

DotLiquid.setTemplatesDir  "."
DotLiquid.setCSharpNamingConvention ()

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

// based on
// https://theimowski.gitbooks.io/suave-music-store/content/en/query_parameters.html
// http://putridparrot.com/blog/getting-restful-with-suave/
let browse =
    request (fun r ->
        match r.queryParam "loan_amount" with
        | Choice1Of2 loanAmount -> JSON (getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" (int loanAmount))
        | Choice2Of2 msg -> BAD_REQUEST msg)

let browse2 =
    request (fun r ->
        request (fun r ->
            match r.queryParam "loan_amount" with
            | Choice1Of2 loanAmount -> page "Quote.html" (getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" (int loanAmount))
            | Choice2Of2 msg -> BAD_REQUEST msg))

let pathApp =
  choose
    [ GET >=> choose
        [ path "/quote" >=> browse 
          path "/html" >=> browse2
        ]
    ]

// ==============================================================================================

let pathScanApp =
    choose [
        // string
        pathScan "/string/%d" (fun (loanAmount : int) -> OK(sprintfQuote <| getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" loanAmount ) ) 
        // json string
        pathScan "/jsonstring/%d" (fun (loanAmount : int) -> OK(JsonConvert.SerializeObject(getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" loanAmount) ) )
        // json
        pathScan "/json/%d" (fun (loanAmount : int) -> JSON(getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" loanAmount ) )
        // html
        pathScan "/html/%d" (fun (loanAmount : int) -> page "Quote.html" (getQuote @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" loanAmount ) )
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
