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
using Xtensive.Core.Testing;
using Xtensive.Integrity.Transactions;
using Xtensive.TransactionLog.Providers;
using Xtensive.TransactionLog.Providers.FileSystem;
using Xtensive.Core.Collections;
using System.Linq;

namespace Xtensive.TransactionLog.Tests
{
  [TestFixture]
  public class ClassMembersTest
  {
    private readonly string providerPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "LogFolder");
    private ILogProvider logProvider;
    private readonly Biconverter<long, string> keyToFileNameConverter = TestFileNameProvider.Instance;
    private const string LogName = "TestLogName";

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
    public void ConstructorTest()
    {
      SetUp();
      using (new TransactionLog<long>(logProvider, LogName,
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>())) {
      }
    }

    [Test]
    public void CounterHasNoValueTest()
    {
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, LogName,
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>())) {
        long? value = log.FirstUncommitted;
        Assert.IsNull(value);
      }
    }

    [Test]
    public void PropertiesTest()
    {
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, LogName,
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>())) {
        Assert.IsNull(log.FirstUncommitted);
        const long key = 123;
        log.Append(new TestTransaction(key));
        Assert.IsNotNull(log.FirstUncommitted);
        Assert.AreEqual(key, log.FirstUncommitted);
        Assert.IsNotNull(log.Provider);
        Assert.IsNotNull(log.Name);
        Assert.AreEqual(LogName, log.Name);
        Assert.AreEqual(logProvider, log.Provider);
      }
    }

    [Test]
    public void TruncateTest()
    {
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, LogName,
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>())) {
        for (long i = 0; i < 1000; i++) {
          var transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.RolledBack);
          log.Append(transaction);
        }
        int filesCount = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, LogName)).Length;
        Assert.IsTrue(filesCount > 5); // Just to have files to truncate/delete
        log.Truncate(0);
        int newFilesCount = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, LogName)).Length;
        Assert.AreEqual(filesCount, newFilesCount);
        // Delete first and second files
        long[] files = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, LogName)).Convert(fileName => long.Parse(Path.GetFileName(fileName)));
        Array.Sort(files);
        int position = RandomManager.CreateRandom().Next(2, files.Length - 2);
        long keyToTruncate = files[position];
        log.Truncate(keyToTruncate - 1);
        int filesCountAfterTruncate = Directory.GetFiles(string.Format(@"{0}\{1}", providerPath, LogName)).Length;
        Assert.AreEqual(filesCount, filesCountAfterTruncate + position-1);
      }
    }

    [Test]
    public void GetEnumeratorOfTransactionTest()
    {
      const int transactionCount = 100;
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, LogName,
        keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>())) {
        for (long i = 0; i < transactionCount; i++) {
          var transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.RolledBack);
          log.Append(transaction);
        }
        for (int i = 0; i < transactionCount; i++) {
          Assert.AreEqual((transactionCount-i)*2, log.Read(i).Count());
        }
      }
    }

    [Test]
    public void GetEnumeratorOfMissingTransactionTest()
    {
      const int transactionCount = 100;
      SetUp();
      using (var log =new TransactionLog<long>(logProvider, LogName, keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>())) {
        for (long i = 0; i < transactionCount; i++) {
          var transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.RolledBack);
          log.Append(transaction);
        }

        var transactionList = new List<TestTransaction>();
        foreach (TestTransaction transaction in log.Read(transactionCount + 100))
          transactionList.Add(transaction);
        Assert.AreEqual(0, transactionList.Count);
      }
    }

    [Test]
    public void GetEnumeratorTest()
    {
      const int transactionCount = 400;
      SetUp();
      using (var log = new TransactionLog<long>(logProvider, LogName, keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>())) {
        for (long i = 0; i < transactionCount; i++) {
          var transaction = new TestTransaction(i);
          log.Append(transaction);
          transaction.SetState(TransactionState.RolledBack);
          log.Append(transaction);
        }
        var transactionList = new List<TestTransaction>();
        foreach (TestTransaction transaction in log.Read(0))
          transactionList.Add(transaction);
        Assert.AreEqual(2 * transactionCount, transactionList.Count);
      }
    }
  }
}