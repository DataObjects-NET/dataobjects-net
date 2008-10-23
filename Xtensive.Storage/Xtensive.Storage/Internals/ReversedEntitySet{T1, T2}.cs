// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.15

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  internal class ReversedEntitySet<T1, T2> : EntitySet<T1, T2>
    where T1 : Entity
    where T2 : Entity
  {
    /// <inheritdoc/>
    public override bool Add(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Add, OwnerEntity, item, Field.Association);

      OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
      return true;
    }

    /// <inheritdoc/>
    public override bool Remove(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (!Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Remove, OwnerEntity, item, Field.Association);

      OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
      return true;
    }

    public override bool Contains(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Contains(item.Key);
    }

    #region Initialization members

    protected override IndexInfo GetIndex()
    {
      return Field.ReflectedType.Model.Types[typeof (T2)].Indexes.First(indexInfo => indexInfo.IsSecondary);
    }

    protected override MapTransform GetKeyExtractTransform()
    {
      TypeInfoCollection types = Session.Domain.Model.Types;
      var field = types[typeof (T2)].Fields[Session.Domain.NameBuilder.EntitySetItemSlaveFieldName];
      var columns = field.Fields.ExtractColumns();
      var keyTupleDescriptor = types[typeof (T1)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = columns.Select(columnInfo => Index.Columns.First(columnInfo2 => columnInfo2.Name==columnInfo.Name)).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    #endregion


    // Constructors.

    public ReversedEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}