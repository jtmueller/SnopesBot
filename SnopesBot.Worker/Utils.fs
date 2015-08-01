namespace SnopesBot.Worker

[<AutoOpen>]
module Utils =
    open System

    let botToken = "114606830:AAEiWn0dCVaCcLjYJ4yxKTrIvxQI1_4xld4"

    let baseUri = Uri(sprintf "https://api.telegram.org/bot%s/" botToken)

    let private queryString (parameters:seq<string * string>) =
        parameters
        |> Seq.map (fun (k, v) -> k + "=" + v)
        |> String.concat "&"

    let methodUri (methodName:string) (parameters:(string * string) list) =
        let builder = UriBuilder(Uri(baseUri, methodName))
        if parameters <> [] then
            builder.Query <- queryString parameters
        builder.Uri

    let inline isNull (x:obj) = Object.ReferenceEquals(x, null)

    let isNotNull: obj -> bool = (isNull >> not)

    type System.Net.Http.HttpClient with
        member x.AsyncGet(uri:Uri, ct:Threading.CancellationToken) =
            x.GetAsync(uri, ct) |> Async.AwaitTask

        member x.AsyncGet(uri:Uri) =
            x.GetAsync(uri) |> Async.AwaitTask

        member x.AsyncPost(uri:Uri, content:System.Net.Http.HttpContent) =
            x.PostAsync(uri, content) |> Async.AwaitTask

        member x.AsyncPost(uri:Uri, content:System.Net.Http.HttpContent, ct) =
            x.PostAsync(uri, content, ct) |> Async.AwaitTask

    type System.Net.Http.HttpContent with
        member x.AsyncReadAsStream() =
            x.ReadAsStreamAsync() |> Async.AwaitTask

    let private epoch = DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)

    type DateTimeOffset with
        static member FromUnixTime(unixTime:int) =
            epoch.AddSeconds(float unixTime)

namespace Newtonsoft.Json.Linq

[<AutoOpen>]
module Extensions =

    let (?) (this:JToken) (prop:string) : 'result =
        this.Value<'result>(prop)

    let (?<-) (this:JToken) (prop:string) (value:'value) =
        this.[prop] <- value

namespace Akka.FSharp

[<AutoOpen>]
module Extensions =
    open Akka.Actor
    
    let pipeToOpt (recipient: ICanTell) (sender: IActorRef) (computation: Async<'T option>) : unit =  
        let success (result: 'T option) : unit = 
            match result with
            | Some x ->
                recipient.Tell(x, sender) 
            | None -> ()
        let failure (err: exn) : unit = recipient.Tell(Status.Failure(err), sender)
        Async.StartWithContinuations(computation, success, failure, failure) 
