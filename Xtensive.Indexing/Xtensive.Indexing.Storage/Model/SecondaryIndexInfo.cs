// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single secondary index.
  /// </summary>
  [Serializable]
  public class SecondaryIndexInfo : NodeBase<PrimaryIndexInfo>
  {
    private bool isUnique;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="SecondaryIndexInfo"/> is unique.
    /// </summary>
    /// <value>
    /// 	<see langword="true"/> if unique; otherwise, <see langword="false"/>.
    /// </value>
    [Property]
    public bool IsUnique
    {
      get { return isUnique; }
      set
      {
        EnsureIsEditable();
        isUnique = value;
      }
    }

    /// <summary>
    /// Gets or sets the secondary key columns.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<SecondaryIndexInfo> SecondaryKeyColumns { get; private set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<SecondaryIndexInfo, PrimaryIndexInfo, SecondaryIndexInfoCollection>(this, "SecondaryIndexes");
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      SecondaryKeyColumns = new ColumnInfoRefCollection<SecondaryIndexInfo>(this, "SecondaryKeyColumns");
    }

    /// <inheritdoc/>
    /// <exception cref="IntegrityException">Empty secondary key columns collection.</exception>
    protected override void ValidateState()
    {
      base.ValidateState();

      var primaryValueColumns = new List<ColumnInfo>(Parent.ValueColumns.Select(valueRef => valueRef.Value));
      var secondaryKeyColumns = new List<ColumnInfo>(SecondaryKeyColumns.Select(valueRef => valueRef.Value));

      // Empty keys.
      if (secondaryKeyColumns.Count==0)
        throw new IntegrityException("Empty secondary key columns collection.", Path);

      // Double keys.
      foreach (var column in secondaryKeyColumns.GroupBy(keyColumn => keyColumn).Where(group => group.Count() > 1).Select(group => group.Key)) {
        throw new IntegrityException(
          string.Format("Secondary key columns collection contains more then one reference to column '{0}'.", column.Name),
          Path);
      }

      // Secondary key column does not primary value column.
      foreach (var column in secondaryKeyColumns.Except(primaryValueColumns)) {
        throw new IntegrityException(
          string.Format("Secondary key column '{0}' must be primary value column.", column.Name),
          Path);
      }
    }


    // Constructors

    /// <inheritdoc/>
    public SecondaryIndexInfo(PrimaryIndexInfo primaryIndex, string name)
      : base(primaryIndex, name)
    {
    }
  }
}