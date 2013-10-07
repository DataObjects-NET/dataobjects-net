// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Diagnostics;
using Xtensive.Reflection;

using Xtensive.IoC;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Provides (creates or resolves) <see cref="ILog"/> instances by their name.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public static class LogProvider
  {
    private static readonly ILogProvider instance;

    /// <see cref="SingletonDocTemplate.Instance" copy="true"/>
    public static ILogProvider Instance {
      [DebuggerStepThrough]
      get { return instance; }
    }

    /// <summary>
    /// Gets the <see cref="ILog"/> object by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to get the log for.</param>
    /// <returns>The <see cref="ILog"/> object.</returns>
    public static ILog GetLog(string key)
    {
      return Instance.GetLog(key);
    }

    /// <summary>
    /// Gets the <see cref="ILog"/> object for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to get the log for.</param>
    /// <returns>The <see cref="ILog"/> object.</returns>
    public static ILog GetLog(Type type)
    {
      return Instance.GetLog(type.GetFullName());
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to nothing.
    /// </summary>
    public static ILog NullLog
    {
      get { return GetLog(LogProviderType.Null.ToString()); }
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to console.
    /// </summary>
    public static ILog ConsoleLog
    {
      get { return GetLog(LogProviderType.Console.ToString()); }
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to debug output.
    /// </summary>
    public static ILog DebugLog
    {
      get { return GetLog(LogProviderType.Debug.ToString()); }
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to error output.
    /// </summary>
    public static ILog ErrorLog
    {
      get { return GetLog(LogProviderType.Error.ToString()); }
    }


    // Type initializer

    static LogProvider()
    {
      try {
        instance = ServiceContainer.Default.Get<ILogProvider>();
      }
      catch {
        instance = null;
      }
      if (instance==null)
        instance = new LogProviderImplementation();
    }
  }
}