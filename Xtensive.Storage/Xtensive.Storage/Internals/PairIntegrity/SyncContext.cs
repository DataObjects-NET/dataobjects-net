// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  internal class SyncContext
  {
    private readonly List<SyncAction> actions = new List<SyncAction>(3);
    private int actionIndex;

    public bool Contains(Entity entity, AssociationInfo association)
    {
      return IndexOf(entity, association) >= 0;
    }

    private int IndexOf(Entity entity, AssociationInfo association)
    {
      for (int i = 0; i < actions.Count; i++) {
        SyncAction item = actions[i];
        if (item.Owner == entity && item.Association == association)
          return i;
      }
      return -1;
    }

    public bool HasNextAction()
    {
      return actionIndex < actions.Count && actions[actionIndex].Action!=null;
    }

    public void ExecuteNextAction()
    {
      int nextActionIndex = actionIndex++;
      var nextAction = actions[nextActionIndex];
      nextAction.Action(nextAction.Association, nextAction.Owner, nextAction.Target);
    }

    public void RegisterAction(SyncAction action)
    {
      actions.Add(action);
    }
  }
}