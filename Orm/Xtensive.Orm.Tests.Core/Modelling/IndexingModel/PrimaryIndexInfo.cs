// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Linq;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
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
    [Property(IgnoreInComparison = true)]
    public ValueColumnRefCollection ValueColumns { get; private set; }

    /// <summary>
    /// Populates <see cref="ValueColumns"/> collection by
    /// including all the columns except <see cref="IndexInfo.KeyColumns"/>
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
      using (var ea = new Xtensive.Core.ExceptionAggregator()) {
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
        if (keys.Where(ci => ci.Type.IsNullable).Count() > 0)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExPrimaryKeyColumnCanNotBeNullable, Path);
          });

        if (all.Count!=tableColumns.Count)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExInvalidPrimaryKeyStructure, Path);
          });
        if (all.Zip(tableColumns, (first, second) => new {First = first, Second = second }).Where(p => p.First!=p.Second).Any())
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
    public PrimaryIndexInfo(TableInfo table, string name)
      : base(table, name)
    {
    }
  }
}
