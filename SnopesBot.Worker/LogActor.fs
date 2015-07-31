namespace SnopesBot.Worker

open System
open System.Diagnostics
open Akka.Actor
open Akka.FSharp
open Akka.Event

type LogActor() =
    inherit UntypedActor()

    override x.OnReceive(msg) =
        match msg with
        | :? InitializeLogger ->
            x.Sender <! LoggerInitialized()
        | :? Error as err ->
            Trace.TraceError(err.ToString())
        | :? Warning as warn ->
            Trace.TraceWarning(warn.Message.ToString())
        | :? Info as info ->
            Trace.TraceInformation(info.Message.ToString())
        | :? Debug as debug ->
            Trace.TraceInformation(debug.Message.ToString())
        | _ ->
            x.Unhandled(msg)
