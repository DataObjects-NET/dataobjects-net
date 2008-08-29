// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.28

using System;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlRequestBuilder
  {
    public DomainHandler DomainHandler;

    public SqlModificationRequest BuildInsertRequest(SqlRequestBuilderTask task)
    {
      var context = new SqlRequestBuilderContext(task, SqlFactory.Batch());
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.GetTable(index));
        SqlInsert query = SqlFactory.Insert(table);
        int i = 0;
        foreach (ColumnInfo column in index.Columns) {
          int offset = context.GetOffsetFor(column);
          SqlParameter p = context.GetParameterFor(column);
          query.Values[table[i++]] = p;
          context.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
        }
        context.Batch.Add(query);
      }
      return BuildRequestFrom(context);
    }

    public SqlModificationRequest BuildUpdateRequest(SqlRequestBuilderTask task)
    {
      var context = new SqlRequestBuilderContext(task, SqlFactory.Batch());
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.GetTable(index));
        SqlUpdate query = SqlFactory.Update(table);
        int i = 0;
        foreach (ColumnInfo column in index.Columns) {
          int offset = context.GetOffsetFor(column);
          if (!task.FieldMap[offset]) {
            i++;
            continue;
          }
          SqlParameter p = context.GetParameterFor(column);
          query.Values[table[i++]] = p;
          context.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
        }
        // There is nothing to update in this table, skipping it
        if (query.Values.Count == 0)
          continue;
        query.Where &= BuildWhereExpression(context, table);
        context.Batch.Add(query);
      }
      return BuildRequestFrom(context);
    }

    public SqlModificationRequest BuildRemoveRequest(SqlRequestBuilderTask task)
    {
      var context = new SqlRequestBuilderContext(task, SqlFactory.Batch());
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.GetTable(index));
        SqlDelete query = SqlFactory.Delete(table);
        query.Where &= BuildWhereExpression(context, table);
        context.Batch.Add(query);
      }
      return BuildRequestFrom(context);
    }

    private static SqlExpression BuildWhereExpression(SqlRequestBuilderContext context, SqlTableRef table)
    {
      SqlExpression result = null;
      int i = 0;
      foreach (ColumnInfo column in context.PrimaryIndex.KeyColumns.Keys) {
        int offset = context.GetOffsetFor(column);
        SqlParameter p = context.GetParameterFor(column);
        result &= table[i++]==p;
        context.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
      }
      return result;
    }

    private static Func<Tuple, object> CreateTupleFieldAccessor(int fieldIndex)
    {
      return (target => target.IsNull(fieldIndex) ? DBNull.Value : target.GetValue(fieldIndex));
    }

    private SqlModificationRequest BuildRequestFrom(SqlRequestBuilderContext context)
    {
      SqlModificationRequest request = new SqlModificationRequest(context.Batch);
      foreach (var binding in context.ParameterBindings)
        request.ParameterBindings[binding.Key] = binding.Value;
      request.ExpectedResult = context.Batch.Count;
      DomainHandler.Compile(request);
      return request;
    }


    // Constructor

    public SqlRequestBuilder(DomainHandler domainHandler)
    {
      DomainHandler = domainHandler;
    }
  }
}