// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.31

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Xtensive.Core.Conversion;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Integrity.Transactions;
using Xtensive.TransactionLog.Providers;
using Xtensive.TransactionLog.Providers.FileSystem;

namespace Xtensive.TransactionLog.Tests
{
  [TestFixture]
  public class ClassMembers
  {
    private readonly string providerPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "LogFolder");
    private ILogProvider logProvider;
    private readonly Biconverter<long, string> keyToFileNameConverter = TestFileNameProvider.Instance;
    private readonly string logName = "TestLogName";

    public void SetUp()
    {
      try {
        Directory.Delete(providerPath, true);
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
      }
      logProvider = new FileSystemLogProvider(providerPath);
    }

    [Test]
    public void Constructor()
    {
      SetUp();
      using (new TransactionLog<long>(logProvider, logName, 
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
      }
    }

    [Test]
    public void CounterHasNoValue()
    {
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, logName, 
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        long? value = log.FirstUncommitted;
        Assert.IsNull(value);
      }
    }

    [Test]
    public void Properties()
    {
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, logName, 
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        Assert.IsNull(log.FirstUncommitted);
        long key = 123;
        log.Append(new TestTransaction(key));
        Assert.IsNotNull(log.FirstUncommitted);
        Assert.AreEqual(key, log.FirstUncommitted);
        Assert.IsNotNull(log.Provider);
        Assert.IsNotNull(log.Name);
        Assert.AreEqual(logName, log.Name);
        Assert.AreEqual(logProvider, log.Provider);
      }
    }

    [Test]
    public void Truncate()
    {
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, logName, 
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        for (long i = 0; i < 1000; i++) {
          var transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.Completed | TransactionState.RolledBack);
          log.Append(transaction);
        }
        int filesCount = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, logName)).Length;
        Assert.IsTrue(filesCount > 5); // Just to have files to truncate/delete
        log.Truncate(0);
        int newFilesCount = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, logName)).Length;
        Assert.AreEqual(filesCount, newFilesCount);
        // Delete first and second files
        string[] files = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, logName));
        Array.Sort(files);
        long keyToTruncate = long.Parse(Path.GetFileName(files[2]));
        log.Truncate(keyToTruncate - 1);
        int filesCountAfterTruncate = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, logName)).Length;
        Assert.AreEqual(filesCount, filesCountAfterTruncate + 1);
      }
    }

    [Test]
    public void GetEnumeratorOfTransaction()
    {
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, logName, 
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        int transactionCount = 400;
        for (long i = 0; i < transactionCount; i++) {
          var transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.Completed | TransactionState.RolledBack);
          log.Append(transaction);
        }
        for (int i = 0; i < transactionCount; i++) {
          var transactionList = new List<TestTransaction>();
          foreach (TestTransaction transaction in log.Read(i))
            transactionList.Add(transaction);
          Assert.AreEqual(2, transactionList.Count);
          Assert.AreEqual(transactionList[0].Identifier, i);
          Assert.AreEqual(transactionList[1].Identifier, i);
          Assert.AreEqual(transactionList[0].State, TransactionState.Active);
          Assert.AreEqual(transactionList[1].State, TransactionState.Completed | TransactionState.RolledBack);
        }
      }
    }

    [Test]
    public void GetEnumeratorOfMissingTransaction()
    {
      SetUp();
      using (
        TransactionLog<long> log =
          new TransactionLog<long>(logProvider, logName, keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        int transactionCount = 400;
        for (long i = 0; i < transactionCount; i++) {
          TestTransaction transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.Completed | TransactionState.RolledBack);
          log.Append(transaction);
        }

        List<TestTransaction> transactionList = new List<TestTransaction>();
        foreach (TestTransaction transaction in log.Read(transactionCount + 100))
          transactionList.Add(transaction);
        Assert.AreEqual(0, transactionList.Count);
      }
    }

    [Test]
    public void GetEnumerator()
    {
      SetUp();
      using (
        TransactionLog<long> log =
          new TransactionLog<long>(logProvider, logName, keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        int transactionCount = 400;
        for (long i = 0; i < transactionCount; i++)
        {
          TestTransaction transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.Completed | TransactionState.RolledBack);
          log.Append(transaction);
        }
        List<TestTransaction> transactionList = new List<TestTransaction>();
        foreach (TestTransaction transaction in log.Read(0))
          transactionList.Add(transaction);
        Assert.AreEqual(2*transactionCount, transactionList.Count);
      }
    }

  }
}