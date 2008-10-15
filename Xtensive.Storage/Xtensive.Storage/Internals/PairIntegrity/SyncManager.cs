// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  internal static class SyncManager
  {
    public static void Enlist(OperationType type, Entity owner, Entity value, AssociationInfo association)
    {
      var stack = owner.Session.Transaction.PairSyncContextStack;
      SyncContext context = null;
      if (stack.Count > 0)
        context = stack.Peek();

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
          slave1 = masterActions.GetPairedValue(master1);
        if (slave2 != null && slaveActions.GetPairedValue != null)
          master2 = slaveActions.GetPairedValue(slave2);

        context = new SyncContext();
        stack.Push(context);

        switch (type) {
        case OperationType.Add:
        case OperationType.Set:
          // Breaking existing associations
          if (master2!=null) {
            context.RegisterAction(masterActions.BreakAssociation, master2, slave2);
            context.RegisterParticipant(master2, association);
          }
          if (slave1!=null) {
            context.RegisterAction(slaveActions.BreakAssociation, slave1, master1);
            context.RegisterParticipant(slave1, association.Reversed);
          }
          // Setting new association value for slave
          if (slave2!=null) {
            context.RegisterAction(slaveActions.CreateAssociation, slave2, master1);
            context.RegisterParticipant(slave2, association.Reversed);
          }
          break;
        case OperationType.Remove:
          context.RegisterAction(slaveActions.BreakAssociation, slave2, master1);
          context.RegisterParticipant(slave2, association.Reversed);
          break;
        default:
          throw new ArgumentOutOfRangeException();
        }

        context.ExecuteNextAction();
        stack.Pop();
      }
    }
  }
}