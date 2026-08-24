// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
        _ = new ValueColumnRef(this, column);
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Validation error.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        base.ValidateState();

        var tableColumns = Parent.Columns;
        var keys = KeyColumns.Select(static keyRef => keyRef.Value).ToList();

        if (keys.Count == 0) {
          ea.Add(new ValidationException(Strings.ExEmptyKeyColumnsCollection, Path), handle: true);
        }
        if (keys.Count(static ci => ci.Type is null || ci.Type.IsNullable) > 0) {
          ea.Add(new ValidationException(Strings.ExPrimaryKeyColumnCanNotBeNullable, Path), handle: true);
        }

        var values = ValueColumns.Select(static valueRef => valueRef.Value).ToList();
        var all = keys.Concat(values).ToList();

        if (all.Count!=tableColumns.Count) {
          ea.Add(new ValidationException(Strings.ExInvalidPrimaryKeyStructure, Path), handle: true);
        }

        if (all.Zip(tableColumns, static (column, tableColumn) => new Pair<StorageColumnInfo>(column, tableColumn)).Any(static p => p.First!=p.Second)) {
          ea.Add(new ValidationException(Strings.ExInvalidPrimaryKeyStructure, Path), handle: true);
        }

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
      if (ValueColumns is null)
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