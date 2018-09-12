// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.20

using System;
using System.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Manager class, which gives logs.
  /// </summary>
  public sealed class LogManager
  {
    private static readonly LogManager defaultInstance = new LogManager();

    private readonly object syncObj = new object();
    private LogProvider provider;

    /// <summary>
    /// Gets default <see cref="LogManager"/> instance.
    /// </summary>
    public static LogManager Default { get { return defaultInstance; } }
    
    /// <summary>
    /// Initialaze manager by creation logs from default section of configuration file.
    /// </summary>
    public void Initialize(System.Configuration.Configuration configuration)
    {
      var loggingConfiguration = LoggingConfiguration.Load(configuration);
      Initialize(loggingConfiguration);
    }
    
    /// <summary>
    /// Initialaze manager by privider.
    /// </summary>
    /// <param name="logProvider">Instance of class, which implements <see cref="LogProvider"/>.</param>
    public void Initialize(LogProvider logProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(logProvider, "logProvider");
      lock (syncObj) {
        EnsureIsNotInitialized();
        provider = logProvider;
      }
    }

    /// <summary>
    /// Initialaze manager by <see cref="LoggingConfiguration"/> instance.
    /// </summary>
    /// <param name="configuration">Configuration of logging.</param>
    public void Initialize(LoggingConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      lock (syncObj) {
        EnsureIsNotInitialized();
        if (!string.IsNullOrEmpty(configuration.Provider)) {
          var providerType = Type.GetType(configuration.Provider);
          if (providerType==null)
            throw new InvalidOperationException(string.Format(Strings.ExUnableToGetTypeOfProviderByNameX, configuration.Provider));
          provider = Activator.CreateInstance(providerType) as LogProvider;
          if (provider==null)
            throw new InvalidOperationException(string.Format(Strings.ExProviderXDoesNotImplementLogProviderClass, configuration.Provider));
          return;
        }
        provider = new InternalLogProvider(configuration.Logs);
      }
    }

    private void EnsureIsNotInitialized()
    {
      if(provider!=null)
        throw new InvalidOperationException(Strings.ExLogManagerAlreadyInitialized);
    }

    internal void AutoInitialize()
    {
      //var configuration =
      //  ConfigurationManager.GetSection(WellKnown.DefaultConfigurationSection)!=null
      //    ? LoggingConfiguration.Load()
      //    : new LoggingConfiguration();
      var configuration = new LoggingConfiguration();
      lock (syncObj) {
        if (provider==null) {
          Initialize(configuration);
        }
      }
    }

    /// <summary>
    /// Gets log by name.
    /// </summary>
    /// <param name="logName">Name of log.</param>
    /// <returns>Founded log or default.</returns>
    public BaseLog GetLog(string logName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(logName, "logName");

      lock (syncObj) {
        if (provider==null)
          throw new InvalidOperationException(Strings.ExLogManagerMustBeInitializedBeforeUsing);
        return provider.GetLog(logName);
      }
    }
  }
}