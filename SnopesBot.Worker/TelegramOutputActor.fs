namespace SnopesBot.Worker

open System.IO
open System.Net.Http
open System.Threading
open Akka.Actor
open Akka.FSharp
open Akka.Event
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module private OutputUtils =

    let private serializer = JsonSerializer()

    let serialize (item:obj) =
        use sw = new StringWriter()
        serializer.Serialize(sw, item)
        sw.ToString()

    let deserialize<'t> (stream:Stream) =
        use sr = new StreamReader(stream)
        use jtr = new JsonTextReader(sr)
        serializer.Deserialize<'t>(jtr)

    let kvp (key, value) =
        System.Collections.Generic.KeyValuePair(key, value)

open OutputUtils

type TelegramOutputActor() =
    inherit UntypedActor()

    let log = Logging.GetLogger(UntypedActor.Context)
    let client = new HttpClient()
    let cts = new CancellationTokenSource()

    let handleMethod msg = async {
        let! ct = Async.CancellationToken
        match msg with
        | GetMe ->
            let uri = methodUri "getMe" []
            let! response = client.AsyncGet(uri, ct)
            response.EnsureSuccessStatusCode() |> ignore
            use! incoming = response.Content.AsyncReadAsStream()
            let user = deserialize<User> incoming
            log.Info(sprintf "%A" user)
        | SendMessage(msg) ->
            let uri = methodUri "sendMessage" []
            let formData = ResizeArray()
            formData.Add("chat_id", string msg.chat_id)
            formData.Add("text", msg.text)
            msg.disable_web_page_preview
            |> Option.iter (fun x -> formData.Add("disable_web_page_preview", (string x).ToLower()))
            msg.reply_to_message_id
            |> Option.iter (fun x -> formData.Add("reply_to_message_id", string x))
            msg.reply_markup
            |> Option.iter (fun x -> formData.Add("reply_markup", serialize x))
            
            use content = new FormUrlEncodedContent(formData |> Seq.map kvp)
            let! response = client.AsyncPost(uri, content, ct)
            response.EnsureSuccessStatusCode() |> ignore

        | SendChatAction(chat_id, action) ->
            let uri =
                [("chat_id", string chat_id); ("action", string action)]
                |> methodUri "sendChatAction"
            let! response = client.AsyncGet(uri)
            response.EnsureSuccessStatusCode() |> ignore

        | _ ->
            log.Warning(sprintf "Unsupported method: %A" msg)
    }

    override x.OnReceive(msg) =
        match msg with
        | :? TelegramBotMethod as botMethod ->
            Async.Start(handleMethod botMethod, cts.Token)
        | _ ->
            x.Unhandled()

    override x.PostStop() =
        cts.Cancel()
        client.Dispose()