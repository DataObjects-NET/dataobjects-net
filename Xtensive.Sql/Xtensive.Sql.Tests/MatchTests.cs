// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.04.24

using System;
using System.Data.Common;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Tests
{
  public class MatchTests: AdventureWorks
  {
    private SqlConnection sqlConnection;
    private SqlCommand sqlCommand;

    [Test]
    public void MatchPredicateTest()
    {
      SqlConnectionProvider provider = new SqlConnectionProvider();
      sqlConnection = provider.CreateConnection(@"mssql2005://localhost/AdventureWorks") as SqlConnection;
      sqlConnection.Open();
      sqlCommand = (SqlCommand)sqlConnection.CreateCommand();
      sqlCommand.Transaction = sqlConnection.BeginTransaction();
      try {
        SqlBatch batch = Sql.Batch();
        TemporaryTable t = Catalog.DefaultSchema.CreateTemporaryTable("match_pred_test");
        t.PreserveRows = false;
        t.CreateColumn("id", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("col1", new SqlValueType(SqlDataType.Int32)).IsNullable = true;
        t.CreateColumn("col2", new SqlValueType(SqlDataType.Int32)).IsNullable = true;
        batch.Add(SqlDdl.Create(t));
        SqlTableRef tref = Sql.TableRef(t);

        Action<int, SqlExpression, SqlExpression> insert = delegate(int id, SqlExpression expr1, SqlExpression expr2) {
                                                             SqlInsert ins2 = Sql.Insert(tref);
                                                             ins2.Values.Add(tref["id"], id);
                                                             ins2.Values.Add(tref["col1"], expr1);
                                                             ins2.Values.Add(tref["col2"], expr2);
                                                             batch.Add(ins2);
                                                           };
        //unique part
        insert(00, Sql.Null, Sql.Null);
        insert(01, Sql.Null, Sql.Null);
        insert(02, Sql.Null, 2);
        insert(03, 1, Sql.Null);
        insert(04, 1, 2);
        //non-unique part
        insert(05, Sql.Null, 4);
        insert(06, Sql.Null, 4);
        insert(07, 3, Sql.Null);
        insert(08, 3, Sql.Null);
        insert(09, 3, 4);
        insert(10, 3, 4);

        ExecuteNonQuery(batch);

        //query
        SqlSelect testQuery = Sql.Select(tref);
        testQuery.Columns.Add(tref["col1"]);
        testQuery.Columns.Add(tref["col2"]);

        Action<SqlExpression, SqlExpression, bool, SqlMatchType> matchTesterExists
          = delegate(SqlExpression col1, SqlExpression col2, bool unique, SqlMatchType matchType) {
              SqlSelect q = Sql.Select();
              q.Columns.Add(1);
              q.Where = Sql.Match(Sql.Row(col1, col2), testQuery, unique, matchType);
              AssertQueryExists(q);
            };

        Action<SqlExpression, SqlExpression, bool, SqlMatchType> matchTesterNotExists
          = delegate(SqlExpression col1, SqlExpression col2, bool unique, SqlMatchType matchType) {
              SqlSelect q = Sql.Select();
              q.Columns.Add(1);
              q.Where = Sql.Match(Sql.Row(col1, col2), testQuery, unique, matchType);
              AssertQueryNotExists(q);
            };

        //match simple
        matchTesterExists(Sql.Null, Sql.Null, false, SqlMatchType.None);
        matchTesterExists(Sql.Null, Sql.Null, true, SqlMatchType.None);
        matchTesterExists(Sql.Null, 2, false, SqlMatchType.None);
        matchTesterExists(Sql.Null, 2, true, SqlMatchType.None);
        matchTesterExists(Sql.Null, 4, false, SqlMatchType.None);
        matchTesterExists(Sql.Null, 4, true, SqlMatchType.None);
        matchTesterExists(Sql.Null, 3, false, SqlMatchType.None);
        matchTesterExists(Sql.Null, 3, true, SqlMatchType.None);
        matchTesterExists(9, Sql.Null, false, SqlMatchType.None);
        matchTesterExists(9, Sql.Null, true, SqlMatchType.None);
        matchTesterExists(1, 2, false, SqlMatchType.None);
        matchTesterExists(1, 2, true, SqlMatchType.None);
        matchTesterExists(3, 4, false, SqlMatchType.None);
        matchTesterNotExists(3, 4, true, SqlMatchType.None);
        matchTesterNotExists(3, 3, false, SqlMatchType.None);
        matchTesterNotExists(3, 3, true, SqlMatchType.None);

        //match full

        matchTesterExists(Sql.Null, Sql.Null, false, SqlMatchType.Full);
        matchTesterExists(Sql.Null, Sql.Null, true, SqlMatchType.Full);
        matchTesterNotExists(Sql.Null, 2, false, SqlMatchType.Full);
        matchTesterNotExists(Sql.Null, 2, true, SqlMatchType.Full);
        matchTesterNotExists(Sql.Null, 4, false, SqlMatchType.Full);
        matchTesterNotExists(Sql.Null, 4, true, SqlMatchType.Full);
        matchTesterNotExists(Sql.Null, 3, false, SqlMatchType.Full);
        matchTesterNotExists(Sql.Null, 3, true, SqlMatchType.Full);
        matchTesterNotExists(9, Sql.Null, false, SqlMatchType.Full);
        matchTesterNotExists(9, Sql.Null, true, SqlMatchType.Full);
        matchTesterExists(1, 2, false, SqlMatchType.Full);
        matchTesterExists(1, 2, true, SqlMatchType.Full);
        matchTesterExists(3, 4, false, SqlMatchType.Full);
        matchTesterNotExists(3, 4, true, SqlMatchType.Full);
        matchTesterNotExists(9, 9, false, SqlMatchType.Full);
        matchTesterNotExists(9, 9, true, SqlMatchType.Full);
      }
      catch (Exception ex) {
        Assert.Fail(ex.ToString());
      }
      finally {
        sqlCommand.Transaction.Rollback();
        sqlConnection.Close();
      }
    }

    protected void ExecuteNonQuery(ISqlCompileUnit stmt)
    {
      sqlCommand.Statement = stmt;
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);
      int result = sqlCommand.ExecuteNonQuery();
    }

    protected void AssertQueryExists(ISqlCompileUnit q)
    {
      sqlCommand.Statement = q;
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);
      using (DbDataReader dr = sqlCommand.ExecuteReader()) {
        bool exists = false;
        while (dr.Read()) {
          exists = true;
          break;
        }
        if (!exists)
          Assert.Fail("Query not exists.");
      }
    }

    protected void AssertQueryNotExists(ISqlCompileUnit q)
    {
      sqlCommand.Statement = q;
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);
      using (DbDataReader dr = sqlCommand.ExecuteReader()) {
        while (dr.Read()) {
          Assert.Fail("Query exists.");
        }
      }
    }
  }
}