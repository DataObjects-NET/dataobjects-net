// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;

namespace Xtensive.Threading
{
  [Serializable]
  internal class Task : ITask
  {
    private readonly Action implementation;

    public void Execute()
    {
      implementation.Invoke();
    }

    public void Terminate(Exception exception)
    {
      return;
    }


    // Constructors
    
    public Task(Action implementation)
    {
      this.implementation = implementation;
    }
  }
}