// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.19.09

using System.Collections.Generic;
using System.Data.SqlClient;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.Performance.SqlClientCrudModel;

namespace Xtensive.Orm.Tests.Storage.Performance
{
  [TestFixture]
  [Explicit]
  public class SqlClientCrudTest : AutoBuildTest
  {
    public const int BaseCount = 10000;
    public const int InsertCount = BaseCount;
    private bool warmup = false;
    private int instanceCount;

    private readonly SqlConnection con = new SqlConnection("Data Source=localhost;"
      + "Initial Catalog = DO40-Tests;"
      + "Integrated Security=SSPI;");

    protected override DomainConfiguration BuildConfiguration()
    {
      // Just to ensure schema is ready
      var config = DomainConfigurationFactory.CreateForCrudTest("mssql2005");
      config.Types.Register(
        typeof (CrudModel.Simplest).Assembly, typeof (CrudModel.Simplest).Namespace);
      return config;
    }


    [Test]
    public void RegularTest()
    {
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
      CombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      int instanceCount = 10000;
      InsertTest(instanceCount);
      MaterializeTest(instanceCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void UpdatePerformanceTest()
    {
      int instanceCount = 1000000;
      InsertTest(instanceCount);
      SingleStatementUpdateTest();
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      InsertTest(insertCount);
      MaterializeTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      UpdateTest();
      SingleStatementUpdateTest();
      RemoveTest();
    }
    
    private void InsertTest(int insertCount)
    {
      con.Open();
      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Insert", insertCount)) {
        SqlTransaction transaction = con.BeginTransaction();
        SqlCommand cmd = con.CreateCommand();
        cmd.Transaction = transaction;
        cmd.Parameters.AddWithValue("@pId", (long) 0);
        cmd.Parameters.AddWithValue("@pTypeId", (long) 0);
        cmd.CommandText = "INSERT INTO " + 
          "[dbo].[Simplest] ([Simplest].[Id], [Simplest].[TypeId], [Simplest].[Value]) " + 
          "VALUES (@pId, @pTypeId, @pId)";

        for (int i = 0; i < insertCount; i++) {
          cmd.Parameters["@pId"].SqlValue = (long) i;
          cmd.Parameters["@pTypeId"].SqlValue = (long) 0;
          cmd.ExecuteNonQuery();
        }
        
        transaction.Commit();
      }
      instanceCount = insertCount;
      con.Close();
    }

    private void MaterializeTest(int count)
    {
      long sum = 0;
      int i = 0;
      con.Open();
      SqlTransaction transaction = con.BeginTransaction();

      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Materialize & GetField", count)) {
        while (i < count) {
          SqlCommand cmd = con.CreateCommand();
          cmd.Transaction = transaction;
          cmd.CommandText = "SELECT [Simplest].[Id], [Simplest].[TypeId], [Simplest].[Value] " +
            "FROM [dbo].[Simplest]";
          SqlDataReader dr = cmd.ExecuteReader();
          while (dr.Read()) {
            var s = new Simplest();
            if (!dr.IsDBNull(0))
              s.Id = (long) dr.GetValue(0);
            if (!dr.IsDBNull(2))
              s.Value = (long) dr.GetValue(2);
            sum += s.Id;
            if (++i >= count)
              break;
          }
          dr.Close();
        }
        transaction.Commit();
      }
      Assert.AreEqual((long)count * (count - 1) / 2, sum);
      con.Close();
    }

    private void FetchTest(int count)
    {
      long sum = (long) count * (count - 1) / 2;

      con.Open();
      SqlTransaction transaction = con.BeginTransaction();
      SqlCommand cmd = con.CreateCommand();
      cmd.Transaction = transaction;
      cmd.Parameters.AddWithValue("@pId", 0);
      cmd.CommandText = "SELECT [Simplest].[Id], [Simplest].[TypeId], [Simplest].[Value] " + 
        "FROM [dbo].[Simplest] WHERE [Simplest].[Id] = @pId";
      SqlDataReader dr;

      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Fetch & GetField", count)) {
        for (int i = 0; i < count; i++) {
          cmd.Parameters["@pId"].SqlValue = i % instanceCount;
          dr = cmd.ExecuteReader();

          var s = new Simplest();
          while (dr.Read()) {
            if (!dr.IsDBNull(0))
              s.Id = (long) dr.GetValue(0);
            if (!dr.IsDBNull(2))
              s.Value = (long) dr.GetValue(2);
          }
          sum -= s.Id;
          dr.Close();
        }
        transaction.Commit();
      }
      if (count <= instanceCount)
        Assert.AreEqual(0, sum);
      con.Close();
    }

    private void QueryTest(int count)
    {
      con.Open();
      SqlTransaction transaction = con.BeginTransaction();
      SqlCommand cmd = con.CreateCommand();
      cmd.Transaction = transaction;
      cmd.Parameters.AddWithValue("@pId", 0);
      cmd.CommandText = "SELECT [Simplest].[Id], [Simplest].[TypeId], [Simplest].[Value] " + 
        "FROM [dbo].[Simplest] WHERE [Simplest].[id] = @pId";
      SqlDataReader dr;

      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Query", count)) {
        for (int i = 0; i < count; i++) {
          cmd.Parameters["@pId"].SqlValue = i % instanceCount;
          dr = cmd.ExecuteReader();

          var s = new Simplest();
          while (dr.Read()) {
            if (!dr.IsDBNull(0))
              s.Id = (long) dr.GetValue(0);
            if (!dr.IsDBNull(2))
              s.Value = (long) dr.GetValue(2);
          }
          dr.Close();
        }
        transaction.Commit();
      }
      con.Close();
    }

    private void UpdateTest()
    {
      con.Open();
      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Update", instanceCount)) {
        SqlTransaction transaction = con.BeginTransaction();
        SqlCommand cmd = con.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT [Simplest].[Id], [Simplest].[TypeId], [Simplest].[Value] " + 
          "FROM [dbo].[Simplest]";
        cmd.Parameters.AddWithValue("@pId", 0);
        cmd.Parameters.AddWithValue("@pValue", 0);
        
        SqlDataReader dr = cmd.ExecuteReader();
        var list = new List<Simplest>();
        while (dr.Read()) {
          if (!dr.IsDBNull(0) && !dr.IsDBNull(2))
            list.Add(new Simplest((long)dr.GetValue(0), (long)dr.GetValue(2)));
        }
        dr.Close();

        cmd.CommandText = "UPDATE [dbo].[Simplest] SET [Simplest].[Value] = @pValue WHERE [Simplest].[Id] = @pId";
        foreach (var l in list) {
          cmd.Parameters["@pId"].SqlValue = l.Id;
          cmd.Parameters["@pValue"].SqlValue = l.Value + 1;
          cmd.ExecuteNonQuery();
        }
        transaction.Commit();
      }
      con.Close();
    }

    private void SingleStatementUpdateTest()
    {
      con.Open();
      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("SingleStatementUpdate", instanceCount)) {
        SqlTransaction transaction = con.BeginTransaction();
        SqlCommand cmd = con.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = 
          "UPDATE [dbo].[Simplest] " + 
          "SET [Simplest].[Value] = -[Simplest].[Value] " + 
          "WHERE [Simplest].[Value] >= 0";
        cmd.ExecuteNonQuery();
        transaction.Commit();
      }
      con.Close();
    }

    private void RemoveTest()
    {
      con.Open();
      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Remove", instanceCount)) {
        SqlTransaction transaction = con.BeginTransaction();
        SqlCommand cmd = con.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT [Simplest].[Id], [Simplest].[TypeId], [Simplest].[Value] " + 
          "FROM [dbo].[Simplest]";
        cmd.Parameters.AddWithValue("@pId", 0);
        
        SqlDataReader dr = cmd.ExecuteReader();
        var list = new List<Simplest>();
        while (dr.Read()) {
          if (!dr.IsDBNull(0) && !dr.IsDBNull(2))
            list.Add(new Simplest((long)dr.GetValue(0), (long)dr.GetValue(2)));
        }
        dr.Close();

        cmd.CommandText = "DELETE [dbo].[Simplest] WHERE [Simplest].[Id] = @pId";
        foreach (var l in list) {
          cmd.Parameters["@pId"].SqlValue = l.Id;
          cmd.ExecuteNonQuery();
        }
        transaction.Commit();
      }
      con.Close();
    }
  }
}
