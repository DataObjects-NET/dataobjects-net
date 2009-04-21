// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Indexing;
using IndexUpdateCommand = Xtensive.Storage.Providers.Index.IndexUpdateCommand;

namespace Xtensive.Storage.Providers.Memory
{
  public class SessionHandler : Index.SessionHandler
  {
    /// <inheritdoc/>
    protected override void Insert(EntityState state)
    {
      var batch = new List<Command>();
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var transform = ((DomainHandler) Handlers.DomainHandler).GetTransform(index, state.Type);
        var transformed = transform.Apply(TupleTransformType.Tuple, state.Tuple).ToFastReadOnly();
        batch.Add(IndexUpdateCommand.Insert(index.MappingName, state.Key.Value, transformed));
      }

      StorageView.Execute(batch);
    }

    /// <inheritdoc/>
    protected override void Update(EntityState state)
    {
      var batch = new List<Command>();
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var transform = ((DomainHandler) Handlers.DomainHandler).GetTransform(index, state.Type);
        var transformed = transform.Apply(TupleTransformType.Tuple, state.Tuple).ToFastReadOnly();
        batch.Add(IndexUpdateCommand.Update(index.MappingName, state.Key.Value, transformed));
      }

      StorageView.Execute(batch);
    }

    /// <inheritdoc/>
    protected override void Remove(EntityState state)
    {
      var batch = new List<Command>();
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary))
        batch.Add(IndexUpdateCommand.Remove(index.MappingName, state.Key.Value));

      StorageView.Execute(batch);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      // TODO: Think what should be done here.
    }
  }
}