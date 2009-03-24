// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single primary index.
  /// </summary>
  [Serializable]
  public class PrimaryIndexInfo : NodeBase<StorageInfo>
  {
    /// <summary>
    /// Gets all columns belongs to the index.
    /// </summary>
    [Property]
    public ColumnInfoCollection Columns { get; private set; }

    /// <summary>
    /// Gets key columns belongs to the index.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<PrimaryIndexInfo> KeyColumns { get; private set; }

    /// <summary>
    /// Gets value columns belongs to the index.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<PrimaryIndexInfo> ValueColumns { get; private set; }

    /// <summary>
    /// Gets secondary indexes.
    /// </summary>
    [Property]
    public SecondaryIndexInfoCollection SecondaryIndexes { get; private set; }

    /// <summary>
    /// Gets foreign keys.
    /// </summary>
    [Property]
    public ForeignKeyCollection ForeignKeys { get; private set; }

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
      ForeignKeys = new ForeignKeyCollection(this, "ForeignKeys");
    }

    /// <inheritdoc/>
    /// <exception cref="IntegrityException">Validation error.</exception>
    protected override void ValidateState()
    {
      base.ValidateState();

      var keys = new List<ColumnInfo>(KeyColumns.Select(keyRef => keyRef.Value));
      var values = new List<ColumnInfo>(ValueColumns.Select(valueRef => valueRef.Value));

      // Empty keys.
      if (keys.Count==0)
        throw new IntegrityException("Empty key columns collection.", Path);

      // Double column reference.
      foreach (var column in keys.Intersect(values)) {
        throw new IntegrityException(
          string.Format("Column '{0}' contains in both key and value collections.", column.Name),
          Path);
      }

      // Not referenced columns.
      foreach (var column in Columns.Except(keys.Union(values))) {
        throw new IntegrityException(
          string.Format("Can not find reference to column '{0}'.", column.Name),
          Path);
      }

      // Double keys.
      foreach (var column in keys.GroupBy(key => key).Where(group => group.Count() > 1).Select(group => group.Key)) {
        throw new IntegrityException(
          string.Format("Key columns collection contains more then one reference to column '{0}'.", column.Name),
          Path);
      }

      // Key columns contains refernce to column from another index.
      foreach (var column in keys.Except(Columns)) {
        throw new IntegrityException(
          string.Format("Referenced column '{0}' does not belong to index '{1}'.", column.Name, Name),
          Path);
      }

      // Double values.
      foreach (var column in values.GroupBy(value => value).Where(group => group.Count() > 1).Select(group => group.Key)) {
        throw new IntegrityException(
          string.Format("Value columns collection contains more then one reference to column '{0}'.", column.Name),
          Path);
      }

      // Value columns contains refernce to column from another index.
      foreach (var column in values.Except(Columns)) {
        throw new IntegrityException(
          string.Format("Referenced column '{0}' does not belong to index '{1}'.", column.Name, Name),
          Path);
      }
    }


    // Constructors

    /// <inheritdoc/>
    public PrimaryIndexInfo(StorageInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}