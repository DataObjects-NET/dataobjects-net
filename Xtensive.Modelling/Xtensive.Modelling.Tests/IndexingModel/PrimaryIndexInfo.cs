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
    /// <exception cref="IntegrityException">Validation error.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        base.ValidateState();

        var columns = Parent.Columns;
        var keys = new List<ColumnInfo>(KeyColumns.Select(keyRef => keyRef.Value));
        var values = new List<ColumnInfo>(ValueColumns.Select(valueRef => valueRef.Value));

        // Empty keys.
        if (keys.Count==0)
          ea.Execute(() => {
            throw new IntegrityException(Strings.ExEmptyKeyColumnsCollection, Path);
          });

        // Nullable key columns.
        if (keys.Where(ci => ci.Type.IsNullable).Count() > 0)
          ea.Execute(() => {
            throw new IntegrityException(Strings.ExPrimaryKeyColumnCanNotBeNullable, Path);
          });

        // Double column reference.
        foreach (var column in keys.Intersect(values))
          ea.Execute((_column) => {
            throw new IntegrityException(
              string.Format(Strings.ExColumnXContainsBothKeyAndValueCollections, _column.Name),
              Path);
          }, column);

        // Not referenced columns.
        foreach (var column in columns.Except(keys.Union(values)))
          ea.Execute((_column) => {
            throw new IntegrityException(
              string.Format(Strings.ExCanNotFindReferenceToColumnX, _column.Name),
              Path);
          }, column);

        // Double keys.
        foreach (var column in 
          keys
            .GroupBy(keyColumn => keyColumn)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key))
          ea.Execute((_column) => {
            throw new IntegrityException(
              string.Format(Strings.ExMoreThenOneKeyReferenceToColumnX, _column.Name),
              Path);
          }, column);

        // Key columns contains refernce to column from another table.
        foreach (var column in keys.Except(columns))
          ea.Execute((_column) => {
            throw new IntegrityException(
              string.Format(Strings.ExReferencedColumnXDoesNotBelongToIndexY,
                _column.Name, Name),
              Path);
          }, column);

        // Double values.
        foreach (var column in
          values
            .GroupBy(valueColumn => valueColumn)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key))
          ea.Execute((_column) => {
            throw new IntegrityException(
              string.Format(Strings.ExMoreThenOneValueReferenceToColumnX, _column.Name),
              Path);
          }, column);

        // Value columns contains refernce to column from another table.
        foreach (var column in values.Except(columns))
          ea.Execute((_column) => {
            throw new IntegrityException(
              string.Format(Strings.ExReferencedColumnXDoesNotBelongToIndexY, _column.Name, Name),
              Path);
          }, column);
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<PrimaryIndexInfo, TableInfo, PrimaryIndexInfo>(this, "PrimaryIndex");
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