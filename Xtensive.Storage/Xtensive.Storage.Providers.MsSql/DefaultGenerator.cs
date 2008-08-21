  // Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.29

using System;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.MsSql
{
  [Serializable]
  public class DefaultGenerator : Storage.Generator
  {
    private Table generatorTable;
    private SqlDataType dataType;
    private const int BULK_SIZE = 100;

    protected override Tuple NextNumber()
    {
      Tuple result = Tuple.Create(Hierarchy.KeyTupleDescriptor);
      SqlBatch batch = SqlFactory.Batch();
//      var i = SqlFactory.Variable("i", dataType);
//      batch.Add(i.Declare());
//      batch.Add(SqlFactory.Assign(i, 0));
//      batch.Add(SqlFactory.While(SqlFactory.LessThan(i, BULK_SIZE)));
//      batch.Add(SqlFactory.("BEGIN"));


      SqlInsert insert = SqlFactory.Insert(SqlFactory.TableRef(generatorTable));
      batch.Add(insert);
      SqlSelect select = SqlFactory.Select();
      select.Columns.Add(SqlFactory.Cast(SqlFactory.FunctionCall("SCOPE_IDENTITY"), dataType));
      batch.Add(select);
      var sessionHandler = (SessionHandler)Handlers.SessionHandler;
      using (var command = new SqlCommand(sessionHandler.Connection)) {
        command.Transaction = sessionHandler.Transaction;
        command.Statement = batch;
        command.Prepare();
        var id = command.ExecuteScalar();
        result.SetValue(0, id);
      }
      return result;
    }

    public override void Initialize()
    {
      base.Initialize();
      var keyColumn = Hierarchy.Columns[0];
      var domainHandler = (DomainHandler)Handlers.DomainHandler;
      generatorTable = domainHandler.Schema.CreateTable(Hierarchy.MappingName);
      if (keyColumn.ValueType == typeof(int))
        dataType = SqlDataType.Int32;
      else if (keyColumn.ValueType == typeof(uint))
        dataType = SqlDataType.UInt32;
      else if (keyColumn.ValueType == typeof(long))
        dataType = SqlDataType.Int64;
      else
        dataType = SqlDataType.UInt64;
      var column = generatorTable.CreateColumn("ID", new SqlValueType(dataType));
      column.SequenceDescriptor = new SequenceDescriptor(column, 1, 1);
      SqlBatch batch = SqlFactory.Batch();
      batch.Add(SqlFactory.Create(generatorTable));
      var sessionHandler = (SessionHandler) Handlers.SessionHandler;
      using (var command = new SqlCommand(sessionHandler.Connection)) {
        command.Transaction = sessionHandler.Transaction;
        command.Statement = batch;
        command.Prepare();
        command.ExecuteNonQuery();
      }
    }
  }
}