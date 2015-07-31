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

    let methodUri (methodName:string) (parameters:#seq<string * string>) =
        let builder = UriBuilder(Uri(baseUri, methodName))
        builder.Query <- queryString parameters
        builder.Uri

    let inline isNull (x:obj) = Object.ReferenceEquals(x, null)

    let isNotNull: obj -> bool = (isNull >> not)

    //type System.Net.Http.HttpClient with
    //    member x.AsyncGet() =
    //        x.Get

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
