using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Configuration;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

namespace Petabridge.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("app.conf")).BootstrapFromDocker();
            var actorSystem = ActorSystem.Create("ClusterSys", config);

            var pbm = PetabridgeCmd.Get(actorSystem);
            pbm.RegisterCommandPalette(ClusterCommands.Instance);
            pbm.RegisterCommandPalette(RemoteCommands.Instance);
            pbm.Start(); // begin listening for PBM management commands

            var persistentActor = actorSystem.ActorOf(Props.Create(() => new PersistentCountingActor("foo")), "foo");

            var serializer = actorSystem.Serialization.FindSerializerForType(typeof(object));
            actorSystem.Log.Info("Found serializer of type [{0}, Id={1}] for msg of type {2}", serializer, serializer.Identifier, typeof(object));

            actorSystem.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0.5), 
                TimeSpan.FromSeconds(2), persistentActor, 
                new PersistentCountingActor.MyVal(1), ActorRefs.NoSender);


            actorSystem.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5), () =>
                {
                    persistentActor.Ask<int>(PersistentCountingActor.GetSum.Instance)
                        .ContinueWith(tr => Console.WriteLine(tr.Result));
                });
            
            await actorSystem.WhenTerminated;
        }
    }
}
