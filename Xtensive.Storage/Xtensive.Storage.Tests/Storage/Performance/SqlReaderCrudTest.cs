// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.19.09

using System.Data.SqlClient;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.CrudModel;

namespace Xtensive.Storage.Tests.Storage.CrudModel
{
  public class SimplestSql
  {
    public long Id { get; set; }
    public long Value { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class SqlReaderCrudTest : AutoBuildTest
  {
    public const int BaseCount = 10000;
    public const int InsertCount = BaseCount;
    private bool warmup = false;
    private int instanceCount;
    private readonly SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog = DO40-Tests;"
      + "Integrated Security=SSPI;");

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("mssql2005");
      config.Types.Register(typeof(Simplest).Assembly, typeof(Simplest).Namespace);
      return config;
    }


    [Test]
    public void SqlReaderRegularTest()
    {
      warmup = true;
      SqlReaderCombinedTest(10, 10);
      warmup = false;
      SqlReaderCombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      int instanceCount = 1000;
      InsertTest(instanceCount);
      //      BulkFetchTest(instanceCount);
      FetchTest(instanceCount);
    }
    
    private void SqlReaderCombinedTest(int baseCount, int insertCount)
    {
      InsertTest(insertCount);
      BulkFetchTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      RemoveTest();
    }

    #region Create, Insert, BulkFetch, Fetch, Query, Remove for SqlReader

    private void InsertTest(int inserCount)
    {
      con.Open();
      SqlCommand cmd = con.CreateCommand();
      cmd.Parameters.AddWithValue("@prm", 0);
      TestHelper.CollectGarbage();

      using (warmup ? null : new Measurement("Insert", inserCount))
        for (int i = 0; i < inserCount; i++) {
          cmd.CommandText = "Insert into Simplest VALUES ( @prm, 0, @prm)";
          cmd.Parameters["@prm"].SqlValue = i;
          cmd.ExecuteNonQuery();
        }
      instanceCount = inserCount;
      con.Close();
    }

    private void BulkFetchTest(int count)
    {
      long sum = 0;
      int i = 0;
      con.Open();
      SqlCommand cmd = con.CreateCommand();
      cmd.CommandText = "Select Id, Value from Simplest";
      SqlDataReader dr = cmd.ExecuteReader();

      TestHelper.CollectGarbage();

      using (warmup ? null : new Measurement("Bulk Fetch & GetField", count))
        while (i < count) {
          while (dr.Read()) {
            var s = new SimplestSql();
            if (!dr.IsDBNull(0))
              s.Id = dr.GetInt64(0);
            if (!dr.IsDBNull(1))
              s.Value = dr.GetInt64(1);
            sum += s.Id;
            if (++i >= count)
              break;
          }
          dr.Close();
          dr = cmd.ExecuteReader();
        }
      con.Close();
    }

    private void FetchTest(int count)
    {
      long sum = (long) count * (count - 1) / 2;

      con.Open();
      SqlCommand cmd = con.CreateCommand();
      cmd.Parameters.AddWithValue("@prm", 0);
      SqlDataReader dr;

      TestHelper.CollectGarbage();

      using (warmup ? null : new Measurement("Fetch & GetField", count))
        for (int i = 0; i < count; i++) {
          cmd.Parameters["@prm"].SqlValue = i % instanceCount;
          cmd.CommandText = "Select Id, Value from Simplest where Value = @prm";
          dr = cmd.ExecuteReader();

          var s = new SimplestSql();
          while (dr.Read()) {
            if (!dr.IsDBNull(0))
              s.Id = dr.GetInt64(0);
            if (!dr.IsDBNull(1))
              s.Value = dr.GetInt64(1);
          }

          sum -= s.Id;
          dr.Close();
        }
      if (count <= instanceCount)
        Assert.AreEqual(0, sum);
      con.Close();
    }

    private void QueryTest(int count)
    {
      long sum = (long)count * (count - 1) / 2;

      con.Open();
      SqlCommand cmd = con.CreateCommand();
      cmd.Parameters.AddWithValue("@prm", 0);
      SqlDataReader dr;

      TestHelper.CollectGarbage();

      using (warmup ? null : new Measurement("Query", count))
        for (int i = 0; i < count; i++)
        {
          cmd.Parameters["@prm"].SqlValue = i % instanceCount;
          cmd.CommandText = "Select Id, Value from Simplest where Value = @prm";
          dr = cmd.ExecuteReader();

          var s = new SimplestSql();
          while (dr.Read())
          {
            if (!dr.IsDBNull(0))
              s.Id = dr.GetInt64(0);
            if (!dr.IsDBNull(1))
              s.Value = dr.GetInt64(1);
          }
          dr.Close();
        }
      con.Close();
    }


    private void RemoveTest()
    {
      con.Open();
      SqlCommand cmd = con.CreateCommand();
      TestHelper.CollectGarbage();

      using (warmup ? null : new Measurement("Remove", instanceCount)) {
        cmd.CommandText = "Delete Simplest";
        cmd.ExecuteNonQuery();
      }
      con.Close();
    }

    #endregion
  }
}
