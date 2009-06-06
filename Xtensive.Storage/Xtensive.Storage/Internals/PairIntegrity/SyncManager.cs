// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using System;
using System.Collections.Generic;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  internal class SyncManager : SessionBound
  {
    private readonly Stack<SyncContext> contextStack = new Stack<SyncContext>();

    public void Enlist(OperationType type, Entity owner, Entity value, AssociationInfo association, bool notify)
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
      using (owner.Session.OpenInconsistentRegion()) {
        ActionSet masterActions = owner.Session.Domain.PairSyncActions[association];
        ActionSet slaveActions = owner.Session.Domain.PairSyncActions[association.Reversed];
        Entity master1 = owner;
        Entity slave2 = value;
        Entity slave1 = null;
        Entity master2 = null;
      
        if (masterActions.GetPairedValue!=null)
          slave1 = (Entity) masterActions.GetPairedValue(master1, notify);
        if (slave2 != null && slaveActions.GetPairedValue != null)
          master2 = (Entity) slaveActions.GetPairedValue(slave2, notify);

        context = new SyncContext();
        contextStack.Push(context);

        switch (type) {
        case OperationType.Add:
        case OperationType.Set:
          // Setting new association value for slave
          if (slave2!=null && !(association.IsLoop && master1 == slave2)) {
            context.RegisterAction(slaveActions.CreateAssociation, slave2, master1, notify);
            context.RegisterParticipant(slave2, association.Reversed);
          }
          // Breaking existing associations
          if (master2!=null) {
            context.RegisterAction(masterActions.BreakAssociation, master2, slave2, notify);
            context.RegisterParticipant(master2, association);
          }
          if (slave1!=null) {
            context.RegisterAction(slaveActions.BreakAssociation, slave1, master1, notify);
            context.RegisterParticipant(slave1, association.Reversed);
          }
          break;
        case OperationType.Remove:
          if (!(association.IsLoop && master1 == slave2)) {
            context.RegisterAction(slaveActions.BreakAssociation, slave2, master1, notify);
            context.RegisterParticipant(slave2, association.Reversed);
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
        }

        if (context.HasNextAction())
          context.ExecuteNextAction();
        contextStack.Pop();
      }
    }


    // Constructors

    public SyncManager(Session session)
      : base(session)
    {
    }
  }
}