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

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Secondary index.
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
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);

        var secondaryKeyColumns = new List<ColumnInfo>(KeyColumns.Select(valueRef => valueRef.Value));

        // Empty keys.
        if (secondaryKeyColumns.Count==0)
          ea.Execute(() => { throw new IntegrityException(Resources.Strings.ExEmptyKeyColumnsCollection, Path); });

        // Double keys.
        foreach (var column in secondaryKeyColumns
          .GroupBy(keyColumn => keyColumn).Where(group => group.Count() > 1)
          .Select(group => group.Key))
          ea.Execute(() => {
            throw new IntegrityException(
              string.Format(Resources.Strings.ExMoreThenOneKeyReferenceToColumnX, column.Name),
              Path);
          });
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