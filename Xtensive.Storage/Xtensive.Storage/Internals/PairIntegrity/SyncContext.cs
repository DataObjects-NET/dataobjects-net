// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  internal class SyncContext
  {
    private readonly List<Pair<Entity, AssociationInfo>> participants = new List<Pair<Entity, AssociationInfo>>(3);
    private readonly List<Action<Entity, Entity, bool>> actions = new List<Action<Entity, Entity, bool>>(3);
    private readonly List<Triplet<Entity, Entity, bool>> arguments = new List<Triplet<Entity, Entity, bool>>(3);
    private int stage;

    public bool Contains(Entity entity, AssociationInfo association)
    {
      return IndexOf(entity, association) >= 0;
    }

    public int IndexOf(Entity entity, AssociationInfo association)
    {
      for (int i = 0; i < participants.Count; i++) {
        Pair<Entity, AssociationInfo> item = participants[i];
        if (item.First == entity && item.Second == association)
          return i;
      }
      return -1;
    }

    public bool HasNextAction()
    {
      return stage < actions.Count && actions[stage]!=null;
    }

    public void ExecuteNextAction()
    {
      int current = stage++;
      var action = actions[current];
      var args = arguments[current];
      action(args.First, args.Second, args.Third);
    }

    public void RegisterAction(Action<Entity, Entity, bool> action, Entity arg0, Entity arg1, bool arg2)
    {
      actions.Add(action);
      arguments.Add(new Triplet<Entity, Entity, bool>(arg0, arg1, arg2));
    }

    public void RegisterParticipant(Entity entity, AssociationInfo association)
    {
      participants.Add(new Pair<Entity, AssociationInfo>(entity, association));
    }
  }
}