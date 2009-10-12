// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;

namespace Xtensive.Core.Diagnostics
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
    internal const string Console = "Console";
    internal const string Null = "Null";
    internal const string Debug = "Debug";

    /// <see cref="SingletonDocTemplate.Instance" copy="true"/>
    public static ILogProvider Instance
    {
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
    /// Gets <see cref="ILog"/> object forwarding logging messages to console.
    /// </summary>
    public static ILog ConsoleLog
    {
      get { return GetLog(Console); }
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to nothing.
    /// </summary>
    public static ILog NullLog
    {
      get { return GetLog(Null); }
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to nothing.
    /// </summary>
    public static ILog DebugLog
    {
      get { return GetLog(Debug); }
    }


    // Type initializer

    static LogProvider()
    {
      instance = ServiceLocator.GetInstance<ILogProvider>();
    }
  }
}