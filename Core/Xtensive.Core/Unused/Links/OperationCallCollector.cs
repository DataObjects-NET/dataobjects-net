// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

namespace Xtensive.Core.Links
{
  internal class OperationCallCollector<TArg> : IOperationCallCollector
  {
    public HybridList<TArg> Args;
    public OperationCallCollector<TArg> Next;
    public Operation<TArg> Operation;
    public OperationCallCollector<TArg> Previous;
    public OperationCallRegistrator<TArg> Registrator;



    public void ExecutePrologue()
    {
      int count = Args.Count;
      ExecutionStage stage = ExecutionStage.Prologue;
      if (count < 5)
        if (count < 3)
          if (count==2) {
            Operation.ExecuteCall(ref Args.Item0, ref stage);
            Operation.ExecuteCall(ref Args.Item1, ref stage);
          }
          else
            Operation.ExecuteCall(ref Args.Item0, ref stage);
        else if (count==4) {
          Operation.ExecuteCall(ref Args.Item0, ref stage);
          Operation.ExecuteCall(ref Args.Item1, ref stage);
          Operation.ExecuteCall(ref Args.Item2, ref stage);
          Operation.ExecuteCall(ref Args.Item3, ref stage);
        }
        else {
          Operation.ExecuteCall(ref Args.Item0, ref stage);
          Operation.ExecuteCall(ref Args.Item1, ref stage);
          Operation.ExecuteCall(ref Args.Item2, ref stage);
        }
      else
        for (int i = 0; i < count; i++) {
          TArg arg = Args[i];
          Operation.ExecuteCall(ref arg, ref stage);
        }
    }

    public void ExecuteOperation()
    {
      ExecutionStage stage = ExecutionStage.Operation;
      int count = Args.Count;
      if (count < 5)
        if (count < 3)
          if (count==2) {
            Operation.ExecuteCall(ref Args.Item0, ref stage);
            Operation.ExecuteCall(ref Args.Item1, ref stage);
          }
          else
            Operation.ExecuteCall(ref Args.Item0, ref stage);
        else if (count==4) {
          Operation.ExecuteCall(ref Args.Item0, ref stage);
          Operation.ExecuteCall(ref Args.Item1, ref stage);
          Operation.ExecuteCall(ref Args.Item2, ref stage);
          Operation.ExecuteCall(ref Args.Item3, ref stage);
        }
        else {
          Operation.ExecuteCall(ref Args.Item0, ref stage);
          Operation.ExecuteCall(ref Args.Item1, ref stage);
          Operation.ExecuteCall(ref Args.Item2, ref stage);
        }
      else
        for (int i = 0; i < count; i++) {
          TArg arg = Args[i];
          Operation.ExecuteCall(ref arg, ref stage);
        }
    }

    public void ExecuteEpilogue()
    {
      ExecutionStage stage = ExecutionStage.Epiloge;
      int count = Args.Count;
      if (count < 5)
        if (count < 3)
          if (count==2) {
            Operation.ExecuteCall(ref Args.Item0, ref stage);
            Operation.ExecuteCall(ref Args.Item1, ref stage);
          }
          else
            Operation.ExecuteCall(ref Args.Item0, ref stage);
        else if (count==4) {
          Operation.ExecuteCall(ref Args.Item0, ref stage);
          Operation.ExecuteCall(ref Args.Item1, ref stage);
          Operation.ExecuteCall(ref Args.Item2, ref stage);
          Operation.ExecuteCall(ref Args.Item3, ref stage);
        }
        else {
          Operation.ExecuteCall(ref Args.Item0, ref stage);
          Operation.ExecuteCall(ref Args.Item1, ref stage);
          Operation.ExecuteCall(ref Args.Item2, ref stage);
        }
      else
        for (int i = 0; i < count; i++) {
          TArg arg = Args[i];
          Operation.ExecuteCall(ref arg, ref stage);
        }
    }

    public void OnBeforeExecute()
    {
      Registrator.AddedToLatestContext = false;
    }

    public void OnAfterExecute()
    {
      Args.Clear();
      Registrator.ActiveCollector = Previous;
    }



    public OperationCallCollector(Operation<TArg> operation, OperationCallRegistrator<TArg> registrator)
    {
      Operation = operation;
      Registrator = registrator;
    }
  }
}