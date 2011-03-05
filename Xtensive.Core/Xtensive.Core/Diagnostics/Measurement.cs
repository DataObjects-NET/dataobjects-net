// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.11

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Time and memory measurement helper.
  /// </summary>
  public class Measurement: IDisposable
  {
    private readonly MeasurementOptions options;
    private readonly long     initialBytesAllocated;
    private readonly DateTime initialTime;

    private string   name = "";
    private string   fullName = "";
    private TimeSpan timeSpent;
    private long     memoryAllocated;
    private int      operationCount;
    private bool     isCompleted;
    private bool     isSuppressed;


    public string Name
    {
      [DebuggerStepThrough]
      get { return name; }
      [DebuggerStepThrough]
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        name = value;
        UpdateFullName();
      }
    }

    public string FullName {
      [DebuggerStepThrough]
      get { return fullName; }
      [DebuggerStepThrough]
      protected set { fullName = value; }
    }

    public MeasurementOptions Options
    {
      [DebuggerStepThrough]
      get { return options; }
    }

    public bool IsCompleted
    {
      [DebuggerStepThrough]
      get { return isCompleted; }
    }

    public int OperationCount {
      [DebuggerStepThrough]
      get { return operationCount; }
      [DebuggerStepThrough]
      set {
        operationCount = value;
        UpdateFullName();
      }
    }

    public TimeSpan TimeSpent {
      get {
        if (!isCompleted)
          return HighResolutionTime.Now.Subtract(initialTime);
        else
          return timeSpent;
      }
    }

    public long MemoryAllocated {
      get {
        if (!isCompleted)
          return GC.GetTotalMemory(false)-initialBytesAllocated;
        else
          return memoryAllocated;
      }
    }

    public virtual void Suppress()
    {
      isSuppressed = true;
    }

    public virtual void Complete()
    {
      if (isSuppressed)
        return;
      if (isCompleted)
        throw new InvalidOperationException(Strings.ExMeasurementIsAlreadyCompleted);
      isCompleted = true;
      timeSpent = HighResolutionTime.Now.Subtract(initialTime);
      memoryAllocated = GC.GetTotalMemory((options & MeasurementOptions.CollectGarbageOnLeave)>0)-initialBytesAllocated;
      if ((options & MeasurementOptions.Log)>0) {
        if ((options & MeasurementOptions.LogEnter)>0)
          Log.Info("Measurement: Leave {0}.", this);
        else
          Log.Info("Measurement: {0}.", this);
      }
    }

    protected virtual void UpdateFullName()
    {
      FullName = string.Format("{0} ({1})", name, operationCount);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      double kmbBase = operationCount/TimeSpent.TotalSeconds;
      string kmbFormat = "{0:F1}/s";
      if (kmbBase>=1000) {
        kmbFormat = "{0:F3} K/s";
        kmbBase = kmbBase/1000;
      }
      if (kmbBase>=1000) {
        kmbFormat = "{0:F3} M/s";
        kmbBase = kmbBase/1000;
      }
      if (kmbBase>=1000) {
        kmbFormat = "{0:F3} B/s";
        kmbBase = kmbBase/1000;
      }
      return string.Format(
        "{0}: Time: {1}ms, Memory: {2}kb{3}{4}",
        FullName, TimeSpent.TotalMilliseconds, MemoryAllocated/1000.0,
        operationCount!=0 ? string.Format(", Operations: "+kmbFormat, kmbBase) : "",
        !isCompleted ? ", not completed yet" : "");
    }
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Measurement()
      : this(MeasurementOptions.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="operationCount">The operation count.</param>
    public Measurement(int operationCount)
      : this(MeasurementOptions.Default, operationCount)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The measurement name.</param>
    public Measurement(string name)
      : this(name, MeasurementOptions.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The measurement  name.</param>
    /// <param name="operationCount">The operation count.</param>
    public Measurement(string name, int operationCount)
      : this(name, MeasurementOptions.Default, operationCount)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="options">The measurement options.</param>
    public Measurement(MeasurementOptions options)
      : this("Unnamed", options)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="options">The measurement options.</param>
    /// <param name="operationCount">The operation count.</param>
    public Measurement(MeasurementOptions options, int operationCount)
      : this("Unnamed", options, operationCount)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The measurement name.</param>
    /// <param name="options">The measurement options.</param>
    public Measurement(string name, MeasurementOptions options)
      : this(name, options, 0)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The measurement name.</param>
    /// <param name="options">The measurement options.</param>
    /// <param name="operationCount">The operation count.</param>
    public Measurement(string name, MeasurementOptions options, int operationCount)
    {
      this.options = options;
      this.operationCount = operationCount;
      Name = name;
      if ((options & MeasurementOptions.LogEnter)>0)
        Log.Info("Measurement: Enter {0}.", FullName);
      initialBytesAllocated = GC.GetTotalMemory((options & MeasurementOptions.CollectGarbageOnEnter)>0);
      initialTime = HighResolutionTime.Now;
    }

    // Destructors

    void IDisposable.Dispose() 
    {
      if (!isCompleted)
        Complete();
    }
  }
}