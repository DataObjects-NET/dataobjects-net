// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;

namespace Xtensive.Core.Threading
{
  [Serializable]
  internal sealed class Task : ITask
  {
    private readonly Action taskDelegate;

    public void Execute()
    {
      taskDelegate.Invoke();
    }

    public void RegisterException(Exception exception)
    {}


    // Constructors
    
    public Task(Action taskDelegate)
    {
      this.taskDelegate = taskDelegate;
    }
  }
}