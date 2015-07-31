namespace SnopesBot.Worker

open Akka.Actor
open Akka.FSharp

type BotActor(outputActor:IActorRef) =
    inherit UntypedActor()

    let handleMessage (msg:Message) =
        ()

    override x.OnReceive(msg) =
        match msg with
        | :? Message as message ->
            handleMessage message
        | _ ->
            x.Unhandled()