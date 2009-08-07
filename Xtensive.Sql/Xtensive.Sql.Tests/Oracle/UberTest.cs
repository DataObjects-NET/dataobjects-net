// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.21

using System;
using NUnit.Framework;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.Oracle
{
  public abstract class UberTest : SqlTest
  {
    private int nextId;

    protected const string SimpleTable = "simple_table";
    protected Schema TestSchema { get; private set; }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      TestSchema = ExtractAllSchemas().DefaultSchema;
      EnsureTableNotExists(TestSchema, SimpleTable);
    }

    [Test]
    public void BatchTest()
    {
      var table = TestSchema.CreateTable(SimpleTable);
      table.CreateColumn("id", new SqlValueType(SqlType.Decimal, 10, 0));
      ExecuteNonQuery(SqlDdl.Create(table));
      var tableRef = SqlDml.TableRef(table);
      
      var singleStatementBatch = SqlDml.Batch();
      singleStatementBatch.Add(CreateInsert(tableRef));
      ExecuteNonQuery(singleStatementBatch);

      var multiStatementBatch = SqlDml.Batch();
      multiStatementBatch.Add(CreateInsert(tableRef));
      multiStatementBatch.Add(CreateInsert(tableRef));
      ExecuteNonQuery(multiStatementBatch);

      var innerEmptyBatch = SqlDml.Batch();
      var innerSingleStatementBatch = SqlDml.Batch();
      innerSingleStatementBatch.Add(CreateInsert(tableRef));
      var innerMultiStatementBatch = SqlDml.Batch();
      innerMultiStatementBatch.Add(CreateInsert(tableRef));
      var outerBatch = SqlDml.Batch();
      outerBatch.Add(innerEmptyBatch);
      outerBatch.Add(innerSingleStatementBatch);
      outerBatch.Add(multiStatementBatch);
      ExecuteNonQuery(outerBatch);

      var countQuery = SqlDml.Select(tableRef);
      countQuery.Columns.Add(SqlDml.Count());
      Assert.AreEqual(6, Convert.ToInt32(ExecuteScalar(countQuery)));
    }

    private SqlInsert CreateInsert(SqlTableRef tableRef)
    {
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef["id"], nextId++);
      return insert;
    }
  }
}