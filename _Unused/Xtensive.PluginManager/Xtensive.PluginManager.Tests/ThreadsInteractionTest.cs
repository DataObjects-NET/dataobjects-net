// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.06.19

using System;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.PluginManager.Tests
{
  public class Searcher
  {
    private bool isComplete = false;

    public delegate void EventDelegate(object sender);

    public event EventDelegate startSearch;
    public event EventDelegate completeSearch;

    public bool IsComplete
    {
      get { return isComplete; }
      set { isComplete = value; }
    }

    public void Search()
    {
      startSearch(this);
      Thread.Sleep(50);
      completeSearch(this);
    }

    public void CreateSearch()
    {
      Thread thread = new Thread(new ThreadStart(Search));
      thread.Start();
    }
  }

  [TestFixture]
  public class ThreadsInteractionTest
  {
    private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

    private void searcher_completeSearch(object sender)
    {
      Searcher s = sender as Searcher;
      if (s != null)
        s.IsComplete = true;
      Console.WriteLine("complete");
    }

    private void searcher_startSearch(object sender)
    {
      Console.WriteLine("start");
    }

    private void ThreadProc()
    {
      Searcher searcher = new Searcher();
      searcher.startSearch += new Searcher.EventDelegate(searcher_startSearch);
      searcher.completeSearch += new Searcher.EventDelegate(searcher_completeSearch);
      searcher.CreateSearch();
      while (searcher.IsComplete != true) {
        Console.WriteLine("sleep");
        Thread.Sleep(20);
      }
    }

    [Test]
    public void Test1()
    {
      Thread[] thrar = new Thread[10];
      for (int i = 0; i < 10; i++) {
        Thread thread = new Thread(new ThreadStart(ThreadProc));
        thrar[i] = thread;
        thread.Start();
      }
      foreach (Thread thread in thrar) {
        thread.Join();
      }
    }
  }
}