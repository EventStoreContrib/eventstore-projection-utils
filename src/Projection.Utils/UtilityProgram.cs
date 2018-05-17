using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using PowerArgs;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EventStoreContrib.Utils
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class UtilityProgram
    {
        [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
        public bool Help { get; set; }

        [ArgActionMethod, ArgDescription("Ensures that the projections are enabled")]
        public void EnsureProjectionsEnabled(EnsureProjectionsEnabledArgs args)
        {
            (var projectionsManager, var userCreds) = Build(args);

            Console.WriteLine($"Ensuring that projections are enabled: {args.Projections}");
            var projectionsEnsureEnabledList = args.Projections
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            if (projectionsEnsureEnabledList.Any())
            {
                var ensureTasks = projectionsEnsureEnabledList.Select(async projectionName =>
                {
                    await projectionsManager.EnsureEventStoreProjectionEnabled(projectionName, userCreds);
                    Console.WriteLine($"Ensured that {projectionName} is running");
                }).ToArray();

                Task.WaitAll(ensureTasks);
                Console.WriteLine($"Done ensuring that projections are enabled.");
            }
            Console.WriteLine($"Complete: EnsureProjectionsEnabledArgs");
        }

        [ArgActionMethod, ArgDescription("Updates Projections that existing the folder if they don't match")]
        public void UpdateProjectionsIfNeeded(UpdateProjectionsIfNeededArgs args)
        {
            (var projectionsManager, var userCreds) = Build(args);
            var projectionDirectoryInfo = new DirectoryInfo(args.Directory);

            if (projectionDirectoryInfo.Exists)
            {
                var projectionTasks = projectionDirectoryInfo.GetFiles("*.js")
                    .ToList()
                    .Select(async fileInfo =>
                    {
                        Console.WriteLine($"Processing {fileInfo.Name}...");
                        var projectionScript = File.ReadAllText(fileInfo.FullName);
                        var projectionName = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
                        await projectionsManager.UpdateProjectionScriptIfNeeded(projectionName, projectionScript, userCreds);
                        await projectionsManager.EnsureEventStoreProjectionEnabled(projectionName, userCreds);
                        Console.WriteLine($"Done Processing {fileInfo.Name}...");
                    })
                    .ToList();

                if (projectionTasks.Any())
                {
                    Task.WaitAll(projectionTasks.ToArray());
                    Console.WriteLine($"Done updating projections and ensuring that they are enabled");
                }
                else
                {
                    Console.WriteLine($"No projections found in {projectionDirectoryInfo.FullName}");
                }
            }
            else
            {
                Console.WriteLine($"{projectionDirectoryInfo.FullName} does not exist. Skipping...");
            }
            Console.WriteLine($"Complete: UpdateProjectionsIfNeeded");
        }

        private (ProjectionsManager, UserCredentials) Build(BaseArgs args)
        {
            var creds = new UserCredentials(args.Username, args.Password);
            var eventStorePortNumber = Convert.ToInt32(args.Port);
            var ipAddress = IPAddress.Parse(args.IpAddress);
            var projectionsManager = new ProjectionsManager(new ConsoleLogger(),
                new IPEndPoint(ipAddress, eventStorePortNumber),
                TimeSpan.FromMilliseconds(args.OperationTimeout));

            return (projectionsManager, creds);
        }
    }
}