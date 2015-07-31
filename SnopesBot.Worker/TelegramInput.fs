namespace SnopesBot.Worker

open System
open System.IO
open System.Diagnostics
open System.Net.Http
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Akka.Actor
open Akka.FSharp

module TelegramInput =
    let private bodyPropNames =
        ["text";"audio";"document";"photo";"sticker";"video";"contact";
         "location";"new_chat_participant";"left_chat_participant";
         "new_chat_title";"new_chat_photo";"delete_chat_photo";"group_chat_created"]

    let private getOneOf (jt:#JToken) (propNames:#seq<string>) =
        propNames |> Seq.tryPick (fun n ->
            let prop = jt.[n]
            if isNull prop then None
            else Some(n, prop)
        )

    let rec private parseMessage (jobj:JToken) =
        {
            message_id = jobj?message_id
            from = jobj.["from"].ToObject<User>()
            date = DateTimeOffset.FromUnixTime(jobj?date)
            chat =
                let chatObj: JToken = jobj?chat
                if isNotNull chatObj?first_name then
                    User(chatObj.ToObject())
                else
                    GroupChat(chatObj.ToObject())
            body =
                match getOneOf jobj bodyPropNames with
                | Some("text", p) -> Text(p.Value<_>())
                | Some("audio", p) -> Audio(p.ToObject())
                | Some("document", p) -> Document(p.ToObject())
                | Some("photo", p) -> Photo(p.ToObject())
                | Some("sticker", p) -> Sticker(p.ToObject())
                | Some("video", p) -> Video(p.ToObject())
                | Some("contact", p) -> Contact(p.ToObject())
                | Some("location", p) -> Location(p.ToObject())
                | Some("new_chat_participant", p) -> UserJoined(p.ToObject())
                | Some("left_chat_participant", p) -> UserLeft(p.ToObject())
                | Some("new_chat_title", p) -> NewChatTitle(p.Value<_>())
                | Some("new_chat_photo", p) -> NewChatPhoto(p.ToObject())
                | Some("delete_chat_photo", _) -> DeleteChatPhoto
                | Some("group_chat_created", _) -> GroupChatCreated
                | _ -> NoMessage
                
            forward_from =
                match jobj.["forward_from"] with
                | null -> None
                | jt -> Some(jt.ToObject<User>())
            forward_date =
                match jobj.["forward_date"] with
                | null -> None
                | jt -> Some(DateTimeOffset.FromUnixTime(jt.Value<_>()))
            reply_to_message =
                match jobj.["reply_to_message"] with
                | null -> None
                | jt -> Some(parseMessage jt)
            caption =
                match jobj.["caption"] with
                | null -> None
                | jt -> Some(jt.Value<string>())
                    
        }

    let private getUpdatesUri (offset:int) = 
        [("offset", string offset); ("timeout", "300")]
        |> methodUri "getUpdates"

    let rec startPolling (processor:IActorRef) offset = async {
        
        let! ct = Async.CancellationToken
        use client = new HttpClient()
        let uri = getUpdatesUri offset
        let! response = client.AsyncGet(uri, ct)
        try
            response.EnsureSuccessStatusCode() |> ignore

            use! incoming = response.Content.AsyncReadAsStream()
            use sr = new StreamReader(incoming)
            use jtr = new JsonTextReader(sr)
            let updates = JObject.Load(jtr)
            let mutable lastOffset = offset

            if updates?ok then
                let results: JArray = updates?result
                for update in results do
                    lastOffset <- update?update_id
                    let message: JObject = update?message
                    if isNotNull message then
                        processor.Tell(parseMessage message, ActorRefs.NoSender)

            return! startPolling processor (lastOffset + 1)

        with ex ->
            Trace.TraceError("Error polling Telegram messages. Sleeping for 1 second.\n{0}", ex)
            do! Async.Sleep 1000
            return! startPolling processor offset
    }



