// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Threading;

namespace Xtensive.Tests.Threading
{
  [TestFixture]
  public sealed class AsyncProcessorTest
  {
    private readonly List<Thread> stressingThreads = new List<Thread>();

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      var startedEvent = new AutoResetEvent(false);
      for (int i = 0; i < Environment.ProcessorCount * 2; i++) {
        var thread = new Thread(() => {
          startedEvent.Set();
          var t = true;
          while (t) {
            t = DateTime.Now.Year >= 2009;
          }
          Assert.IsTrue(t);
        });
        thread.Start();
        stressingThreads.Add(thread);
        startedEvent.WaitOne();
      }
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      foreach (var thread in stressingThreads)
        thread.Abort();
    }
    
    [Test]
    public void AsyncExecutionTest()
    {
      TestExecution(false, false);
    }

    [Test]
    public void SyncExecutionTest()
    {
      TestExecution(false, true);
      TestExecution(true, false);
      TestExecution(true, true);
    }

    [Test]
    public void InterceptionOfExceptionFromFuncTest()
    {
      using (var processor = new AsyncProcessor()) {
        int completedTaskCount = 0;
        var results = new List<TaskResult<int>>();
        var startEvent = new ManualResetEvent(false);
        processor.Execute(() => {
          startEvent.WaitOne();
          completedTaskCount++;
        }, false);
        results.Add(processor.Execute(() => ++completedTaskCount, false));
        results.Add(processor.Execute(() => {
          throw new AssertionException(string.Empty);
#pragma warning disable 162
          return ++completedTaskCount;
#pragma warning restore 162
        }, false));
        processor.Execute(() => { completedTaskCount++; }, false);
        results.Add(processor.Execute(() => ++completedTaskCount, false));
        startEvent.Set();
        processor.WaitForCompletion();
        Assert.AreEqual(3, results.Count);
        Assert.AreEqual(2, completedTaskCount);
        Assert.AreEqual(2, results[0].Result);
        AssertEx.Throws<AssertionException>(() => { var t = results[1].Result; });
        CheckThrowingOfTaskExecutionException(() => { var t = results[2].Result; });
        CheckThrowingOfTaskExecutionException(() => processor.Execute(() => { completedTaskCount++; }, false));
        CheckThrowingOfTaskExecutionException(() => processor.Execute(() => ++completedTaskCount, false));
        TestErrorReseting(processor);
      }
    }

    [Test]
    public void InterceptionOfExceptionFromActionTest()
    {
      using (var processor = new AsyncProcessor()) {
        int completedTaskCount = 0;
        var results = new List<TaskResult<int>>();
        var startEvent = new ManualResetEvent(false);
        processor.Execute(() => {
          startEvent.WaitOne();
          completedTaskCount++;
        }, false);
        results.Add(processor.Execute(() => ++completedTaskCount, false));
        processor.Execute(() => {
          throw new AssertionException(string.Empty);
          // completedTaskCount++;
        }, false);
        processor.Execute(() => { completedTaskCount++; }, false);
        results.Add(processor.Execute(() => ++completedTaskCount, false));
        startEvent.Set();
        processor.WaitForCompletion();
        Assert.AreEqual(2, results.Count);
        Assert.AreEqual(2, completedTaskCount);
        Assert.AreEqual(2, results[0].Result);
        CheckThrowingOfTaskExecutionException(() => processor.Execute(() => { completedTaskCount++; }, false));
        CheckThrowingOfTaskExecutionException(() => processor.Execute(() => ++completedTaskCount, false));
        TestErrorReseting(processor);
      }
    }

    [Test]
    public void SyncInterceptionOfExceptionFromFuncTest()
    {
      TestSyncInterceptionOfExceptionFromFunc(false);
      TestSyncInterceptionOfExceptionFromFunc(true);
    }

    [Test]
    public void SyncInterceptionOfExceptionFromActionTest()
    {
      TestSyncInterceptionOfExceptionFromAction(false);
      TestSyncInterceptionOfExceptionFromAction(true);
    }

    [Test]
    public void SwitchingBetweenAsyncAndSyncModesTest()
    {
      TestSwitchingBetweenAsyncAndSyncModes(false);
      TestSwitchingBetweenAsyncAndSyncModes(true);
    }

    private static void TestExecution(bool syncMode, bool invokeSynchrounously)
    {
      using (var processor = new AsyncProcessor {IsSynchronous = syncMode}) {
        int completedActionCount = 0;
        int completedFuncCount = 0;
        const int taskCount = 5;
        var testingThread = Thread.CurrentThread;
        var funcEvent = new ManualResetEvent(invokeSynchrounously || syncMode);
        var actionEvent = new ManualResetEvent(invokeSynchrounously || syncMode);
        var results = new List<TaskResult<int>>();
        for (int i = 0; i < taskCount; i++) {
          processor.Execute(() => {
            actionEvent.WaitOne();
            if (!invokeSynchrounously && !syncMode)
              Assert.AreNotSame(testingThread, Thread.CurrentThread);
            else
              Assert.AreSame(testingThread, Thread.CurrentThread);
            completedActionCount++;
          }, invokeSynchrounously);

          var result = processor.Execute(() => {
            funcEvent.WaitOne();
            if (!invokeSynchrounously && !syncMode)
              Assert.AreNotSame(testingThread, Thread.CurrentThread);
            else
              Assert.AreSame(testingThread, Thread.CurrentThread);
            completedFuncCount++;
            return completedFuncCount;
          }, invokeSynchrounously);

          results.Add(result);
        }
        if (!invokeSynchrounously && !syncMode) {
          Assert.AreEqual(0, completedActionCount);
          Assert.AreEqual(0, completedFuncCount);
          actionEvent.Set();
          funcEvent.Set();
          for (int i = 0; i < results.Count; i++)
            Assert.AreEqual(i + 1, results[i].Result);
          processor.WaitForCompletion();
          Assert.AreEqual(taskCount, completedActionCount);
          Assert.AreEqual(taskCount, completedFuncCount);
        }
        else {
          Assert.AreEqual(taskCount, completedActionCount);
          Assert.AreEqual(taskCount, completedFuncCount);
          for (int i = 0; i < results.Count; i++)
            Assert.AreEqual(i + 1, results[i].Result);
          processor.WaitForCompletion();
        }
      }
    }

    private void CheckThrowingOfTaskExecutionException(Action action)
    {
      var isFailed = true;
      try {
        action.Invoke();
      }
      catch(TaskExecutionException e) {
        isFailed = false;
        Assert.AreEqual(typeof(AssertionException), e.InnerException.GetType());
      }
      Assert.IsFalse(isFailed);
    }

    private void TestErrorReseting(AsyncProcessor processor)
    {
      Assert.IsTrue(processor.HasError);
      processor.ResetError();
      Assert.IsFalse(processor.HasError);
      var count = 0;
      processor.Execute(() => count++, false);
      var result = processor.Execute(() => ++count, false);
      Assert.AreEqual(2, result.Result);
    }

    private void TestSyncInterceptionOfExceptionFromFunc(bool syncMode)
    {
      using (var processor = new AsyncProcessor()) {
        processor.IsSynchronous = syncMode;
        int completedTaskCount = 0;
        TaskResult<int> result = processor.Execute(() => {
          throw new AssertionException(string.Empty);
#pragma warning disable 162
          return completedTaskCount++;
#pragma warning restore 162
        }, !syncMode);
        Assert.IsTrue(processor.HasError);
        AssertEx.Throws<AssertionException>(() => { var t = result.Result; });
        Assert.AreEqual(0, completedTaskCount);
        CheckThrowingOfTaskExecutionException(() =>
          processor.Execute(() => { completedTaskCount++; }, !syncMode));
        CheckThrowingOfTaskExecutionException(() =>
          processor.Execute(() => ++completedTaskCount, !syncMode));
        Assert.AreEqual(0, completedTaskCount);
        processor.ResetError();
        Assert.IsFalse(processor.HasError);
      }
    }

    private void TestSyncInterceptionOfExceptionFromAction(bool syncMode)
    {
      using (var processor = new AsyncProcessor()) {
        processor.IsSynchronous = syncMode;
        int completedTaskCount = 0;
        processor.Execute(() => {
          throw new AssertionException(string.Empty);
          // completedTaskCount++;
        }, !syncMode);
        Assert.IsTrue(processor.HasError);
        Assert.AreEqual(0, completedTaskCount);
        CheckThrowingOfTaskExecutionException(() =>
          processor.Execute(() => { completedTaskCount++; }, !syncMode));
        CheckThrowingOfTaskExecutionException(() =>
          processor.Execute(() => ++completedTaskCount, !syncMode));
        Assert.AreEqual(0, completedTaskCount);
        processor.ResetError();
        Assert.IsFalse(processor.HasError);
      }
    }

    private void TestSwitchingBetweenAsyncAndSyncModes(bool syncMode)
    {
      using (var processor = new AsyncProcessor()) {
        int completedTaskCount = 0;
        var startEvent = new ManualResetEvent(false);
        completedTaskCount = 0;
        processor.Execute(() => {
          startEvent.WaitOne();
            Thread.Sleep(2000);
            completedTaskCount++;
        }, false);
        var result = processor.Execute(() => ++completedTaskCount, false);
        processor.IsSynchronous = syncMode;
        startEvent.Set();
        processor.Execute(() => {
          Assert.AreEqual(2, completedTaskCount++);
          Assert.AreEqual(2, result.Result);
        }, !syncMode);
        if (processor.HasError)
          throw processor.Error;
        Assert.AreEqual(3, completedTaskCount);
      }
    }
  }
}