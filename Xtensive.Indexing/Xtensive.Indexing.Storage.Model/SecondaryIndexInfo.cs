// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single secondary index.
  /// </summary>
  [Serializable]
  public class SecondaryIndexInfo : IndexInfo
  {

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<SecondaryIndexInfo, TableInfo, SecondaryIndexInfoCollection>(this, "SecondaryIndexes");
    }


    /// <inheritdoc/>
    /// <exception cref="IntegrityException">Empty secondary key columns collection.</exception>
    protected override void ValidateState()
    {
      base.ValidateState();

      var primaryValueColumns = new List<ColumnInfo>(Parent.PrimaryIndex.ValueColumns.Select(valueRef => valueRef.Value));
      var secondaryKeyColumns = new List<ColumnInfo>(KeyColumns.Select(valueRef => valueRef.Value));

      // Empty keys.
      if (secondaryKeyColumns.Count == 0)
        throw new IntegrityException(Resources.Strings.ExEmptyKeyColumnsCollection, Path);

      // Double keys.
      foreach (var column in secondaryKeyColumns.GroupBy(keyColumn => keyColumn).Where(group => group.Count() > 1).Select(group => group.Key))
      {
        throw new IntegrityException(
          string.Format(Resources.Strings.ExMoreThenOneKeyReferenceToColumnX, column.Name),
          Path);
      }

      // Secondary key column does not primary value column.
      foreach (var column in secondaryKeyColumns.Except(primaryValueColumns))
      {
        throw new IntegrityException(
          string.Format("Secondary key column '{0}' must be primary value column.", column.Name),
          Path);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The index name.</param>
    public SecondaryIndexInfo(TableInfo table, string name)
      : base(table, name)
    {
    }
  }
}