// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Orm.ReferentialIntegrity;


namespace Xtensive.Orm.PairIntegrity
{
  [DebuggerDisplay("SyncContext #{Identifier}")]
  internal class SyncContext
  {
    private static volatile int identifier;
    public int Identifier = identifier++;

    private RemovalContext removalContext;
    private List<SyncAction> actions = new List<SyncAction>(3);
    private Stack<Action> finalizers = new Stack<Action>(3);
    private int currentActionIndex;

    public void Enqueue(SyncAction action)
    {
      actions.Add(action);
    }

    public void ProcessPendingActionsRecursively(Action finalizer)
    {
      finalizers.Push(finalizer);
      if (HasPendingActions()) {
        ExecuteNextPendingAction();
        if (HasPendingActions())
          // Must be empty because of recursion!
          throw Exceptions.InternalError(Strings.LogSyncContextMustHaveNoPendingActions, OrmLog.Instance);
      }
      else {
        while (finalizers.Count > 0) {
          var action = finalizers.Pop();
          if (action!=null)
            action.Invoke();
        }
      }
    }

    private bool HasPendingActions()
    {
      return currentActionIndex < actions.Count;
    }

    private void ExecuteNextPendingAction()
    {
      var action = actions[currentActionIndex++];
      action.Action.Invoke(action.Association, action.Owner, action.Target, this, removalContext);
    }

    
    // Constructors
    
    public SyncContext(RemovalContext removalContext)
    {
      this.removalContext = removalContext;
    }
  }
}