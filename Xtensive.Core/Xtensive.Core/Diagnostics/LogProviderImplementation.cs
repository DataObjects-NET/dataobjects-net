// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Diagnostics.Configuration;
using Xtensive.IoC;
using ConfigurationSection = Xtensive.Diagnostics.Configuration.ConfigurationSection;
using DiagnosticsSection=Xtensive.Diagnostics.Configuration.ConfigurationSection;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Default <see cref="ILogProvider"/> implementation.
  /// </summary>
  [Serializable]
  [Service(typeof(ILogProvider), Default = true, Singleton = true)]
  public sealed class LogProviderImplementation : LogProviderImplementationBase
  {
    private const string DefaultLogName = "Default.log";
    private const string KeyPrefixRegex = @"(.*)\.([^.])+";

    /// <inheritdoc/>
    protected override ILog GetLogImplementation(IRealLog realLog)
    {
      return new LogImplementation(realLog);
    }

    /// <inheritdoc/>
    protected override IRealLog GetRealLog(string key)
    {
      // Looking for configuration section
      var settings = (ConfigurationSection) ConfigurationManager.GetSection(
        ConfigurationSection.DefaultSectionName);
      if (settings==null || settings.Logs==null)
        return GetDefaultRealLog(key); // Nothing is found, fallback to default.

      // Looking for log with specified name (key) there,
      // or one of its its parents
      var currentKey = key ?? string.Empty;
      LogElement logSettings = null;
      while (true) {
        logSettings = settings.Logs[currentKey];
        if (logSettings!=null)
          break;
        if (currentKey.IsNullOrEmpty())
          break;
        var nextKey = Regex.Replace(currentKey, KeyPrefixRegex, "${1}", RegexOptions.Multiline);
        if (nextKey!=currentKey)
          currentKey = nextKey;
        else
          currentKey = string.Empty;
      }
      if (logSettings==null)
        return GetDefaultRealLog(key); // Nothing is found, fallback to default.

      // Processing found settings
      TextualLogImplementationBase log = null;
      switch (logSettings.Provider) {
      case LogProviderType.File:
        var fileName = logSettings.FileName;
        try {
          log = new FileLog(key, fileName.IsNullOrEmpty() ? 
            Environment.GetCommandLineArgs()[0] + ".log" : fileName);
        }
        catch {
          log = new FileLog(key, fileName.IsNullOrEmpty() ? 
            DefaultLogName : fileName);
        }
        break;
      case LogProviderType.Console:
        log = new ConsoleLog(key);
        break;
      case LogProviderType.Debug:
        log = new DebugLog(key);
        break;
      case LogProviderType.Error:
        log = new ErrorLog(key);
        break;
      case LogProviderType.DebugOnlyConsole:
        log = new DebugOnlyConsoleLog(key);
        break;
      default:
        return new NullLog(key);
      }
      log.LoggedEventTypes = logSettings.Events;
      log.Format = logSettings.Format;
      if (!logSettings.FormatString.IsNullOrEmpty())
        log.FormatString = logSettings.FormatString;
      return log;
    }

    private static IRealLog GetDefaultRealLog(string key)
    {
      if (key == LogProviderType.Console.ToString())
        return new ConsoleLog(key);
      if (key == LogProviderType.Null.ToString())
        return new NullLog(key);
      if (key == LogProviderType.Debug.ToString())
        return new DebugLog(key);
      return new NullLog(key);
//      if (Debugger.IsAttached)
//        return new DebugLog(key);
//      else 
//        return new NullLog(key);
    }
  }
}