// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class CompilerTest : SqlTest
  {
    protected Catalog Catalog { get; private set; }
    protected override void CheckRequirements()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.PostgreSql);
    }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      var initialCatalog = ExtractCatalog();
      DropSchema(initialCatalog);
      Catalog = ExtractCatalog();
      CreateTables(Catalog);
    }

    [Test]
    public void UpdateTest01()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Update(tableRef);
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now));
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public void UpdateTest02()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Update(tableRef);
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now));
      update.Where = SqlDml.Equals(tableRef.Columns["Name"], SqlDml.Literal("Human Resources Department"));
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public void UpdateTest03()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Update(tableRef);
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now));
      update.Limit = SqlDml.Native("100");
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public void UpdateTest04()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Update(tableRef);
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now.Date));
      update.Where = SqlDml.Equals(tableRef.Columns["Name"], SqlDml.Literal("Human Resources Department"));
      update.Limit = SqlDml.Native("100");
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public void DeleteTest01()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Delete(tableRef);
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public void DeleteTest02()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Delete(tableRef);
      update.Where = SqlDml.Equals(tableRef.Columns["Name"], SqlDml.Literal("Human Resources Department"));
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public void DeleteTest03()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Delete(tableRef);
      update.Limit = SqlDml.Native("100");
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public void DeleteTest04()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Delete(tableRef);
      update.Where = SqlDml.Equals(tableRef.Columns["Name"], SqlDml.Literal("Human Resources Department"));
      update.Limit = SqlDml.Native("100");
      Console.WriteLine(Driver.Compile(update).GetCommandText());
      using (var command = Connection.CreateCommand(update)) {
        command.ExecuteNonQuery();
      }
    }
    protected virtual void CreateTables(Catalog catalog)
    {
      var schema = catalog.DefaultSchema;
      Table table;
      var createBatch = SqlDml.Batch();

      table = schema.CreateTable("EmployeeAddress");
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("EmployeeDepartmentHistory");
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int16));
      table.CreateColumn("ShiftID", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("EmployeePayHistory");
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("RateChangeDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("Rate", new SqlValueType(SqlType.Double));
      table.CreateColumn("PayFrequency", new SqlValueType(SqlType.Int16));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("Employee");
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("NationalIDNumber", Driver.TypeMappings[typeof(string)].MapType());
      table.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("LoginID", Driver.TypeMappings[typeof (string)].MapType());
      table.CreateColumn("ManagerID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Title", Driver.TypeMappings[typeof(string)].MapType());
      table.CreateColumn("BirthDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("MaritalStatus", Driver.TypeMappings[typeof(string)].MapType());
      table.CreateColumn("Gender", Driver.TypeMappings[typeof(string)].MapType());
      table.CreateColumn("HireDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("VacationHours", new SqlValueType(SqlType.Int16));
      table.CreateColumn("SickLeaveHours", new SqlValueType(SqlType.Int16));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("Department");
      table.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Name", Driver.TypeMappings[typeof (string)].MapType());
      table.CreateColumn("GroupName", Driver.TypeMappings[typeof (string)].MapType());
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("Shift");
      table.CreateColumn("ShiftID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Name", Driver.TypeMappings[typeof (string)].MapType());
      table.CreateColumn("StartTime", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("EndTime", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("JobCandidate");
      table.CreateColumn("JobCandidateID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      ExecuteBatch(createBatch);
    }

    private void DropSchema(Catalog catalog)
    {
      var schema = catalog.DefaultSchema;
      var dropBatch = SqlDml.Batch();

      foreach (var constraint in schema.Tables.Where(t => t.TableConstraints.Count != 0).SelectMany(t => t.TableConstraints.OfType<ForeignKey>())) {
        if (dropBatch.Count > 31) {
          ExecuteBatch(dropBatch);
          dropBatch = SqlDml.Batch();
        }
        dropBatch.Add(SqlDdl.Alter(constraint.Table, SqlDdl.DropConstraint(constraint)));
      }

      foreach (var view in schema.Views) {
        if (dropBatch.Count > 31) {
          ExecuteBatch(dropBatch);
          dropBatch = SqlDml.Batch();
        }
        dropBatch.Add(SqlDdl.Drop(view));
        if (schema.Tables[view.Name]!=null) {
          _ = schema.Tables.Remove(schema.Tables[view.Name]);
        }
      }

      foreach (var schemaTable in schema.Tables) {
        if (dropBatch.Count > 31) {
          ExecuteBatch(dropBatch);
          dropBatch = SqlDml.Batch();
        }
        dropBatch.Add(SqlDdl.Drop(schemaTable));
      }

      if (dropBatch.Count!=0)
        ExecuteBatch(dropBatch);
    }

    private void ExecuteBatch(SqlBatch batch)
    {
      if (Driver.ServerInfo.Query.Features.HasFlag(QueryFeatures.Batches)) {
        if (batch.Count > 0) {
          using (var command = Connection.CreateCommand(batch)) {
            Console.WriteLine(command.CommandText);
            command.ExecuteNonQuery();
          }
        }
        return;
      }
      if (batch.Count > 0)
        foreach (var query in batch) {
          using (var command = Connection.CreateCommand((ISqlCompileUnit) query)) {
            Console.WriteLine(command.CommandText);
            command.ExecuteNonQuery();
          }
        }
    }
  }
}
