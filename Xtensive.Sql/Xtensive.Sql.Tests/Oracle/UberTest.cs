// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.21

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.Oracle
{
  public abstract class UberTest : SqlTest
  {
    private const string BatchTestTable = "batch_test";
    private const string LobTestTable = "lob_test";
    
    private int nextId;

    private Schema testSchema;

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      testSchema = ExtractAllSchemas().DefaultSchema;
      EnsureTableNotExists(testSchema, BatchTestTable);
      EnsureTableNotExists(testSchema, LobTestTable);
    }

    [Test]
    public void BatchTest()
    {
      var table = testSchema.CreateTable(BatchTestTable);
      CreateIdColumn(table);
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

    [Test]
    public void LobTest()
    {
      var table = testSchema.CreateTable(LobTestTable);
      CreateIdColumn(table);
      table.CreateColumn("bin_lob", new SqlValueType(SqlType.VarBinaryMax)).IsNullable = true;
      table.CreateColumn("char_lob", new SqlValueType(SqlType.VarCharMax)).IsNullable = true;
      ExecuteNonQuery(SqlDdl.Create(table));

      var tableRef = SqlDml.TableRef(table);
      var insert = CreateInsert(tableRef);
      insert.Values.Add(tableRef["bin_lob"], SqlDml.ParameterRef("p_bin"));
      insert.Values.Add(tableRef["char_lob"], SqlDml.ParameterRef("p_char"));

      var charBuffer = Enumerable.Range(0, 10000)
        .Select(i => (char) (i % (char.MaxValue - 1) + 1))
        .ToArray();
      var binBuffer = Enumerable.Range(0, 10000)
        .Select(i => (byte) (i % byte.MaxValue))
        .ToArray();

      using (var binLob = Connection.CreateBinaryLargeObject())
      using (var charLob = Connection.CreateCharacterLargeObject())
      using (var command = Connection.CreateCommand(insert)) {
        var binParameter = command.CreateParameter();
        binParameter.ParameterName = "p_bin";
        binLob.Write(binBuffer, 0, binBuffer.Length);
        binLob.SetParameterValue(binParameter);
        var charParameter = command.CreateParameter();
        charParameter.ParameterName = "p_char";
        charLob.Write(charBuffer, 0, charBuffer.Length);
        charLob.SetParameterValue(charParameter);
        command.Parameters.Add(charParameter);
        command.Parameters.Add(binParameter);
        command.ExecuteNonQuery();
      }

      var select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["bin_lob"]);
      select.Columns.Add(tableRef["char_lob"]);
      select.Where = SqlDml.Native("rownum")==1;
      using (var comand = Connection.CreateCommand(select))
      using (var reader = comand.ExecuteReader()) {
        Assert.IsTrue(reader.Read());
        Compare(binBuffer, (byte[]) reader[0]);
        Compare(charBuffer, ((string) reader[1]).ToCharArray());
      }
    }

    private void CreateIdColumn(Table table)
    {
      table.CreateColumn("id", new SqlValueType(SqlType.Decimal, 10, 0));
    }

    private void Compare<T>(T[] expected, T[] actual)
    {
      if ((expected!=null)!=(actual!=null))
        Assert.Fail("expected: {0}; actual {1}", expected, actual);
      if (expected==null)
        return;
      if (expected.Length!=actual.Length)
        Assert.Fail("expected length: {0}; actual length {0}", expected.Length, actual.Length);
      for (int i = 0; i < expected.Length; i++)
        Assert.AreEqual(expected[i], actual[i]);
    }

    private SqlInsert CreateInsert(SqlTableRef tableRef)
    {
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef["id"], nextId++);
      return insert;
    }
  }
}