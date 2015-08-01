namespace SnopesBot.Worker

open System
open System.IO
open System.Net.Http
open Akka.Actor
open Akka.FSharp
open Akka.Event
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type BotActor(outputActor:IActorRef) =
    inherit UntypedActor()

    let context = UntypedActor.Context
    let client = new HttpClient()
    let googleUrl = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&q=site:snopes.com%20"
    
    let log = Logging.GetLogger(UntypedActor.Context)

    let handleCommand (outMsg:SendMessage) (command:string) (args:string[]) = async {
        let! ct = Async.CancellationToken
        match command.ToLower() with
        | "snopes" ->
            let searchUrl = Uri(googleUrl + String.Join("%20", args))
            let! response = client.AsyncGet(searchUrl, ct)
            if response.IsSuccessStatusCode then
                use! incoming = response.Content.AsyncReadAsStream()
                use sr = new StreamReader(incoming)
                use jtr = new JsonTextReader(sr)
                let result = JObject.Load(jtr)
                let resultUrl = string <| result.SelectToken("responseData.results[0].url")
                if not (String.IsNullOrEmpty resultUrl) then
                    return Some(SendMessage { outMsg with text = resultUrl })
                else
                    return None
            else
                return None
                
        | other ->
            return Some(SendMessage { outMsg with text = "Unknown command: " + other })
    }

    let handleMessage (msg:Message) =
        match msg.body with
        | Text text ->
            log.Debug("Incoming: {0}", text)
            let response = {
                chat_id = msg.chat.chat_id
                text = String.Empty
                disable_web_page_preview = None
                reply_to_message_id = Some(msg.message_id)
                reply_markup = None
            }
            if text.[0] = '/' then
                let parts = text.[1..].Split(' ')
                handleCommand response parts.[0] parts.[1..]
                |> pipeToOpt outputActor context.Self
            else
                let parts = text.Split(' ')
                handleCommand response "snopes" parts
                |> pipeToOpt outputActor context.Self
        | other ->
            log.Debug(sprintf "Ignored: %A" other)

    override x.OnReceive(msg) =
        match msg with
        | :? Message as message ->
            handleMessage message
        | _ ->
            x.Unhandled(msg)

    override x.PostStop() =
        client.Dispose()