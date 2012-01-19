// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.19

using System;

namespace Xtensive.Core.Links
{
  internal class ExecutionContextSwitcher
  {
    [ThreadStatic] 
    private static ExecutionContextSwitcher current;
    private static ExecutionContext currentContext;

    internal static ExecutionContextSwitcher Current
    {
      get
      {
        if (current==null)
          current = new ExecutionContextSwitcher();
        return current;
      }
    }

    public ExecutionContext CurrentContext
    {
      get
      {
        if (currentContext!=null)
          return currentContext;
        currentContext = new ExecutionContext(this, ExecutionContextState.Idle);
        return currentContext;
      }
      set { currentContext = value; }
    }

    public void AddCollector(IOperationCallCollector collector)
    {
      CurrentContext.callCollectors.Add(ref collector);
    }
  }
}