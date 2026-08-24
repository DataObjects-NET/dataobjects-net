// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Secondary index.
  /// </summary>
  public sealed class SecondaryIndexInfo : StorageIndexInfo
  {
    private PartialIndexFilterInfo filter;

    /// <summary>
    /// Gets value columns.
    /// </summary>
    [Property(Priority = -110)]
    public PrimaryKeyColumnRefCollection PrimaryKeyColumns { get; private set; }

    /// <summary>
    /// Gets included columns.
    /// </summary>
    [Property(Priority = -100)]
    public IncludedColumnRefCollection IncludedColumns { get; private set; }

    /// <summary>
    /// Gets filter expression for partial index.
    /// </summary>
    [Property(Priority = -90)]
    public PartialIndexFilterInfo Filter
    {
      get => filter;
      set {
        EnsureIsEditable();
        filter = value;
      }
    }

    /// <summary>
    /// Populates <see cref="PrimaryKeyColumns"/> collection by
    /// copying them from primary index.
    /// </summary>
    public void PopulatePrimaryKeyColumns()
    {
      if (Parent.PrimaryIndex == null)
        return;
      foreach (var kcr in Parent.PrimaryIndex.KeyColumns)
        _ = new PrimaryKeyColumnRef(this, kcr.Value, kcr.Direction);
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Empty secondary key columns collection.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);

        // Secondary key columns: empty set, duplicates
        var keyColumns = KeyColumns.Select(static valueRef => valueRef.Value).ToList();
        if (keyColumns.Count == 0) {
          ea.Add(new ValidationException(Strings.ExEmptyKeyColumnsCollection, Path), handle: true);
        }
        foreach (var group in keyColumns
          .GroupBy(keyColumn => keyColumn)
          .Where(static g => g.Count() > 1)) {

          ea.Add(new ValidationException(string.Format(Strings.ExMoreThenOneKeyColumnReferenceToColumnX, group.Key.Name), Path), handle: true);
        }

        // Primary key columns
        if (Parent.PrimaryIndex != null && PrimaryKeyColumns.Count != Parent.PrimaryIndex.KeyColumns.Count) {
          ea.Add(new ValidationException(Strings.ExInvalidPrimaryKeyColumnsCollection, Path), handle: true);
        }

        for (int i = 0, count = PrimaryKeyColumns.Count; i < count; i++) {
          var ref1 = PrimaryKeyColumns[i];
          var ref2 = Parent.PrimaryIndex.KeyColumns[i];
          if (ref1.Value != ref2.Value || ref1.Direction != ref2.Direction) {
            ea.Add(new ValidationException(Strings.ExInvalidPrimaryKeyColumnsCollection, Path), handle: true);
          }
        }

        // Included columns
        var fullKeySet = 
          KeyColumns
            .Select(cr => cr.Value)
            .Concat(PrimaryKeyColumns.Select(cr => cr.Value))
            .ToHashSet();
        foreach (var columnRef in IncludedColumns) {
          if (fullKeySet.Contains(columnRef.Value)) {
            ea.Add(new ValidationException(Strings.ExInvalidIncludedColumnsCollection, Path), handle: true);
          }
        }

        foreach (var group in IncludedColumns
          .GroupBy(keyColumn => keyColumn)
          .Where(g => g.Count() > 1)) {

          ea.Add(
            new ValidationException(
              string.Format(Strings.ExMoreThenOneIncludedColumnReferenceToColumnX, group.Key.Name), Path),
            handle: true);
        }

        ea.Complete();
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
      if (PrimaryKeyColumns is null)
        PrimaryKeyColumns = new PrimaryKeyColumnRefCollection(this);
      if (IncludedColumns is null)
        IncludedColumns = new IncludedColumnRefCollection(this);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The index name.</param>
    public SecondaryIndexInfo(TableInfo table, string name)
      : base(table, name)
    {
    }
  }
}