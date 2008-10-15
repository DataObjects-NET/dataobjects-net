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
    private readonly List<Action<Entity, Entity>> actions = new List<Action<Entity, Entity>>(3);
    private readonly List<Pair<Entity, Entity>> arguments = new List<Pair<Entity, Entity>>(3);
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
      action(args.First, args.Second);
    }

    public void RegisterAction(Action<Entity, Entity> action, Entity arg0, Entity arg1)
    {
      actions.Add(action);
      arguments.Add(new Pair<Entity, Entity>(arg0, arg1));
    }

    public void RegisterParticipant(Entity entity, AssociationInfo association)
    {
      participants.Add(new Pair<Entity, AssociationInfo>(entity, association));
    }
  }
}