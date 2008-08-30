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

    public SqlModificationRequest BuildRequest(SqlRequestBuilderTask task)
    {
      SqlRequestBuilderResult result = null;
      switch (task.Kind) {
      case SqlRequestKind.Insert:
        result = BuildInsertRequest(task);
        break;
      case SqlRequestKind.Remove:
        result = BuildRemoveRequest(task);
        break;
      case SqlRequestKind.Update:
        result = BuildUpdateRequest(task);
        break;
      }
      SqlModificationRequest request = new SqlModificationRequest(result.Batch);
      foreach (var binding in result.ParameterBindings)
        request.ParameterBindings[binding.Key] = binding.Value;
      request.ExpectedResult = result.Batch.Count;
      DomainHandler.Compile(request);
      return request;
    }

    private SqlRequestBuilderResult BuildInsertRequest(SqlRequestBuilderTask task)
    {
      var result = new SqlRequestBuilderResult(task, SqlFactory.Batch());
      foreach (IndexInfo index in result.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.GetTable(index));
        SqlInsert query = SqlFactory.Insert(table);
        int i = 0;
        foreach (ColumnInfo column in index.Columns) {
          int offset = result.GetOffsetFor(column);
          SqlParameter p = result.GetParameterFor(column);
          query.Values[table[i++]] = p;
          result.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
        }
        result.Batch.Add(query);
      }
      return result;
    }

    private SqlRequestBuilderResult BuildUpdateRequest(SqlRequestBuilderTask task)
    {
      var result = new SqlRequestBuilderResult(task, SqlFactory.Batch());
      foreach (IndexInfo index in result.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.GetTable(index));
        SqlUpdate query = SqlFactory.Update(table);
        int i = 0;
        foreach (ColumnInfo column in index.Columns) {
          int offset = result.GetOffsetFor(column);
          if (!task.FieldMap[offset]) {
            i++;
            continue;
          }
          SqlParameter p = result.GetParameterFor(column);
          query.Values[table[i++]] = p;
          result.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
        }
        // There is nothing to update in this table, skipping it
        if (query.Values.Count == 0)
          continue;
        query.Where &= BuildWhereExpression(result, table);
        result.Batch.Add(query);
      }
      return result;
    }

    private SqlRequestBuilderResult BuildRemoveRequest(SqlRequestBuilderTask task)
    {
      var result = new SqlRequestBuilderResult(task, SqlFactory.Batch());
      foreach (IndexInfo index in result.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.GetTable(index));
        SqlDelete query = SqlFactory.Delete(table);
        query.Where &= BuildWhereExpression(result, table);
        result.Batch.Add(query);
      }
      return result;
    }

    private static SqlExpression BuildWhereExpression(SqlRequestBuilderResult result, SqlTableRef table)
    {
      SqlExpression expression = null;
      int i = 0;
      foreach (ColumnInfo column in result.PrimaryIndex.KeyColumns.Keys) {
        int offset = result.GetOffsetFor(column);
        SqlParameter p = result.GetParameterFor(column);
        expression &= table[i++]==p;
        result.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
      }
      return expression;
    }

    private static Func<Tuple, object> CreateTupleFieldAccessor(int fieldIndex)
    {
      return (target => target.IsNull(fieldIndex) ? DBNull.Value : target.GetValue(fieldIndex));
    }


    // Constructor

    public SqlRequestBuilder(DomainHandler domainHandler)
    {
      DomainHandler = domainHandler;
    }
  }
}