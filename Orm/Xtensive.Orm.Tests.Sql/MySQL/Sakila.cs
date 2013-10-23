// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System.Data;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Constraint = Xtensive.Sql.Model.Constraint;

namespace Xtensive.Orm.Tests.Sql.MySQL
{
  [TestFixture]
  public abstract class Sakila
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
              for (int i = 0; i<fieldCount; i++) {
                fieldNames[i] = reader.GetName(i);
              }
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
      try
      {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        result.RowCount = cmd.ExecuteNonQuery();
      }
      //      catch (Exception e) {
      //        Console.WriteLine(e);
      //      }
      finally
      {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    public void IgnoreMe(string message)
    {
      throw new IgnoreException(message);
    }

    public Catalog Catalog { get; protected set; }

    [TestFixtureSetUp]
    public virtual void SetUp()
    {
      //      BinaryModelProvider bmp = new BinaryModelProvider(@"C:/Debug/AdventureWorks.bin");
      //      model = Database.Model.Build(bmp);
      //
      //      if (model!=null)
      //        return;

      Catalog = new Catalog("Sakila");

      Table t;
      View v;
      TableColumn c;
      Constraint cs;

      t = Catalog.Schemas["Sakila"].CreateTable("Malisa");
      t.CreateColumn("TransactionID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TransactionDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("TransactionType", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ActualCost", new SqlValueType(SqlType.Double));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));


      //SqlDriver mssqlDriver = new MssqlDriver(new MssqlVersionInfo(new Version()));
      //v = Catalog.Schemas["HumanResources"].CreateView("vEmployee",
      //        Sql.Native(mssqlDriver.Compile(select).CommandText));
      //      bmp.Save(model);
    }
  }
}
