// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.02.06

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
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
      Catalog = ExtractCatalog();
      CreateTables(Catalog);
    }

    [Test]
    public void UpdateTest01()
    {
      var tableRef = SqlDml.TableRef(Catalog.DefaultSchema.Tables["Department"]);
      var update = SqlDml.Update(tableRef);
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now.ToString()));
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
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now.ToString()));
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
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now.ToString()));
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
      update.Values.Add(tableRef.Columns["ModifiedDate"], SqlDml.Literal(DateTime.Now.ToString()));
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
      TableColumn column;
      var createBatch = SqlDml.Batch();
      var dropBatch = SqlDml.Batch();

      if (schema.Tables["EmployeeAddress"]!=null) {
        dropBatch.Add(SqlDdl.Drop(schema.Tables["EmployeeAddress"]));
        schema.Tables.Remove(schema.Tables["EmployeeAddress"]);
      }
      table = schema.CreateTable("EmployeeAddress");
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      if (schema.Tables["EmployeeDepartmentHistory"]!=null) {
        dropBatch.Add(SqlDdl.Drop(schema.Tables["EmployeeDepartmentHistory"]));
        schema.Tables.Remove(schema.Tables["EmployeeDepartmentHistory"]);
      }
      table = schema.CreateTable("EmployeeDepartmentHistory");
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int16));
      table.CreateColumn("ShiftID", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      if (schema.Tables["EmployeePayHistory"]!=null) {
        dropBatch.Add(SqlDdl.Drop(schema.Tables["EmployeePayHistory"]));
        schema.Tables.Remove(schema.Tables["EmployeePayHistory"]);
      }
      table = schema.CreateTable("EmployeePayHistory");
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("RateChangeDate", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("Rate", new SqlValueType(SqlType.Double));
      table.CreateColumn("PayFrequency", new SqlValueType(SqlType.Int16));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      if (schema.Tables["Employee"]!=null) {
        dropBatch.Add(SqlDdl.Drop(schema.Tables["Employee"]));
        schema.Tables.Remove(schema.Tables["Employee"]);
      }
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

      if (schema.Tables["Department"]!=null) {
        dropBatch.Add(SqlDdl.Drop(schema.Tables["Department"]));
        schema.Tables.Remove(schema.Tables["Department"]);
      }
      table = schema.CreateTable("Department");
      table.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Name", Driver.TypeMappings[typeof (string)].MapType());
      table.CreateColumn("GroupName", Driver.TypeMappings[typeof (string)].MapType());
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      if (schema.Tables["Shift"]!=null) {
        dropBatch.Add(SqlDdl.Drop(schema.Tables["Shift"]));
        schema.Tables.Remove(schema.Tables["Shift"]);
      }
      table = schema.CreateTable("Shift");
      table.CreateColumn("ShiftID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Name", Driver.TypeMappings[typeof (string)].MapType());
      table.CreateColumn("StartTime", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("EndTime", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      if (schema.Tables["JobCandidate"]!=null) {
        dropBatch.Add(SqlDdl.Drop(schema.Tables["JobCandidate"]));
        schema.Tables.Remove(schema.Tables["JobCandidate"]);
      }
      table = schema.CreateTable("JobCandidate");
      table.CreateColumn("JobCandidateID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      table.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      createBatch.Add(SqlDdl.Create(table));

      if (Driver.ServerInfo.Query.Features.HasFlag(QueryFeatures.Batches)) {
        if (dropBatch.Count > 0) {
          using (var command = Connection.CreateCommand(dropBatch)) {
            Console.WriteLine(command.CommandText);
            command.ExecuteNonQuery();
          }
        }
        using (var command = Connection.CreateCommand(createBatch)) {
          Console.WriteLine(command.CommandText);
          command.ExecuteNonQuery();
        }
      }
      else {
        if (dropBatch.Count > 0)
          foreach (var query in dropBatch) {
            using (var command = Connection.CreateCommand((SqlDropTable) query)) {
              Console.WriteLine(command.CommandText);
              command.ExecuteNonQuery();
            }
          }
        foreach (var query in createBatch) {
          using (var command = Connection.CreateCommand((SqlCreateTable) query)) {
            Console.WriteLine(command.CommandText);
            command.ExecuteNonQuery();
          }
        }
      }
    }
  }
}
