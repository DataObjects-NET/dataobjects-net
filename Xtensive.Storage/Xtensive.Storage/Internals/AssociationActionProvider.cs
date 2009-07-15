// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.15

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal static class AssociationActionProvider
  {
    public static readonly Func<AssociationInfo, IEntity, IEntity> GetReferenceAction;

    public static readonly Action<AssociationInfo, IEntity, IEntity> ClearReferenceAction;

    public static readonly Action<AssociationInfo, IEntity, IEntity> SetReferenceAction;

    public static readonly Action<AssociationInfo, IEntity, IEntity> AddReferenceAction;

    public static readonly Action<AssociationInfo, IEntity, IEntity> RemoveReferenceAction;

    static AssociationActionProvider()
    {
      GetReferenceAction      = (association, owner) => ((Entity)owner).GetFieldValue<IEntity>(association.OwnerField);
      ClearReferenceAction    = (association, owner, target) => ((Entity)owner).SetFieldValue<IEntity>(association.OwnerField, null);
      SetReferenceAction      = (association, owner, target) => ((Entity)owner).SetFieldValue(association.OwnerField, target);
      AddReferenceAction      = (association, owner, target) => ((Entity)owner).GetFieldValue<EntitySetBase>(association.OwnerField).Add((Entity)target);
      RemoveReferenceAction   = (association, owner, target) => ((Entity)owner).GetFieldValue<EntitySetBase>(association.OwnerField).Remove((Entity)target);
    }
  }
}