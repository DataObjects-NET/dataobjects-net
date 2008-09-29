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

    public const int BaseCount = 10000;
    public const int InsertCount = BaseCount;
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
      
      string productKeyFile = Path.Combine(Environment.CurrentDirectory, @"Storage\Performance\DO39CrudModel\ProductKey.txt");
      string productKey = "";

      if (File.Exists(productKeyFile))
        using (StreamReader sr = new StreamReader(productKeyFile))
          productKey = sr.ReadToEnd().Trim();

      configuration.ProductKey = productKey;
      configuration.DefaultUpdateMode = DomainUpdateMode.Recreate;

      domain = new DataObjects.NET.Domain(configuration);
      domain.RegisterTypes("Xtensive.Storage.Tests.Storage.Performance.DO39Model");
      domain.Build();
    }

    [Test]
    public void RegularTest()
    {
      Initialize();
      warmup = true;
      CombinedTest(10, 10);

      Initialize();
      warmup = false;
      CombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      int instanceCount = 100000;
      Initialize();
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

    private void Initialize()
    {
      using (var s = domain.CreateSession()) {
        s.BeginTransaction();
        var o = (Simplest)s.CreateObject(typeof(Simplest), new object[] { (long) 0 });
        firstId = o.ID + 1;
        s.Rollback();
      }
    }

    private void InsertTest(int insertCount)
    {
      using (var s = domain.CreateSession()) {
        long sum = 0;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Insert", insertCount)) {
          s.BeginTransaction();
          for (int i = 0; i < insertCount; i++) {
            var o = (Simplest)s.CreateObject(typeof(Simplest), new object[] { (long) i });
            sum += i;
          }
          s.Commit();
        }
      }
      instanceCount = insertCount;
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
            sum -= o.Value;
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
              sum += o.Value;
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
      string queryText = "Select Simplest instances where {ID}=@pId";
      using (var s = domain.CreateSession()) {
        s.BeginTransaction();
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Query", count)) {
          for (int i = 0; i < count; i++) {
            Query q = s.CreateQuery(queryText);
            q.Parameters.Add("@pId", 0);
            q.Parameters["@pId"].Value = ((long)i % instanceCount) + firstId;
            var qr = q.Execute();
            foreach (var o in qr) {
              // Doing nothing, just enumerate
            }
          }
          s.Commit();
        }
      }
    }

    private void RemoveTest()
    {
      string queryText = "Select Simplest instances";
      using (var s = domain.CreateSession()) {
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          s.BeginTransaction();
          foreach (var o in s.CreateQuery(queryText).Execute<Simplest>())
            o.Remove();
          s.Commit();
        }
      }
    }
  }
}
