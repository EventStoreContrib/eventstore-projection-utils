using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;

namespace EventStoreContrib.Utils
{
    public static class ProjectionsManagerExtensions
    {
        public static async Task<bool> UpdateProjectionScriptIfNeeded(this ProjectionsManager projectionsManager,
            string scriptName,
            string scriptValue,
            UserCredentials userCredentials = null)
        {
            var updateNeeded = false;
            var projectionName = scriptName;
            var proposedScript = scriptValue;
            var exists = await projectionsManager.ProjectionExists(projectionName, userCredentials);
            if (exists)
            {
                var currentScript = await projectionsManager.GetQueryAsync(projectionName, userCredentials);
                if (currentScript != proposedScript)
                {
                    updateNeeded = true;
                    await projectionsManager.UpdateQueryAsync(projectionName, proposedScript, userCredentials);
                }
            }
            else
            {
                updateNeeded = true;
                await projectionsManager.CreateContinuousAsync(projectionName, proposedScript, userCredentials);
            }
            return updateNeeded;
        }

        public static async Task<bool> ProjectionExists(this ProjectionsManager projectionsManager,
            string projectionName,
            UserCredentials userCredentials = null)
        {
            var exists = false;
            var allProjections = await projectionsManager.ListAllAsync(userCredentials);
            allProjections.ForEach(p =>
            {
                if (p.Name == projectionName)
                    exists = true;
            });
            return exists;
        }

        public static async Task EnsureEventStoreProjectionEnabled(this ProjectionsManager projectionsManager,
            string projectionName,
            UserCredentials userCredentials = null)
        {
            var running = false;
            var allProjections = await projectionsManager.ListAllAsync(userCredentials);

            foreach (var p in allProjections)
            {
                var name = p.Name;
                var status = p.Status;
                if (name == projectionName)
                {
                    running = status == "Running";
                    break;
                }
            }

            if (!running)
            {
                await projectionsManager.EnableAsync(projectionName, userCredentials);
            }
        }

        public static async Task EnsureSystemProjectionAreEnabled(this ProjectionsManager projectionsManager, UserCredentials userCredentials = null)
        {
            var defaultProjections = new[] { "$by_category", "$by_event_type", "$stream_by_category", "$streams", "$users" };
            foreach (var projectionName in defaultProjections.ToList())
            {
                await projectionsManager.EnsureEventStoreProjectionEnabled(projectionName, userCredentials);
            }
        }
    }
}