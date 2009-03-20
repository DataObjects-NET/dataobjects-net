// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single primary index.
  /// </summary>
  [Serializable]
  public class PrimaryIndexInfo:NodeBase<StorageInfo>
  {
    /// <summary>
    /// Gets or sets all columns belongs to the index.
    /// </summary>
    [Property]
    public ColumnInfoCollection Columns { get; private set; }

    /// <summary>
    /// Gets or sets the key columns belongs to the index.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<PrimaryIndexInfo> KeyColumns { get; private set; }

    /// <summary>
    /// Gets or sets the value columns belongs to the index.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<PrimaryIndexInfo> ValueColumns { get; private set; }

    /// <summary>
    /// Gets or sets the secondary indexes.
    /// </summary>
    [Property]
    public SecondaryIndexInfoCollection SecondaryIndexes { get; private set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<PrimaryIndexInfo, StorageInfo, PrimaryIndexInfoCollection>(this, "PrimaryIndexes");
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      KeyColumns = new ColumnInfoRefCollection<PrimaryIndexInfo>(this, "KeyColumns");
      ValueColumns = new ColumnInfoRefCollection<PrimaryIndexInfo>(this, "ValueColumns");
      Columns = new ColumnInfoCollection(this);
      SecondaryIndexes = new SecondaryIndexInfoCollection(this);
    }

    /// <inheritdoc/>
    protected override void ValidateState()
    {
      base.ValidateState();
      if(KeyColumns.Count == 0)
        throw new Exception("Empty key columns collection.");
      if (KeyColumns
        .Where(keyRef => null!=ValueColumns.SingleOrDefault(valueRef => keyRef.Value==valueRef.Value))
        .Count() != 0) {
        throw new Exception("");
      }

    }


    //Constructors

    public PrimaryIndexInfo(StorageInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}