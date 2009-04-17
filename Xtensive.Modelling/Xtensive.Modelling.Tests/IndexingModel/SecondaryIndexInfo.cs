// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Tests.IndexingModel.Resources;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Secondary index.
  /// </summary>
  [Serializable]
  public sealed class SecondaryIndexInfo : IndexInfo
  {
    /// <summary>
    /// Gets value columns.
    /// </summary>
    [Property]
    public PrimaryKeyColumnRefCollection PrimaryKeyColumns { get; private set; }

    /// <summary>
    /// Gets included columns.
    /// </summary>
    [Property]
    public IncludedColumnRefCollection IncludedColumns { get; private set; }

    /// <summary>
    /// Populates <see cref="PrimaryKeyColumns"/> collection by
    /// copying them from primary index.
    /// </summary>
    public void PopulatePrimaryKeyColumns()
    {
      foreach (var kcr in Parent.PrimaryIndex.KeyColumns)
        new PrimaryKeyColumnRef(this, kcr.Value, kcr.Direction);
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Empty secondary key columns collection.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);

        // Secondary key columns: empty set, duplicates
        var keyColumns = new List<ColumnInfo>(KeyColumns.Select(valueRef => valueRef.Value));
        if (keyColumns.Count==0)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExEmptyKeyColumnsCollection, Path);
          });
        foreach (var group in keyColumns
          .GroupBy(keyColumn => keyColumn)
          .Where(group => group.Count() > 1))
          ea.Execute((_column) => {
            throw new ValidationException(
              string.Format(Strings.ExMoreThenOneKeyColumnReferenceToColumnX, _column.Name),
              Path);
          }, group.Key);

        // Primary key columns
        if (PrimaryKeyColumns.Count!=Parent.PrimaryIndex.KeyColumns.Count)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExInvalidPrimaryKeyColumnsCollection, Path);
          });
        for (int i = 0; i < PrimaryKeyColumns.Count; i++) {
          var ref1 = PrimaryKeyColumns[i];
          var ref2 = Parent.PrimaryIndex.KeyColumns[i];
          if (ref1.Value!=ref2.Value || ref1.Direction!=ref2.Direction)
            ea.Execute(() => {
              throw new ValidationException(Strings.ExInvalidPrimaryKeyColumnsCollection, Path);
            });
        }

        // Included columns
        var allKeyColumns = new HashSet<ColumnInfo>(
          KeyColumns.Select(cr => cr.Value)
            .Union(PrimaryKeyColumns.Select(cr => cr.Value)));
        foreach (var columnRef in IncludedColumns)
          if (allKeyColumns.Contains(columnRef.Value))
            ea.Execute(() => {
              throw new ValidationException(Strings.ExInvalidIncludedColumnsCollection, Path);
            });
        foreach (var group in IncludedColumns
          .GroupBy(keyColumn => keyColumn)
          .Where(group => group.Count() > 1))
          ea.Execute((_column) => {
            throw new ValidationException(
              string.Format(Strings.ExMoreThenOneIncludedColumnReferenceToColumnX, _column.Name),
              Path);
          }, group.Key);
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<SecondaryIndexInfo, TableInfo, SecondaryIndexInfoCollection>(this, "SecondaryIndexes");
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      if (PrimaryKeyColumns==null)
        PrimaryKeyColumns = new PrimaryKeyColumnRefCollection(this);
      if (IncludedColumns==null)
        IncludedColumns = new IncludedColumnRefCollection(this);
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