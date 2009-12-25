// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.18

using System;
namespace Xtensive.Distributed.Test
{
  public class ProcessAbortedEventArgs: EventArgs
  {
    private readonly Exception exception;

    public ProcessAbortedEventArgs(Exception exception)
    {
      this.exception = exception;
    }

    public Exception Exception
    {
      get { return exception; }
    }
  }
}