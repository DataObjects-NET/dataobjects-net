// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;

namespace Xtensive.Threading
{
  [Serializable]
  internal sealed class Task<T> : ITask
  {
    private readonly Func<T> implementation;
    private readonly TaskResult<T> result;

    public TaskResult<T> Result
    {
      get { return result; }
    }

    public void Execute()
    {
      try {
        var r = implementation.Invoke();
        result.SetResult(r);
      }
      catch (Exception e) {
        Terminate(e);
        throw;
      }
    }

    public void Terminate(Exception exception)
    {
      result.SetException(exception);
    }


    // Constructors
    
    public Task(Func<T> implementation)
    {
      result = new TaskResult<T>();
      this.implementation = implementation;
    }
  }
}