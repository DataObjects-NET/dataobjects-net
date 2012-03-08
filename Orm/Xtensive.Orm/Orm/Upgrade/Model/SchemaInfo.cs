// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Schema info.
  /// </summary>
  [Serializable]
  public sealed class SchemaInfo : NodeBase<StorageModel>
  {
    /// <summary>
    /// Gets tables.
    /// </summary>
    [Property(Priority = 0)]
    public TableInfoCollection Tables { get; private set; }

    /// <summary>
    /// Gets sequences.
    /// </summary>
    [Property]
    public SequenceInfoCollection Sequences { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      if (Tables==null)
        Tables = new TableInfoCollection(this);
      if (Sequences==null)
        Sequences = new SequenceInfoCollection(this);
    }

    protected override Nesting CreateNesting()
    {
      return new Nesting<SchemaInfo, StorageModel, SchemaInfoCollection>(this, "Schemas");
    }

    public SchemaInfo(StorageModel parent, string name)
      : base(parent, name)
    {
    }
  }
}