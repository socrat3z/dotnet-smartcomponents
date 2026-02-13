// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

/// <summary>
/// Utility class for loading shared configuration files within the repository.
/// </summary>
public static class RepoSharedConfigUtil
{
    /// <summary>
    /// Adds the repository shared configuration file to the configuration builder.
    /// This method searches for RepoSharedConfig.json starting from the entry assembly location
    /// and moving up the directory tree.
    /// </summary>
    /// <param name="configuration">The configuration builder to add the shared config to.</param>
    public static void AddRepoSharedConfig(this IConfigurationBuilder configuration)
    {
        // This is only used within this repo to simplify sharing config
        // across multiple projects. For real usage, just add the required
        // config values to your appsettings.json file.

        var envVarPath = Environment.GetEnvironmentVariable("SMARTCOMPONENTS_REPO_CONFIG_FILE_PATH");
        if (!string.IsNullOrEmpty(envVarPath))
        {
            configuration.AddJsonFile(envVarPath);
            return;
        }

        var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        while (true)
        {
            var path = Path.Combine(dir, "RepoSharedConfig.json");
            if (File.Exists(path))
            {
                configuration.AddJsonFile(path);
                return;
            }

            var parent = Directory.GetParent(dir);
            if (parent == null)
            {
                throw new FileNotFoundException("Could not find RepoSharedConfig.json");
            }

            dir = parent.FullName;
        }
    }

    /// <summary>
    /// Gets any configuration error that occurred during setup.
    /// </summary>
    /// <param name="config">The configuration to check for errors.</param>
    /// <returns>An exception if there was a configuration error, otherwise null.</returns>
    public static Exception? GetConfigError(IConfiguration config)
    {
        // TODO: now that we are using the Microsoft.Extensions.AI.Abstractions library, what should we do here? Maybe we can just remove this?

        //var apiConfigType = typeof(OpenAIInferenceBackend).Assembly
        //    .GetType("SmartComponents.Inference.OpenAI.ApiConfig", true)!;
        //try
        //{
        //    _ = Activator.CreateInstance(apiConfigType, config);
        //}
        //catch (TargetInvocationException ex) when (ex.InnerException is not null)
        //{
        //    return ex.InnerException;
        //}
        //catch (Exception ex)
        //{
        //    return ex;
        //}

        return null;
    }
}
