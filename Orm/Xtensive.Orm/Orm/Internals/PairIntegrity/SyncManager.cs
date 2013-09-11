// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using System;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.ReferentialIntegrity;

namespace Xtensive.Orm.PairIntegrity
{
  internal class SyncManager : SessionBound
  {
    public void ProcessRecursively(SyncContext context, RemovalContext removalContext, 
      OperationType type, AssociationInfo association, Entity owner, Entity target,
      Action finalizer)
    {
      if (context==null) {
        // We must create a new context
        context = CreateContext(removalContext, type, association, owner, target);
        context.ProcessPendingActionsRecursively(finalizer);
      }
      else {
        // If we are here, provided operation is meaningless -
        // it is passed just because we entered into this method once more
        // after executing N-th operation in context. So it's just right
        // time to execute the next one.
        context.ProcessPendingActionsRecursively(finalizer);
      }
    }

    private SyncContext CreateContext(RemovalContext removalContext, 
      OperationType type, AssociationInfo association, Entity owner, Entity target)
    {
      SyncActionSet masterActions = GetSyncActions(association);
      SyncActionSet slaveActions = GetSyncActions(association.Reversed);
      Entity master1 = owner;
      Entity slave2 = target;
      Entity slave1 = null;
      Entity master2 = null;

      if (masterActions.GetValue!=null)
        slave1 = (Entity) masterActions.GetValue(association, master1);
      if (slave2!=null && slaveActions.GetValue!=null)
        master2 = (Entity) slaveActions.GetValue(association.Reversed, slave2);

      var context = new SyncContext(removalContext);
      switch (type) {
      case OperationType.Add:
      case OperationType.Set:
        // Setting new value for slave
        if (slave2!=null && !(association.IsLoop && master1==slave2))
          context.Enqueue(new SyncAction(slaveActions.Create, association.Reversed, slave2, master1));

        // Breaking existing associations
        if (master2!=null)
          context.Enqueue(new SyncAction(masterActions.Break, association, master2, slave2));
        if (slave1!=null && slave1!=slave2)
          context.Enqueue(new SyncAction(slaveActions.Break, association.Reversed, slave1, master1));
        break;
      case OperationType.Remove:
        var currentRemovalContext = Session.RemovalProcessor.Context;
        var isNotYetRemoved = currentRemovalContext==null || !currentRemovalContext.Contains(slave2);
        if ((!(association.IsLoop && master1==slave2)) && isNotYetRemoved)
          context.Enqueue(new SyncAction(slaveActions.Break, association.Reversed, slave2, master1));
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }
      return context;
    }

    private static SyncActionSet GetSyncActions(AssociationInfo association)
    {
      Func<AssociationInfo, IEntity, IEntity> getValue = null;
      Action<AssociationInfo, IEntity, IEntity> @break = null;
      Action<AssociationInfo, IEntity, IEntity> create = null;

      switch (association.Multiplicity) {
      case Multiplicity.OneToOne:
      case Multiplicity.ManyToOne:
        return new SyncActionSet(
          ReferentialActions.GetReference,
          ReferentialActions.ClearReference,
          ReferentialActions.SetReference);
      case Multiplicity.ManyToMany:
      case Multiplicity.OneToMany:
        return new SyncActionSet(
          null,
          ReferentialActions.RemoveReference,
          ReferentialActions.AddReference);
      default:
        throw new ArgumentOutOfRangeException("association");
      }
    }


    // Constructors

    public SyncManager(Session session)
      : base(session)
    {
    }
  }
}