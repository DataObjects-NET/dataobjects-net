// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Util;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Diagnostics.log4net
{
  /// <summary>
  /// <see cref="IRealLog"/> implementation based on
  /// <see langword="log4net"/>'s <see cref="ILogger"/>.
  /// </summary>
  public sealed class RealLogImplementation: RealLogImplementationBase
  {
    private static Type thisType = typeof(RealLogImplementation);
    private readonly ILogger logger;

    /// <inheritdoc/>
    public override void LogEvent(LogEventTypes eventType, object message, Exception exception, IRealLog sentTo, LogCaptureScope capturedBy)
    {
      if (IsLogged(eventType)) {
        ThreadContextProperties tcp = ThreadContext.Properties;
        try {
          if (capturedBy==null) {
            tcp["indent"] = LogIndentScope.CurrentIndent;
            tcp["indentString"] = LogIndentScope.CurrentIndentString;
          }
          else {
            int indent = LogIndentScope.CurrentIndent - capturedBy.InitialIndent;
            tcp["indent"] = indent;
            tcp["indentString"] = LogIndent.Create(indent).StringValue;
          }
        }
        catch (InvalidOperationException) {
        }
        if (exception != null)
          message = message + "\r\nOriginal error:";
        logger.Log(thisType, ToLevel(eventType), message, exception);
      }
      base.LogEvent(eventType, message, exception, sentTo, capturedBy);
    }

    // TODO: Support for log4net repository reconfiguration
    /// <inheritdoc/>
    public override void UpdateCachedProperties()
    {
      loggedEventTypes = 0;
      if (logger!=null) {
        loggedEventTypes |= logger.IsEnabledFor(ToLevel(LogEventTypes.Debug)) ? LogEventTypes.Debug : 0;
        loggedEventTypes |= logger.IsEnabledFor(ToLevel(LogEventTypes.Info)) ? LogEventTypes.Info : 0;
        loggedEventTypes |= logger.IsEnabledFor(ToLevel(LogEventTypes.Warning)) ? LogEventTypes.Warning : 0;
        loggedEventTypes |= logger.IsEnabledFor(ToLevel(LogEventTypes.Error)) ? LogEventTypes.Error : 0;
        loggedEventTypes |= logger.IsEnabledFor(ToLevel(LogEventTypes.FatalError)) ? LogEventTypes.FatalError : 0;
      }
      base.UpdateCachedProperties();
    }
    
    #region Private methods

    /// <exception cref="InvalidOperationException">RealLogImplementation.ToLevel: Wrong LogEventTypes.</exception>
    private Level ToLevel(LogEventTypes eventType)
    {
      switch(eventType) {
        case LogEventTypes.Debug:
          return Level.Debug;
        case LogEventTypes.Info:
          return Level.Info;
        case LogEventTypes.Warning:
          return Level.Warn;
        case LogEventTypes.Error:
          return Level.Error;
        case LogEventTypes.FatalError:
          return Level.Fatal;
        default:
          throw Exceptions.InternalError("RealLogImplementation.ToLevel: Wrong LogEventTypes.", 
            log4net.Log.Instance);
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Log name.</param>
    /// <param name="logger">Underlying <see langword="log4net"/> logger.</param>
    public RealLogImplementation(string name, ILogger logger)
      : base(name)
    {
      this.logger = logger;
      UpdateCachedProperties();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Log name.</param>
    public RealLogImplementation(string name)
      : this (name, LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), name))
    {
    }
  }
}