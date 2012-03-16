// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

namespace Xtensive.Core.Links
{
  internal class ExecutionContext
  {
    public HybridList<IOperationCallCollector> callCollectors;
    private ExecutionContextState executionContextState = ExecutionContextState.Idle;
    private ExecutionContextSwitcher executionContextSwitcher;
    private ExecutionContext next;
    private ExecutionContext previous;

    private void ProcessCalls()
    {
      int count = callCollectors.Count;
      int i;
      if (count > 0) {
        if (count < 5)
          if (count < 3)
            if (count==2) {
              callCollectors.Item0.ExecutePrologue();
              callCollectors.Item1.ExecutePrologue();
            }
            else
              callCollectors.Item0.ExecutePrologue();
          else if (count==4) {
            callCollectors.Item0.ExecutePrologue();
            callCollectors.Item1.ExecutePrologue();
            callCollectors.Item2.ExecutePrologue();
            callCollectors.Item3.ExecutePrologue();
          }
          else {
            callCollectors.Item0.ExecutePrologue();
            callCollectors.Item1.ExecutePrologue();
            callCollectors.Item2.ExecutePrologue();
          }
        else
          for (i = 0; i < count; i++)
            callCollectors[i].ExecutePrologue();
      }

      if (count > 0) {
        if (count < 5)
          if (count < 3)
            if (count==2) {
              callCollectors.Item0.ExecuteOperation();
              callCollectors.Item1.ExecuteOperation();
            }
            else
              callCollectors.Item0.ExecuteOperation();
          else if (count==4) {
            callCollectors.Item0.ExecuteOperation();
            callCollectors.Item1.ExecuteOperation();
            callCollectors.Item2.ExecuteOperation();
            callCollectors.Item3.ExecuteOperation();
          }
          else {
            callCollectors.Item0.ExecuteOperation();
            callCollectors.Item1.ExecuteOperation();
            callCollectors.Item2.ExecuteOperation();
          }
        else
          for (i = 0; i < count; i++)
            callCollectors[i].ExecuteOperation();
      }

      if (count > 0) {
        if (count < 5)
          if (count < 3)
            if (count==2) {
              callCollectors.Item0.ExecuteEpilogue();
              callCollectors.Item1.ExecuteEpilogue();
            }
            else
              callCollectors.Item0.ExecuteEpilogue();
          else if (count==4) {
            callCollectors.Item0.ExecuteEpilogue();
            callCollectors.Item1.ExecuteEpilogue();
            callCollectors.Item2.ExecuteEpilogue();
            callCollectors.Item3.ExecuteEpilogue();
          }
          else {
            callCollectors.Item0.ExecuteEpilogue();
            callCollectors.Item1.ExecuteEpilogue();
            callCollectors.Item2.ExecuteEpilogue();
          }
        else
          for (i = 0; i < count; i++)
            callCollectors[i].ExecuteEpilogue();
      }
    }

    public bool BeginOperation()
    {
      if (executionContextState==ExecutionContextState.Prepare)
        return false;
      else if (executionContextState==ExecutionContextState.Execute) {
        if (next!=null) {
          executionContextSwitcher.CurrentContext = next;
          return true;
        }
        else {
          next = new ExecutionContext(executionContextSwitcher, ExecutionContextState.Prepare, this);
          executionContextSwitcher.CurrentContext = next;
          return true;
        }
      }
      executionContextState = ExecutionContextState.Prepare;
      return true;
    }

    public void EndOperation()
    {
      int count = callCollectors.Count;
      try {
        executionContextState = ExecutionContextState.Execute;
        if (count > 0) {
          if (count < 5)
            if (count < 3)
              if (count==2) {
                callCollectors.Item0.OnBeforeExecute();
                callCollectors.Item1.OnBeforeExecute();
              }
              else
                callCollectors.Item0.OnBeforeExecute();
            else if (count==4) {
              callCollectors.Item0.OnBeforeExecute();
              callCollectors.Item1.OnBeforeExecute();
              callCollectors.Item2.OnBeforeExecute();
              callCollectors.Item3.OnBeforeExecute();
            }
            else {
              callCollectors.Item0.OnBeforeExecute();
              callCollectors.Item1.OnBeforeExecute();
              callCollectors.Item2.OnBeforeExecute();
            }
          else
            for (int i = 0; i < count; i++)
              callCollectors[i].OnBeforeExecute();
          ProcessCalls();
        }
      }
      finally {
        if (count > 0) {
          if (count < 5)
            if (count < 3)
              if (count==2) {
                callCollectors.Item0.OnAfterExecute();
                callCollectors.Item1.OnAfterExecute();
              }
              else
                callCollectors.Item0.OnAfterExecute();
            else if (count==4) {
              callCollectors.Item0.OnAfterExecute();
              callCollectors.Item1.OnAfterExecute();
              callCollectors.Item2.OnAfterExecute();
              callCollectors.Item3.OnAfterExecute();
            }
            else {
              callCollectors.Item0.OnAfterExecute();
              callCollectors.Item1.OnAfterExecute();
              callCollectors.Item2.OnAfterExecute();
            }
          else
            for (int i = 0; i < count; i++)
              callCollectors[i].OnAfterExecute();
          callCollectors.Clear();
        }
        executionContextState = ExecutionContextState.Idle;
        executionContextSwitcher.CurrentContext = previous;
      }
    }

    public ExecutionContext(ExecutionContextSwitcher executionContextSwitcher,
      ExecutionContextState executionContextState,
      ExecutionContext previous)
    {
      this.executionContextSwitcher = executionContextSwitcher;
      this.executionContextState = executionContextState;
      this.previous = previous;
    }

    public ExecutionContext(ExecutionContextSwitcher executionContextSwitcher,
      ExecutionContextState executionContextState)
    {
      this.executionContextSwitcher = executionContextSwitcher;
      this.executionContextState = executionContextState;
      previous = this;
    }
  }
}