// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class DataTableComparisonResult : NodeComparisonResult,
    IComparisonResult<DataTable>
  {
    /// <inheritdoc/>
    public new DataTable NewValue
    {
      get { return (DataTable)base.NewValue; }
    }

    /// <inheritdoc/>
    public new DataTable OriginalValue
    {
      get { return (DataTable)base.OriginalValue; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DataTableComparisonResult(DataTable originalValue, DataTable newValue)
      : base(originalValue, newValue)
    {
    }
  }
}