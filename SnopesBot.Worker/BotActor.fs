namespace SnopesBot.Worker

open Akka.Actor
open Akka.FSharp
open Akka.Event

type BotActor(outputActor:IActorRef) =
    inherit UntypedActor()

    let log = Logging.GetLogger(UntypedActor.Context)

    let handleMessage (msg:Message) =
        match msg.body with
        | Text text ->
            log.Debug("Incoming: {0}", text)
            let response = {
                chat_id = msg.chat.chat_id
                text = "You said: " + text
                disable_web_page_preview = None
                reply_to_message_id = None
                reply_markup = None
            }
            outputActor <! SendMessage(response)
        | other ->
            log.Debug(sprintf "Ignored: %A" other)

    override x.OnReceive(msg) =
        match msg with
        | :? Message as message ->
            handleMessage message
        | _ ->
            x.Unhandled()