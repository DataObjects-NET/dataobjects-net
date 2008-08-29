// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.31

using System.IO;
using NUnit.Framework;
using Xtensive.Core.Conversion;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Integrity.Transactions;
using Xtensive.TransactionLog.Providers;
using Xtensive.TransactionLog.Providers.FileSystem;
using System;

namespace Xtensive.TransactionLog.Tests
{
  [TestFixture]
  public class AppendTest
  {
    private readonly string providerPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "LogFolder");
    private ILogProvider logProvider;

    public void SetUp()
    {
      try {
        Directory.Delete(providerPath, true);
      }
      catch (Exception e) {
        Log.Warning("Error while deleting the log folder: {0}", e);
      }
      logProvider = new FileSystemLogProvider(providerPath);
    }

    [Test]
    public void Append()
    {
      SetUp();
      Biconverter<long, string> keyToFileNameConverter = TestFileNameProvider.Instance;
      using (var log = new TransactionLog<long>(logProvider, "TestLogName", keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        for (int i = 0; i < 1000; i++) {
          TestTransaction record = new TestTransaction(i);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
      }
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Transaction is not active. It may be missing or is already committed.")]
    public void DoubleCommit()
    {
      SetUp();
      Biconverter<long, string> keyToFileNameConverter = TestFileNameProvider.Instance;
      using (var log = new TransactionLog<long>(logProvider, "TestLogName", keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        for (int i = 0; i < 1000; i++)
        {
          var record = new TestTransaction(i);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
          log.Append(record);
          record.SetState(TransactionState.Completed | TransactionState.RolledBack);
          log.Append(record);
          log.Append(record);
        }
      }
    }

    [Test]
    public void CommitTest()
    {
      SetUp();
      Biconverter<long, string> keyToFileNameConverter = TestFileNameProvider.Instance;
      using (
        var log = new TransactionLog<long>(logProvider, "TestLogName", 
          keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        int transactionCount = 400;
        for (int i = 0; i < transactionCount; i++) {
          TestTransaction record = new TestTransaction(i);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
        for (int i = 0; i < transactionCount; i++) {
          var record = new TestTransaction(i);
          record.SetState(TransactionState.Active);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
        for (int i = 0; i < transactionCount; i++) {
          var record = new TestTransaction(i);
          record.SetState(TransactionState.Active);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
        for (int i = 0; i < transactionCount; i++) {
          var record = new TestTransaction(i);
          record.SetState(i%2==0 ? TransactionState.Committed : TransactionState.RolledBack | TransactionState.Completed);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
      }
    }

    [Test]
    public void RestoreTest()
    {
      SetUp();
      Biconverter<long, string> keyToFileNameConverter = TestFileNameProvider.Instance;
      long firstUncommited;
      using (
        var log = new TransactionLog<long>(logProvider, "TestLogName", 
          keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        int transactionCount = 400;
        for (int i = 0; i < transactionCount; i++)
        {
          var record = new TestTransaction(i);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
        for (int i = 0; i < transactionCount; i++)
        {
          var record = new TestTransaction(i);
          record.SetState(TransactionState.Active);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
        for (int i = 0; i < transactionCount; i++)
        {
          var record = new TestTransaction(i);
          record.SetState(TransactionState.Active);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
        for (int i = 0; i < transactionCount - 10; i++)
        {
          var record = new TestTransaction(i);
          record.SetState(i % 2 == 0 ? TransactionState.Committed : TransactionState.RolledBack | TransactionState.Completed);
          record.InternalData = Guid.NewGuid();
          log.Append(record);
        }
        firstUncommited = log.FirstUncommitted.Value;
      }
      // Open again
      using (
        var newLog = new TransactionLog<long>(logProvider, "TestLogName", 
          keyToFileNameConverter, TimeSpan.FromSeconds(1), 1, null, BinaryValueSerializerProvider.Default.GetSerializer<long>()))
      {
        Assert.AreEqual(newLog.FirstUncommitted, firstUncommited);
      }
    }

  }
}