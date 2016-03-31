using System;
using System.IO;
using Microsoft.AspNetCore.Server.Testing;
using Microsoft.Extensions.Logging;

namespace E2ETests
{
    public class Helpers
    {
        public static bool RunningOnMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }

        public static string GetApplicationPath()
        {
#if NETSTANDARDAPP_15
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "MusicStore"));
#else
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "..", "src", "MusicStore"));
#endif
        }

        public static void SetInMemoryStoreForIIS(DeploymentParameters deploymentParameters, ILogger logger)
        {
            if (deploymentParameters.ServerType == ServerType.IIS)
            {
                // Can't use localdb with IIS. Setting an override to use InMemoryStore.
                logger.LogInformation("Creating configoverride.json file to override default config.");

                var overrideConfig = Path.Combine(deploymentParameters.ApplicationPath, ".." , "configoverride.json");

                File.WriteAllText(overrideConfig, "{\"UseInMemoryDatabase\": \"true\"}");
            }
        }
    }
}