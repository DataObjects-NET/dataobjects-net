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
  internal class SqlRequestBuilderContext
  {
    public SqlBatch Batch { get; private set; }

    public SqlRequestBuilderTask Task { get; private set; }

    public TypeInfo Type { get; private set; }

    public List<IndexInfo> AffectedIndexes { get; private set;}

    public IndexInfo PrimaryIndex { get; private set; }

    public Dictionary<SqlParameter, Func<Tuple, object>> ParameterBindings { get; private set; }

    private readonly Dictionary<ColumnInfo, SqlParameter> parameterMapping = new Dictionary<ColumnInfo, SqlParameter>();

    public SqlParameter GetParameterFor(ColumnInfo column)
    {
      SqlParameter result;
      if (!parameterMapping.TryGetValue(column, out result)) {
        result = new SqlParameter("p" + parameterMapping.Count);
        parameterMapping.Add(column, result);
      }
      return result;
    }

    public int GetOffsetFor(ColumnInfo column)
    {
      return Task.Type.Fields[column.Field.Name].MappingInfo.Offset;
    }

    public SqlRequestBuilderContext(SqlRequestBuilderTask task, SqlBatch batch)
    {
      Task = task;
      Batch = batch;
      Type = task.Type;
      AffectedIndexes = Task.Type.AffectedIndexes.Where(i => i.IsPrimary).ToList();
      PrimaryIndex = Task.Type.Indexes.PrimaryIndex;
      parameterMapping = new Dictionary<ColumnInfo, SqlParameter>();
      ParameterBindings = new Dictionary<SqlParameter, Func<Tuple, object>>();
    }
  }
}