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
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Tests.IndexingModel.Resources;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Primary index.
  /// </summary>
  [Serializable]
  public sealed class PrimaryIndexInfo : IndexInfo
  {
    /// <summary>
    /// Gets value columns.
    /// </summary>
    [Property]
    public ValueColumnRefCollection ValueColumns { get; private set; }

    /// <summary>
    /// Populates <see cref="ValueColumns"/> collection by
    /// including all the columns except <see cref="IndexInfo.KeyColumns"/>
    /// into it.
    /// </summary>
    public void PopulateValueColumns()
    {
      var keyColumns = new HashSet<ColumnInfo>(KeyColumns.Select(kc => kc.Value));
      foreach (var column in Parent.Columns.Where(c => !keyColumns.Contains(c)))
        new ValueColumnRef(this, column);
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Validation error.</exception>
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
            throw new ValidationException(Strings.ExEmptyKeyColumnsCollection, Path);
          });

        // Nullable key columns.
        if (keys.Where(ci => ci.Type.IsNullable).Count() > 0)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExPrimaryKeyColumnCanNotBeNullable, Path);
          });

        // Double column reference.
        foreach (var column in keys.Intersect(values))
          ea.Execute((_column) => {
            throw new ValidationException(
              string.Format(Strings.ExColumnXContainsBothKeyAndValueCollections, _column.Name),
              Path);
          }, column);

        // Not referenced columns.
        foreach (var column in columns.Except(keys.Union(values)))
          ea.Execute((_column) => {
            throw new ValidationException(
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
            throw new ValidationException(
              string.Format(Strings.ExMoreThenOneKeyColumnReferenceToColumnX, _column.Name),
              Path);
          }, column);

        // Key columns contains refernce to column from another table.
        foreach (var column in keys.Except(columns))
          ea.Execute((_column) => {
            throw new ValidationException(
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
            throw new ValidationException(
              string.Format(Strings.ExMoreThenOneValueColumnReferenceToColumnX, _column.Name),
              Path);
          }, column);

        // Value columns contains refernce to column from another table.
        foreach (var column in values.Except(columns))
          ea.Execute((_column) => {
            throw new ValidationException(
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

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      if (ValueColumns==null)
        ValueColumns = new ValueColumnRefCollection(this);
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