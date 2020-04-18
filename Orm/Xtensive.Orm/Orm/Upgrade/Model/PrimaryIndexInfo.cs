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
  /// Primary index.
  /// </summary>
  [Serializable]
  public sealed class PrimaryIndexInfo : StorageIndexInfo
  {
    /// <summary>
    /// Gets value columns.
    /// </summary>
    [Property(Priority = -100, IgnoreInComparison = true)]
    public ValueColumnRefCollection ValueColumns { get; private set; }

    /// <summary>
    /// Populates <see cref="ValueColumns"/> collection by
    /// including all the columns except <see cref="StorageIndexInfo.KeyColumns"/>
    /// into it.
    /// </summary>
    public void PopulateValueColumns()
    {
      var keySet = KeyColumns.Select(kc => kc.Value).ToHashSet();
      foreach (var column in Parent.Columns.Where(c => !keySet.Contains(c)))
        new ValueColumnRef(this, column);
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Validation error.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        base.ValidateState();

        var tableColumns = Parent.Columns;
        var keys = KeyColumns.Select(keyRef => keyRef.Value).ToList();
        var values = ValueColumns.Select(valueRef => valueRef.Value).ToList();
        var all = keys.Concat(values).ToList();

        if (keys.Count==0)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExEmptyKeyColumnsCollection, Path);
          });
        if (keys.Where(ci => ci.Type == null || ci.Type.IsNullable).Count() > 0)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExPrimaryKeyColumnCanNotBeNullable, Path);
          });

        if (all.Count!=tableColumns.Count)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExInvalidPrimaryKeyStructure, Path);
          });
        if (all.Zip(tableColumns, (column, tableColumn) => new Pair<StorageColumnInfo>(column, tableColumn)).Any(p => p.First!=p.Second))
          ea.Execute(() => {
            throw new ValidationException(Strings.ExInvalidPrimaryKeyStructure, Path);
          });

        ea.Complete();
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
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The index name.</param>
    public PrimaryIndexInfo(TableInfo table, string name)
      : base(table, name)
    {
    }
  }
}