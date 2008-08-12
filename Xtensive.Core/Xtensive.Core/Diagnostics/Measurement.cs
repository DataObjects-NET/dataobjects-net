// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.11

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Diagnostics
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
    private bool     isCompleted;
    private TimeSpan timeSpent;
    private long     memoryAllocated;
    private int      operationCount;


    [DebuggerStepThrough]
    public string Name
    {
      get { return name; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        name = value;
        UpdateFullName();
      }
    }

    [DebuggerStepThrough]
    public string FullName {
      get { return fullName; }
      protected set { fullName = value; }
    }

    [DebuggerStepThrough]
    public MeasurementOptions Options
    {
      get { return options; }
    }

    [DebuggerStepThrough]
    public bool IsCompleted
    {
      get { return isCompleted; }
    }

    [DebuggerStepThrough]
    public int OperationCount {
      get { return operationCount; }
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

    public virtual void Complete()
    {
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
      FullName = String.Format("{0} ({1})", name, operationCount);
    }
    
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
      return String.Format(
        "{0}: Time: {1}ms, Memory: {2}kb{3}{4}",
        FullName, TimeSpent.TotalMilliseconds, MemoryAllocated/1000.0,
        operationCount!=0 ? String.Format(", Operations: "+kmbFormat, kmbBase) : "",
        !isCompleted ? ", not completed yet" : "");
    }
    

    // Constructor
    
    public Measurement()
      : this(MeasurementOptions.Default)
    {
    }

    public Measurement(int operationCount)
      : this(MeasurementOptions.Default, operationCount)
    {
    }

    public Measurement(string name)
      : this(name, MeasurementOptions.Default)
    {
    }

    public Measurement(string name, int operationCount)
      : this(name, MeasurementOptions.Default, operationCount)
    {
    }

    public Measurement(MeasurementOptions options)
      : this("Unnamed", options)
    {
    }

    public Measurement(MeasurementOptions options, int operationCount)
      : this("Unnamed", options, operationCount)
    {
    }

    public Measurement(string name, MeasurementOptions options)
      : this(name, options, 0)
    {
    }


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

    // IDisposable impl.

    void IDisposable.Dispose() 
    {
      Complete();
    }
  }
}