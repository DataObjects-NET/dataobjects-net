// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;

namespace Xtensive.Core.Threading
{
  [Serializable]
  internal sealed class Task<T> :
    ITask
  {
    private readonly Func<T> taskDelegate;

    private readonly TaskResult<T> taskResult;

    public void Execute()
    {
      try {
        var result = taskDelegate.Invoke();
        taskResult.Result = result;
      }
      catch(Exception e) {
        RegisterException(e);
        throw;
      }
    }

    public void RegisterException(Exception exception)
    {
      ((ITaskResult)taskResult).SetException(exception);
    }


    // Constructors
    
    public Task(Func<T> taskDelegate, TaskResult<T> taskResult)
    {
      this.taskDelegate = taskDelegate;
      this.taskResult = taskResult;
    }
  }
}