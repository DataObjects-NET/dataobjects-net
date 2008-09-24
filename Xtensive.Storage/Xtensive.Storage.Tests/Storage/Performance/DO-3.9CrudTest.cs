// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.24

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using DataObjects.NET;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.Storage.Performance.DO39Model;

namespace Xtensive.Storage.Tests.Storage.Performance
{

  [TestFixture]
  public class DO39CrudTest
  {
    static DataObjects.NET.Domain domain;

    public int BaseCount = 10;
    public int InsertCount = 10;
    private bool warmup = false;
    private bool profile = false;
    private int instanceCount;
    private long firstId;

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      DomainConfiguration configuration = new DomainConfiguration();

      configuration.ConnectionUrl = "mssql://localhost/DO39-Tests";
      configuration.Cultures.Add(new Culture("En", "U.S. English", new CultureInfo("en-us", false)));
      configuration.Cultures["En"].Default = true;
      
      string productKeyFile = Path.Combine(Environment.CurrentDirectory, "Storage/Performance/DO-3.9CrudModel/ProductKey.txt");
      string productKey = "";

      if (File.Exists(productKeyFile))
        using (StreamReader sr = new StreamReader(productKeyFile))
        {
          productKey = sr.ReadToEnd().Trim();
        }
      configuration.ProductKey = productKey;
      configuration.DefaultUpdateMode = DomainUpdateMode.Recreate;

      domain = new DataObjects.NET.Domain(configuration);
      domain.RegisterTypes("Xtensive.Storage.Tests.Storage.Performance.DO39Model");
      domain.Build();
    }

    [Test]
    public void RegularTest()
    {
      using (var s = domain.CreateSession())
      {
        s.BeginTransaction();
        var o = (Simplest)s.CreateObject(typeof(Simplest), new object[] { 0, 0 });
        firstId = o.ID + 1;
        s.Rollback();
      }
      warmup = true;
      CombinedTest(BaseCount, InsertCount);
      BaseCount = 10000;
      InsertCount = BaseCount;
      warmup = false;
      CombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      int instanceCount = 100000;
      InsertTest(instanceCount);
      BulkFetchTest(instanceCount);
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      InsertTest(insertCount);
      BulkFetchTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      RemoveTest();
    }

    private void InsertTest(int inserCount)
    {
      using (var s = domain.CreateSession()) {
        long sum = 0;
        s.BeginTransaction();
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Insert", inserCount)) {
            for (int i = 0; i < inserCount; i++) {
              var o = (Simplest)s.CreateObject(typeof(Simplest), new object[] { i, i });
              sum += i;
            }
            s.Commit();
        }
      }
      instanceCount = inserCount;
    }

    private void FetchTest(int count)
    {
      long id = firstId;
      using (var s = domain.CreateSession()) {
        long sum = (long)count * (count - 1) / 2;
        s.BeginTransaction();
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Fetch & GetField", count)) {
        for (int i = 0; i < count; i++) {
          var o = (Simplest)s[id++];
            sum -= o.SimplestId;
        }
      s.Commit();
      }
      if (count <= instanceCount) 
        Assert.AreEqual(0, sum);
      }
    }

    private void BulkFetchTest(int count)
    {
      string queryText = "Select Simplest instances";
      using (var s = domain.CreateSession()) {
        long sum = 0;
        int i = 0;
        s.BeginTransaction();
        TestHelper.CollectGarbage();
        Query q = s.CreateQuery(queryText);
        using (warmup ? null : new Measurement("Bulk Fetch & GetField", count)) {
          while (i < count) {
            foreach (Simplest o in q.Execute()) {
              sum += o.SimplestId;
              if (++i >= count)
                break;
            }
          }
          s.Commit();
        }
        Assert.AreEqual((long)count * (count - 1) / 2, sum);
      }
    }

    private void QueryTest(int count)
    {
      const string queryText = "Select Simplest instances where {ID}=@pId";
      using (var s = domain.CreateSession()) {
        s.BeginTransaction();
        Query q = s.CreateQuery(queryText);
        q.Parameters.Add("@pId", 0);
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Query", count)) {
          for (int i = 0; i < count; i++) {
            q.Parameters["@pId"].Value = ((long)i % instanceCount) + firstId;
            var qr = q.Execute();
            foreach (var o in qr) {
            }
          }
          s.Commit();
        }
      }
    }

    private void RemoveTest()
    {
      using (var s = domain.CreateSession()) {
        s.BeginTransaction();
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          for (int i = 0; i < InsertCount; i++ )
            s[firstId++].Remove();
          s.Commit();
        }
      }
    }
  }
}
