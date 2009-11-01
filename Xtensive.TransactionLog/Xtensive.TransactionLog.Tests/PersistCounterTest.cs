// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.02

using System;
using System.IO;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Serialization;
using Xtensive.Core.Serialization.Binary;
using Xtensive.TransactionLog.Providers;
using Xtensive.TransactionLog.Providers.FileSystem;

namespace Xtensive.TransactionLog.Tests
{
  [TestFixture]
  public class PersistCounterTest
  {
    private const string counterName = "PersistCounter{T}";
    private readonly string providerPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "LogFolder");
    private ILogProvider logProvider;

    // [TestFixtureSetUp]
    public void SetUp()
    {
      try {
        Directory.Delete(providerPath, true);
      }
      catch (Exception e) {
        Log.Warning(e, "Error while deleting the log folder");
      }
      logProvider = new FileSystemLogProvider(providerPath);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Counter has no value.")]
    public void TestCreate()
    {
      SetUp();
      using (PersistCounter<int> counter = new PersistCounter<int>(counterName, logProvider, TimeSpan.FromSeconds(1), ValueSerializerProvider.Default.GetSerializer<int>()))
      {
        long persistedValue = counter.PersistedValue;
        long value = counter.Value;
      }
    }

    [Test]
    public void TestWrite()
    {
      SetUp();
      using (PersistCounter<int> counter = new PersistCounter<int>(counterName, logProvider, TimeSpan.FromSeconds(1), ValueSerializerProvider.Default.GetSerializer<int>()))
      {
        int iterations = 1000;
        for (int i=0;i<=iterations;i++) {
          counter.Value = i*10;
        }
        Assert.AreEqual(0, counter.PersistedValue);
        Assert.AreEqual(iterations*10, counter.Value);
      }
    }

    [Test]
    public void TestPersistInt()
    {
      SetUp();
      using (PersistCounter<int> counter = new PersistCounter<int>(counterName, logProvider, TimeSpan.FromSeconds(1), ValueSerializerProvider.Default.GetSerializer<int>()))
      {
        int iterations = 1000;
        for (int i = 1; i <= iterations; i++)
        {
          counter.Value = i * 10;
          if (i%100==0)
            counter.Flush();
        }
        counter.Flush();
        Assert.AreEqual(iterations * 10, counter.PersistedValue);
        Assert.AreEqual(iterations * 10, counter.Value);
      }
    }

    [Test]
    public void TestPersistLong()
    {
      SetUp();
      using (PersistCounter<long> counter = new PersistCounter<long>(counterName, logProvider, TimeSpan.FromSeconds(1), ValueSerializerProvider.Default.GetSerializer<long>()))
      {
        int iterations = 1000;
        for (int i = 1; i <= iterations; i++)
        {
          counter.Value = i * 10;
          if (i % 100 == 0)
            counter.Flush();
        }
        counter.Flush();
        Assert.AreEqual(iterations * 10, counter.PersistedValue);
        Assert.AreEqual(iterations * 10, counter.Value);
      }
    }
  }
}