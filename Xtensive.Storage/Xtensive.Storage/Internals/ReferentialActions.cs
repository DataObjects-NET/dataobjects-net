// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.15

using System;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.ReferentialIntegrity;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal static class ReferentialActions
  {
    public static readonly Func<AssociationInfo, IEntity, IEntity> GetReference = OnGetReference;

    public static readonly Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> ClearReference = OnClearReference;

    public static readonly Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> SetReference = OnSetReference;

    public static readonly Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> AddReference = OnAddReference;

    public static readonly Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> RemoveReference = OnRemoveReference;

    #region Implementation

    private static IEntity OnGetReference(AssociationInfo association, IEntity owner)
    {
      return (IEntity) ((Entity) owner).GetFieldValue(association.OwnerField);
    }

    private static void OnClearReference(AssociationInfo association, IEntity owner, IEntity target, SyncContext syncContext, RemovalContext removalContext)
    {
      var nullIsEntity = owner as IHasNullEntity;
      var nullValue = nullIsEntity==null ? null : nullIsEntity.NullEntity;
      ((Entity) owner).SetFieldValue(association.OwnerField, nullValue, syncContext, removalContext);
    }

    private static void OnSetReference(AssociationInfo association, IEntity owner, IEntity target, SyncContext syncContext, RemovalContext removalContext)
    {
      ((Entity) owner).SetFieldValue(association.OwnerField, target, syncContext, removalContext);
    }

    private static void OnAddReference(AssociationInfo association, IEntity owner, IEntity target, SyncContext syncContext, RemovalContext removalContext)
    {
      ((EntitySetBase) ((Entity) owner).GetFieldValue(association.OwnerField)).Add((Entity) target, syncContext, removalContext);
    }

    private static void OnRemoveReference(AssociationInfo association, IEntity owner, IEntity target, SyncContext syncContext, RemovalContext removalContext)
    {
      ((EntitySetBase) ((Entity) owner).GetFieldValue(association.OwnerField)).Remove((Entity) target, syncContext, removalContext);
    }

    #endregion
  }
}