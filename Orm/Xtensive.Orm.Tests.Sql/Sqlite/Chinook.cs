// Copyright (C) 2011-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  [TestFixture, Explicit]
  public abstract class Chinook
  {
    protected struct DbCommandExecutionResult
    {
      public int FieldCount;
      public string[] FieldNames;
      public int RowCount;

      public override string ToString()
      {
        if (FieldNames == null)
          FieldNames = new string[0];
        return string.Format("Fields: '{0}'; Rows: {1}", string.Join("', '", FieldNames), RowCount);
      }
    }

    protected SqlDriver sqlDriver;
    protected SqlConnection sqlConnection;

    protected string Url { get { return TestConnectionInfoProvider.GetConnectionUrl(); } }
    public Catalog Catalog { get; protected set; }

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
      CheckRequirements();
      sqlDriver = TestSqlDriver.Create(Url);
      sqlConnection = sqlDriver.CreateConnection();

      try {
        sqlConnection.Open();
      }
      catch (Exception exception) {
        Console.WriteLine(exception);
        throw;
      }
      try {
        sqlConnection.BeginTransaction();
        Catalog = sqlDriver.ExtractCatalog(sqlConnection);
        var schema = Catalog.DefaultSchema;

        var creator = new ChinookSchemaCreator(sqlDriver);
        creator.DropSchemaContent(sqlConnection, schema);
        creator.CreateSchemaContent(sqlConnection, schema);

        sqlConnection.Commit();
        sqlConnection.Close();
      }
      catch {
        sqlConnection.Rollback();
        sqlConnection.Close();
        throw;
      }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
      try {
        if (sqlConnection != null) {
          if (sqlConnection.State != ConnectionState.Closed)
            sqlConnection.Close();
          sqlConnection.Dispose();
          sqlConnection = null;
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
        throw;
      }
    }

    [SetUp]
    public virtual void SetUp()
    {
      sqlConnection.Open();
    }

    [TearDown]
    public virtual void TearDown()
    {
      sqlConnection.Close();
    }

    protected virtual void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sqlite);
    }

    protected DbCommandExecutionResult GetExecuteDataReaderResult(IDbCommand cmd)
    {
      var result = new DbCommandExecutionResult();

      sqlConnection.BeginTransaction();
      try {
        cmd.Transaction = sqlConnection.ActiveTransaction;
        int rowCount = 0;
        int fieldCount = 0;
        string[] fieldNames = new string[0];
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            if (rowCount==0) {
              fieldCount = reader.FieldCount;
              fieldNames = new string[fieldCount];
              for (int i = 0; i < fieldCount; i++)
                fieldNames[i] = reader.GetName(i);
            }
            rowCount++;
          }
        }
        result.RowCount = rowCount;
        result.FieldCount = fieldCount;
        result.FieldNames = fieldNames;
      }
      finally {
        sqlConnection.Rollback();
      }
      return result;
    }

    protected DbCommandExecutionResult GetExecuteNonQueryResult(IDbCommand cmd)
    {
      var result = new DbCommandExecutionResult();
      sqlConnection.BeginTransaction();
      try {
        cmd.Transaction = sqlConnection.ActiveTransaction;
        result.RowCount = cmd.ExecuteNonQuery();
      }
      finally {
        sqlConnection.Rollback();
      }
      return result;
    }

    protected SqlCompilationResult Compile(ISqlCompileUnit statement)
    {
      return sqlDriver.Compile(statement);
    }
  }
}