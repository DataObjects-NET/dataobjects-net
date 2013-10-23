// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System.Data;
using NUnit.Framework;
using Xtensive.Sql.Model;
using Constraint = Xtensive.Sql.Model.Constraint;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  [TestFixture]
  public abstract class Northwind
  {
    protected struct DbCommandExecutionResult
    {
      public int FieldCount;
      public string[] FieldNames;
      public int RowCount;

      public override string ToString()
      {
        if (FieldNames==null)
          FieldNames = new string[0];
        return string.Format("Fields: '{0}'; Rows: {1}", string.Join("', '", FieldNames), RowCount);
      }
    }

    protected static DbCommandExecutionResult GetExecuteDataReaderResult(IDbCommand cmd)
    {
      DbCommandExecutionResult result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
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
        //      catch (Exception e) {
        //        Console.WriteLine(e);
        //      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected static DbCommandExecutionResult GetExecuteNonQueryResult(IDbCommand cmd)
    {
      DbCommandExecutionResult result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        result.RowCount = cmd.ExecuteNonQuery();
      }
        //      catch (Exception e) {
        //        Console.WriteLine(e);
        //      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected void IgnoreMe(string message)
    {
      throw new IgnoreException(message);
    }

    public Catalog Catalog { get; protected set; }

    [TestFixtureSetUp]
    public virtual void SetUp()
    {
    }
  }
}