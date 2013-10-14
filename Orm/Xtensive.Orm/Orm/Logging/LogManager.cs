// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.20

using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging.Internals;
using ConfigurationSection = Xtensive.Orm.Configuration.Elements.ConfigurationSection;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Manager class, which gives logs.
  /// </summary>
  public sealed class LogManager
  {
    private static readonly object syncObj = new object();
    private static LogProvider provider;

    /// <summary>
    /// Initialaze manager by creation logs from default section of configuration file.
    /// </summary>
    public static void Initialize()
    {
      var configuration = LoggingConfiguration.Load();
      Initialize(configuration);
    }
    
    /// <summary>
    /// Initialaze manager by privider.
    /// </summary>
    /// <param name="logProvider">Instance of class, which implements <see cref="LogProvider"/>.</param>
    public static void Initialize(LogProvider logProvider)
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
    public static void Initialize(LoggingConfiguration configuration)
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

    private static void EnsureIsNotInitialized()
    {
      if(provider!=null)
        throw new InvalidOperationException(Strings.ExLogManagerAlreadyInitialized);
    }

    internal static void AutoInitialize()
    {
      var section = (ConfigurationSection) ConfigurationManager.GetSection("Xtensive.Orm");
      var configuration = section != null ? section.Logging.ToNative() : new LoggingConfiguration();
      lock (syncObj) {
        if (provider==null) {
          Initialize(configuration);
        }
      }
    }

    internal static void Reset()
    {
      lock (syncObj) {
        provider = null;
      }
    }

    /// <summary>
    /// Gets log by name.
    /// </summary>
    /// <param name="logName">Name of log.</param>
    /// <returns>Founded log or default.</returns>
    public static BaseLog GetLog(string logName)
    {
      lock (syncObj) {
        if (provider==null)
          throw new InvalidOperationException(Strings.ExLogManagerMustBeInitializedBeforeUsing);
        return provider.GetLog(logName);
      }
    }
  }
}