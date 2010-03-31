// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.Helpers;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Method | MulticastTargets.InstanceConstructor | MulticastTargets.StaticConstructor)]
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
  [ProvideAspectRole(StandardRoles.Tracing)]
  [Serializable]
  public sealed class TraceAttribute : OnMethodBoundaryAspect
  {
    private const string EnterFormat           = "{0}";
    private const string EnterFormatArgs       = "{0} ({1}) on {2}";
    private const string EnterFormatArgsStatic = "{0} ({1})";
    private const string ExitFormat            = "Exit {0}";
    private const string ExitFormatResult      = "Exit {0}, result: {1}";
    private const string ExitFormatError       = "Exit {0}, exception: {1}";
    private const string LogTypeName = "Log";

    private Type                                logType;
    private ILog                                log;
    private Action<ILog, string, object[]>      logEvent;
    private TraceOptions                        options;
    private LogEventTypes                       eventType;
    private string                              title;
    private bool                                isStatic;

    /*int ILaosWeavableAspect.AspectPriority {
      [DebuggerStepThrough]
      get { return (int)CoreAspectPriority.Trace; }
    }*/

    public string Title {
      [DebuggerStepThrough]
      get { return title; }
      [DebuggerStepThrough]
      set { title = value; }
    }

    public TraceOptions Options {
      [DebuggerStepThrough]
      get { return options; }
      [DebuggerStepThrough]
      set { options = value; }
    }

    public LogEventTypes EventType {
      [DebuggerStepThrough]
      get { return eventType; }
      [DebuggerStepThrough]
      set
      {
        if (eventType == value)
          return;
        eventType = value;
        switch (eventType) {
          case LogEventTypes.None:
            logEvent = (target, format, args) => {};
            break;
          case LogEventTypes.Debug:
            logEvent = (target, format, args) => log.Debug(format, args);
            break;
          case LogEventTypes.Info:
            logEvent = (target, format, args) => log.Info(format, args);
            break;
          case LogEventTypes.Warning:
            logEvent = (target, format, args) => log.Warning(format, args);
            break;
          case LogEventTypes.Error:
            logEvent = (target, format, args) => log.Error(format, args);
            break;
          case LogEventTypes.FatalError:
            logEvent = (target, format, args) => log.FatalError(format, args);
            break;
          case LogEventTypes.All:
            logEvent = (target, format, args) => log.Info(format, args);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    public Type LogType {
      [DebuggerStepThrough]
      get { return logType; }
      [DebuggerStepThrough]
      set { logType = value; }
    }

    public override bool CompileTimeValidate(MethodBase method)
    {
      if (title.IsNullOrEmpty())
        title = method.GetShortName(true);
      if (logType==null) {
        // Detecting log type...
        Assembly assembly = method.DeclaringType.Assembly;
        string nameSpace = method.DeclaringType.FullName;
        nameSpace = GetOuterNamespace(nameSpace);
        while (!nameSpace.IsNullOrEmpty()) {
          Type foundLogType = assembly.GetType(nameSpace + "." + LogTypeName, false);
          if (IsLogType(foundLogType)) {
            logType = foundLogType;
            break;
          }
          nameSpace = GetOuterNamespace(nameSpace);
        }
        if (logType==null) {
          ErrorLog.Write(SeverityType.Error, Strings.AspectExCannotFindLogFor,
            method.DeclaringType.GetShortName(), 
            GetOuterNamespace(method.DeclaringType.FullName));
          return false;
        }
      }
      isStatic = method.IsStatic;
      MethodInfo mi = method as MethodInfo;
      if (mi!=null && mi.ReturnType==typeof(void))
        options &= (TraceOptions.All ^ TraceOptions.Result);
      return true;
    }

    public override void RuntimeInitialize(MethodBase method)
    {
      try {
        PropertyInfo pi =
          logType.GetProperty("Instance", 
            BindingFlags.Public | 
            BindingFlags.Static | 
            BindingFlags.FlattenHierarchy);
        log = (ILog)pi.GetValue(null, null);
      }
      catch (NullReferenceException e) {
        Log.Error(e, Strings.LogCantResolveLogType, GetType().GetShortName());
      }
      catch (AmbiguousMatchException e) {
        Log.Error(e, Strings.LogCantResolveLogType, GetType().GetShortName());
      }
      base.RuntimeInitialize(method);
    }

    #region Helper methods

    private static string GetOuterNamespace(string nameSpace)
    {
      int i = nameSpace.LastIndexOf('.');
      if (i>0)
        return nameSpace.Substring(0, i);
      else
        return string.Empty;
    }

    private static bool IsLogType(Type candidate)
    {
      if (candidate==null)
        return false;
      try {
        return typeof (LogTemplate<>) == candidate.BaseType.GetGenericTypeDefinition().UnderlyingSystemType;
      }
      catch (NullReferenceException){
        return false;
      }
      catch (InvalidOperationException){
        return false;
      }
      catch (NotSupportedException){
        return false;
      }
    }

    #endregion
    
    public override void OnEntry(MethodExecutionArgs eventArgs)
    {
      if (log==null) {
        Log.Error(Strings.LogCantLogNoLogError, title);
        return;
      }
      if ((options & TraceOptions.Enter)!=0) {
        string format;
        object[] args;
        if ((options & TraceOptions.Arguments)!=0) {
          if (isStatic) {
            format = EnterFormatArgsStatic;
            args = new object[] {title, 
              new ObjectFormatter(eventArgs.Arguments, true)};
          }
          else {
            format = EnterFormatArgs;
            args = new object[] {title, 
              new ObjectFormatter(eventArgs.Arguments, true), 
              ObjectFormatter.ToString(eventArgs.Instance)};
          }
        }
        else {
          format = EnterFormat;
          args = new object[] {title};
        }
        logEvent(log, format, args);
      }
      if ((options & TraceOptions.Indent)!=0)
        eventArgs.MethodExecutionTag = new LogIndentScope();
    }

    public override void OnExit(MethodExecutionArgs eventArgs)
    {
      if (log==null)
        return;
      if ((options & TraceOptions.Indent)!=0) {
        LogIndentScope logIndentScope = (LogIndentScope)eventArgs.MethodExecutionTag;
        logIndentScope.DisposeSafely();
      }
      if ((options & TraceOptions.Exit)!=0) {
        string format;
        object[] args;
        if (eventArgs.Exception!=null) {
          format = ExitFormatError;
          args = new object[] {title, 
            ObjectFormatter.ToString(eventArgs.Exception)};
        }
        else {
          if ((options & TraceOptions.Result)!=0) {
            format = ExitFormatResult;
            args = new object[] {title, 
              ObjectFormatter.ToString(eventArgs.ReturnValue)};
          }
          else {
            format = ExitFormat;
            args = new object[] {title};
          }
        }
        logEvent(log, format, args);
      }
    }

    
    // Constructors

    public TraceAttribute()
      : this(null, TraceOptions.Default)
    {
    }

    public TraceAttribute(string methodName)
      : this(methodName, TraceOptions.Default)
    {
    }

    public TraceAttribute(TraceOptions options)
      : this(null, options)
    {
    }

    public TraceAttribute(string methodName, TraceOptions options)
    {
      this.title = methodName;
      this.options = options;
      EventType = LogEventTypes.Info;
    }
  }
}