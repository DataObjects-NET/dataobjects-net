// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.17

using System;
using System.Diagnostics;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Describes <see cref="RemoteTask"/> state and settings.
  /// </summary>
  [Serializable]
  public class TaskInfo
  {
    private readonly string clientId;
    private readonly Guid identifier;
    private readonly string url;
    private readonly TaskState state;
    private readonly DateTime? startTime;
    private readonly Type instanceType;
    private readonly ProcessPriorityClass processPriorityClass;

    /// <summary>
    /// Gets client unique identifier.
    /// </summary>
    public string ClientId
    {
      get { return clientId; }
    }

    /// <summary>
    /// Gets task's identifier.
    /// </summary>
    public Guid Identifier
    {
      get { return identifier; }
    }

    /// <summary>
    /// Gets task's url.
    /// </summary>
    public string Url
    {
      get { return url; }
    }

    /// <summary>
    /// Gets task's state.
    /// </summary>
    public TaskState State
    {
      get { return state; }
    }

    /// <summary>
    /// Gets task's start time.
    /// </summary>
    public DateTime? StartTime
    {
      get { return startTime; }
    }
    
    /// <summary>
    /// Gets instance type.
    /// </summary>
    public Type InstanceType
    {
      get { return instanceType; }
    }

    /// <summary>
    /// Gets process priority class.
    /// </summary>
    public ProcessPriorityClass ProcessPriorityClass
    {
      get { return processPriorityClass; }
    }

    /// <summary>
    /// Create new instance of <see cref="TaskInfo"/>.
    /// </summary>
    /// <param name="identifier">Task's identifier.</param>
    /// <param name="url">Task's url.</param>
    /// <param name="state">Task's state.</param>
    /// <param name="startTime">Task's start time.</param>
    /// <param name="instanceType">Type of instance.</param>
    /// <param name="processPriorityClass">Process priority class.</param>
    /// <param name="clientId">Client's Id.</param>
    internal TaskInfo(Guid identifier, string url, TaskState state, DateTime? startTime, Type instanceType, ProcessPriorityClass processPriorityClass, string clientId)
    {
      this.url = url;
      this.clientId = clientId;
      this.processPriorityClass = processPriorityClass;
      this.instanceType = instanceType;
      this.startTime = startTime;
      this.state = state;
      this.identifier = identifier;
    }
  }
}