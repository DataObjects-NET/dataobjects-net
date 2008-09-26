// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.29

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlRequestBuilderContext
  {
    public SqlBatch Batch { get; private set; }

    public SqlRequestBuilderTask Task { get; private set; }

    public TypeInfo Type { get; private set; }

    public List<IndexInfo> AffectedIndexes { get; private set;}

    public IndexInfo PrimaryIndex { get; private set; }

    public Dictionary<ColumnInfo, SqlUpdateRequestParameter> ParameterBindings { get; private set; }

    internal SqlUpdateRequestParameter GetParameterBinding(ColumnInfo column, Func<Tuple, object> functor)
    {
      SqlUpdateRequestParameter result;
      if (!ParameterBindings.TryGetValue(column, out result)) {
        result = new SqlUpdateRequestParameter(column, functor);
        ParameterBindings.Add(column, result);
      }
      return result;
    }


    public SqlRequestBuilderContext(SqlRequestBuilderTask task, SqlBatch batch)
    {
      Task = task;
      Batch = batch;
      Type = task.Type;
      AffectedIndexes = Task.Type.AffectedIndexes.Where(i => i.IsPrimary).ToList();
      PrimaryIndex = Task.Type.Indexes.PrimaryIndex;
      ParameterBindings = new Dictionary<ColumnInfo, SqlUpdateRequestParameter>();
    }
  }
}