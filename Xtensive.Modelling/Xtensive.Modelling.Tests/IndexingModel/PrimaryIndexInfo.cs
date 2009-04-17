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
using Xtensive.Modelling.Tests.IndexingModel.Resources;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Primary index.
  /// </summary>
  [Serializable]
  public class PrimaryIndexInfo : IndexInfo
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<PrimaryIndexInfo, TableInfo, PrimaryIndexInfo>(this, "PrimaryIndex");
    }

    /// <inheritdoc/>
    /// <exception cref="IntegrityException">Validation error.</exception>
    protected override void ValidateState()
    {
      base.ValidateState();

      var columns = Parent.Columns;
      var keys = new List<ColumnInfo>(KeyColumns.Select(keyRef => keyRef.Value));
      var values = new List<ColumnInfo>(ValueColumns.Select(valueRef => valueRef.Value));

      // Empty keys.
      if (keys.Count==0)
        throw new IntegrityException(Strings.ExEmptyKeyColumnsCollection, Path);

      // Nullable key columns.
      if (keys.Where(ci=>ci.IsNullable).Count() > 0)
        throw new IntegrityException(Strings.ExPrimaryKeyColumnCanNotBeNullable, Path);
      
      // Double column reference.
      foreach (var column in keys.Intersect(values))
        throw new IntegrityException(
          string.Format(Strings.ExColumnXContainsBothKeyAndValueCollections, column.Name),
          Path);

      // Not referenced columns.
      foreach (var column in columns.Except(keys.Union(values)))
        throw new IntegrityException(
          string.Format(Strings.ExCanNotFindReferenceToColumnX, column.Name),
          Path);

      // Double keys.
      foreach (var column in 
        keys
          .GroupBy(keyColumn => keyColumn)
          .Where(group => group.Count() > 1)
          .Select(group => group.Key))
        throw new IntegrityException(
          string.Format(Strings.ExMoreThenOneKeyReferenceToColumnX, column.Name),
          Path);

      // Key columns contains refernce to column from another table.
      foreach (var column in keys.Except(columns))
        throw new IntegrityException(
          string.Format(Strings.ExReferencedColumnXDoesNotBelongToIndexY,
            column.Name, Name),
          Path);

      // Double values.
      foreach (var column in
        values
          .GroupBy(valueColumn => valueColumn)
          .Where(group => group.Count() > 1)
          .Select(group => group.Key))
        throw new IntegrityException(
          string.Format(Strings.ExMoreThenOneValueReferenceToColumnX, column.Name),
          Path);
      

      // Value columns contains refernce to column from another table.
      foreach (var column in values.Except(columns))
        throw new IntegrityException(
          string.Format(Strings.ExReferencedColumnXDoesNotBelongToIndexY, column.Name, Name),
          Path);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The index name.</param>
    public PrimaryIndexInfo(TableInfo table, string name)
      : base(table, name)
    {
    }
  }
}