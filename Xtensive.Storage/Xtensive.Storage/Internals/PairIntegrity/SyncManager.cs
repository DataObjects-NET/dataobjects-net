// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using System;
using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  internal class SyncManager : SessionBound
  {
    private readonly Stack<SyncContext> contextStack = new Stack<SyncContext>();

    [Infrastructure]
    public void Enlist(OperationType type, Entity owner, Entity target, AssociationInfo association)
    {
      SyncContext context = null;
      if (contextStack.Count > 0)
        context = contextStack.Peek();

      // Existing & corrrect context
      if (context!=null && context.Contains(owner, association)) {
        if (context.HasNextAction())
          context.ExecuteNextAction();
        return;
      }

      // New context
      using (InconsistentRegion.Open(owner.Session)) {
        SyncActionSet masterActions = GetSyncActions(association);
        SyncActionSet slaveActions = GetSyncActions(association.Reversed);
        Entity master1 = owner;
        Entity slave2 = target;
        Entity slave1 = null;
        Entity master2 = null;
      
        if (masterActions.GetValue!=null)
          slave1 = (Entity) masterActions.GetValue(association, master1);
        if (slave2 != null && slaveActions.GetValue != null)
          master2 = (Entity) slaveActions.GetValue(association.Reversed, slave2);

        context = new SyncContext();
        contextStack.Push(context);

        switch (type) {
        case OperationType.Add:
        case OperationType.Set:
          // Setting new value for slave
            if (slave2!=null && !(association.IsLoop && master1==slave2))
              context.RegisterAction(new SyncAction(slaveActions.Create, association.Reversed, slave2, master1));

          // Breaking existing associations
            if (master2!=null)
              context.RegisterAction(new SyncAction(masterActions.Break, association, master2, slave2));
            if (slave1!=null)
              context.RegisterAction(new SyncAction(slaveActions.Break, association.Reversed, slave1, master1));
            break;
        case OperationType.Remove:
            if (!(association.IsLoop && master1==slave2))
              context.RegisterAction(new SyncAction(slaveActions.Break, association.Reversed, slave2, master1));
            break;
        default:
          throw new ArgumentOutOfRangeException();
        }

        if (context.HasNextAction())
          context.ExecuteNextAction();
        contextStack.Pop();
      }
    }

    private static SyncActionSet GetSyncActions(AssociationInfo association)
    {
      Func<AssociationInfo, IEntity, IEntity> getValue = null;
      Action<AssociationInfo, IEntity, IEntity> @break = null;
      Action<AssociationInfo, IEntity, IEntity> create = null;

      switch (association.Multiplicity) {
      case Multiplicity.OneToOne:
          getValue = AssociationActionProvider.GetReferenceAction;
          @break = AssociationActionProvider.ClearReferenceAction;
          create = AssociationActionProvider.SetReferenceAction;
        break;
      case Multiplicity.OneToMany:
          @break = AssociationActionProvider.RemoveReferenceAction;
          create = AssociationActionProvider.AddReferenceAction;
        break;
      case Multiplicity.ManyToOne:
          getValue = AssociationActionProvider.GetReferenceAction;
          @break = AssociationActionProvider.ClearReferenceAction;
          create = AssociationActionProvider.SetReferenceAction;
        break;
      case Multiplicity.ManyToMany:
          @break = AssociationActionProvider.RemoveReferenceAction;
          create = AssociationActionProvider.AddReferenceAction;
        break;
      }
      return new SyncActionSet(getValue, @break, create);
    }


    // Constructors

    public SyncManager(Session session)
      : base(session)
    {
    }
  }
}