// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

namespace Xtensive.Core.Links
{
  internal class OperationCallRegistrator<TArg>
  {
    public OperationCallCollector<TArg> ActiveCollector;
    public bool AddedToLatestContext;
    public ExecutionContextSwitcher ContextSwitcher;
    public Operation<TArg> Operation;

    public void RegisterCall(ref TArg arg)
    {
      if (!AddedToLatestContext) {
        if (ActiveCollector.Next!=null) {
          ActiveCollector = ActiveCollector.Next;
          ContextSwitcher.AddCollector(ActiveCollector);
          AddedToLatestContext = true;
        }
        else {
          ActiveCollector.Next = new OperationCallCollector<TArg>(Operation, this);
          ActiveCollector.Next.Previous = ActiveCollector;
          ActiveCollector = ActiveCollector.Next;
          ContextSwitcher.AddCollector(ActiveCollector);
          AddedToLatestContext = true;
        }
      }
      ActiveCollector.Args.Add(ref arg);
    }

    public OperationCallRegistrator(ExecutionContextSwitcher contextSwitcher, Operation<TArg> operation)
    {
      Operation = operation;
      AddedToLatestContext = false;
      ContextSwitcher = contextSwitcher;
      ActiveCollector = new OperationCallCollector<TArg>(Operation, this);
    }
  }
}