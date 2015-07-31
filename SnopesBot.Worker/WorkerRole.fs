namespace SnopesBot.Worker

open System
namespace SnopesBot.Worker

open System.Collections.Generic
open System.Diagnostics
open System.Linq
open System.Net
open System.Threading
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Diagnostics
open Microsoft.WindowsAzure.ServiceRuntime
open Akka.Actor
open Akka.FSharp

type WorkerRole() =
    inherit RoleEntryPoint()

    let cts = new CancellationTokenSource()
    let mutable system: ActorSystem option = None

    override this.OnStart() = 

        let sys = Configuration.load() |> System.create "snopesbot"
        let outputActor = spawnObj sys "telegram-output" <@ fun () -> TelegramOutputActor() @>
        let botActor = spawnObj sys "bot" <@ fun () -> BotActor(outputActor) @>
        system <- Some(sys)

        Async.Start(TelegramInput.startPolling botActor 0, cts.Token)

        base.OnStart()

    override this.OnStop() =
        cts.Cancel()
        system |> Option.iter (fun s -> s.Shutdown())
        system <- None
        base.OnStop()
        