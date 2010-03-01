// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using System.Threading;

namespace Xtensive.Core.Aspects.Tests.Links
{
  [TestFixture]
  public class LinkTests
  {
    public static bool MultiThreadingTest = false;
    public static bool PerformanceTest = false;
    static LinkEventStage eventStage;
    static int eventCount;
    
    public static LinkEventStage EventStage
    {
      get { return eventStage;}
      set { eventStage = value;}
    }

    public static int EventCount
    {
      get {return eventCount;}
      set {eventCount = value;}
    }

    static void ResetStageAndCount()
    {
      EventStage = LinkEventStage.Prologue;
      EventCount = 0;
    } 

    [Test]
    public void OneToOneMasterSlaveTest()
    {
      OneToOneMaster master1 = new OneToOneMaster("Master1");
      OneToOneMaster master2 = new OneToOneMaster("Master2");
      OneToOneSlave slave1 = new OneToOneSlave("Slave1");
      OneToOneSlave slave2 = new OneToOneSlave("Slave2");

      ResetStageAndCount();
      Log.Info("master1.Slave = slave1;");
      master1.Slave = slave1;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 6);
      Assert.AreSame(master1.Slave, slave1);
      Assert.AreSame(slave1.Master, master1);
      master1.Slave = slave1;
      slave1.Master = master1; 

      ResetStageAndCount();
      Log.Info("");
      Log.Info("master1.Slave = slave2;");
      master1.Slave = slave2;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 9);
      Assert.AreSame(master1.Slave, slave2);
      Assert.AreSame(slave2.Master, master1);
      Assert.IsNull(slave1.Master);
      master1.Slave = slave2;
      slave2.Master = master1; 

      ResetStageAndCount();
      Log.Info("");
      Log.Info("master1.Slave = null;");
      master1.Slave = null;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 6);
      Assert.IsNull(master1.Slave);
      Assert.IsNull(slave2.Master);
      master1.Slave = null;
      slave2.Master = null; 

      ResetStageAndCount();
      Log.Info("");
      Log.Info("slave1.Master = master1;");
      slave1.Master = master1;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 6);
      Assert.AreSame(slave1.Master, master1);
      Assert.AreSame(master1.Slave, slave1);
      slave1.Master = master1;
      master1.Slave = slave1;

      ResetStageAndCount();
      Log.Info("");
      Log.Info("slave1.Master = master2;");
      slave1.Master = master2;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 9);
      Assert.AreSame(slave1.Master, master2);
      Assert.AreSame(master2.Slave, slave1);
      Assert.IsNull(master1.Slave);
      slave1.Master = master2;
      master2.Slave = slave1;

      ResetStageAndCount();
      Log.Info("");
      Log.Info("slave1.Master = null;");
      slave1.Master = null;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 6);
      Assert.IsNull(slave1.Master);
      Assert.IsNull(master2.Slave);
      slave1.Master = null;
      master2.Slave = null;
    }

    [Test]
    public void OneToOnePairTest()
    {
      OneToOnePair pair1 = new OneToOnePair("Pair1");
      OneToOnePair pair2 = new OneToOnePair("Pair2");
      OneToOnePair pair3 = new OneToOnePair("Pair3");

      ResetStageAndCount();
      Log.Info("pair1.Pair = pair2;");
      pair1.Pair = pair2;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 6);
      Assert.AreSame(pair1.Pair, pair2);
      Assert.AreSame(pair2.Pair, pair1);
      pair1.Pair = pair2;
      pair2.Pair = pair1;

      ResetStageAndCount();
      Log.Info("");
      Log.Info("pair1.Pair = pair3;");
      pair1.Pair = pair3;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 9);
      Assert.AreSame(pair1.Pair, pair3);
      Assert.AreSame(pair3.Pair, pair1);
      Assert.IsNull(pair2.Pair);
      pair1.Pair = pair3;
      pair3.Pair = pair1;

      ResetStageAndCount();
      Log.Info("");
      Log.Info("pair3.Pair = null;");
      pair3.Pair = null;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 6);
      Assert.IsNull(pair3.Pair);
      Assert.IsNull(pair1.Pair);
      pair3.Pair = null;
      pair1.Pair = null;

      PerformanceTest = true;
      try {
        pair1.Pair = pair3;
      }
      finally {
        PerformanceTest = false;
      }
      ResetStageAndCount();
      Log.Info("");
      Log.Info("pair1.Pair = null;");
      pair1.Pair = null;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreEqual(EventCount, 6);
      Assert.IsNull(pair1.Pair);
      Assert.IsNull(pair3.Pair);
      pair1.Pair = null;
      pair3.Pair = null;
    }

    [Test]
    public void OneToManyTest()
    {
      // Add item to owner by adding to collection.
      ResetStageAndCount();
      OneToManyOwner owner = new OneToManyOwner("Owner 1");
      OneToManyItem item = new OneToManyItem("Item 1");
      owner.Items.Add(item);
      Assert.IsTrue(item.Owner==owner);
      Assert.IsTrue(owner.Items.Contains(item));
      Assert.IsTrue(EventCount==5);
      item.Owner = owner;
      if (MultiThreadingTest)
        Thread.Sleep(10);

      // Add item to owner by setting owner.
      ResetStageAndCount();
      OneToManyItem item2 = new OneToManyItem("Item 2");
      item2.Owner = owner;
      Assert.IsTrue(item2.Owner==owner);
      Assert.IsTrue(owner.Items.Contains(item2));
      Assert.IsTrue(owner.Items.Count==2);
      Assert.IsTrue(EventCount==5);
      item2.Owner = owner;
      if (MultiThreadingTest)
        Thread.Sleep(10);

      // Change item owner by adding to other owner's collection.
      ResetStageAndCount();
      OneToManyOwner owner2 = new OneToManyOwner("Owner 2");
      owner2.Items.Add(item);
      Assert.IsTrue(item.Owner==owner2);
      Assert.IsTrue(owner2.Items.Contains(item));
      Assert.IsTrue(!owner.Items.Contains(item));
      Assert.IsTrue(EventCount==7);
      if (MultiThreadingTest)
        Thread.Sleep(10);

      // Change item owner by specifying new owner.
      ResetStageAndCount();
      item.Owner = owner;
      Assert.IsTrue(item.Owner==owner);
      Assert.IsTrue(owner.Items.Contains(item));
      Assert.IsTrue(!owner2.Items.Contains(item));
      Assert.IsTrue(EventCount==7);
      if (MultiThreadingTest)
        Thread.Sleep(10);

      // Check clear operation.
      ResetStageAndCount();
      OneToManyItem item3 = new OneToManyItem("Item 3");
      owner.Items.Add(item3);
      ResetStageAndCount();
      int itemCountBeforeClear = owner.Items.Count;
      owner.Items.Clear();
      Assert.IsTrue(item3.Owner==null);
      Assert.IsTrue(item.Owner==null);
      Assert.IsTrue(owner.Items.Count==0);
      Assert.IsTrue(EventCount==itemCountBeforeClear*3+2);
      if (MultiThreadingTest)
        Thread.Sleep(10);

      // Check null items does not throw exceptions.
      ResetStageAndCount();
      owner.Items.Add(null);
      ResetStageAndCount();
      owner.Items.Clear();
      ResetStageAndCount();
      item.Owner = null;
    }

    [Test]
    public void ManyToManyTest()
    {
      // Validate add operation.
      ResetStageAndCount();
      ManyToManyLeft left = new ManyToManyLeft("Left 1");
      ManyToManyRight right = new ManyToManyRight("Right 1");
      left.RightItems.Add(right);
      Assert.IsTrue(right.LeftItems.Contains(left));
      Assert.IsTrue(left.RightItems.Contains(right));
      Assert.IsTrue(EventCount==4);

      // Validate remove operation.
      ResetStageAndCount();
      left.RightItems.Remove(right);
      Assert.IsTrue(!right.LeftItems.Contains(left));
      Assert.IsTrue(!left.RightItems.Contains(right));
      Assert.IsTrue(EventCount==4);

      // Validate clear operation.
      ResetStageAndCount();
      left.RightItems.Add(right);
      Assert.IsTrue(right.LeftItems.Contains(left));
      Assert.IsTrue(left.RightItems.Contains(right));
      ResetStageAndCount();
      left.RightItems.Clear();
      Assert.IsTrue(!right.LeftItems.Contains(left));
      Assert.IsTrue(!left.RightItems.Contains(right));
      Assert.IsTrue(EventCount==4);
    }
    
    [Test]
    public void AddRangeTest()
    {
      OneToManyOwner owner1 = new OneToManyOwner("Owner 1");
      OneToManyOwner owner2 = new OneToManyOwner("Owner 2");
      OneToManyOwner owner3=  new OneToManyOwner("Owner 3");

      OneToManyItem item1 = new OneToManyItem("Item 1");
      ResetStageAndCount();
      owner2.Items.Add(item1);

      OneToManyItem item2 = new OneToManyItem("Item 2");
      OneToManyItem item3 = new OneToManyItem("Item 3");
      ResetStageAndCount();
      owner3.Items.AddRange(new object[] { item2, item3 });
      Assert.IsTrue(item2.Owner == owner3);
      Assert.IsTrue(item3.Owner == owner3);

      ResetStageAndCount();
      owner1.Items.AddRange(new object[] { item1, item2, item3 });
      Assert.IsTrue(owner3.Items.Count == 0);
      Assert.IsTrue(owner2.Items.Count == 0);
      Assert.IsTrue(item1.Owner == owner1);
      Assert.IsTrue(item2.Owner == owner1);
      Assert.IsTrue(item3.Owner == owner1);
    }

    [Test]
    public void LinkReferenceTest()
    {
      MasterWithLinkReference master1 = new MasterWithLinkReference("Master 1");
      MasterWithLinkReference master2 = new MasterWithLinkReference("Master 2");
      OneToOneSlave slave1 = new OneToOneSlave("Slave1");

      ResetStageAndCount();
      master1.Slave = slave1;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreSame(master1.Slave, slave1);
      Assert.AreSame(slave1.Master, master1);
      master1.Slave = slave1;
      slave1.Master = master1;

      ResetStageAndCount();
      slave1.Master = master2;
      Assert.AreEqual(EventStage, LinkEventStage.Epilogue);
      Assert.AreSame(slave1.Master, master2);
      Assert.AreSame(master2.Slave, slave1);
      Assert.IsNull(master1.Slave);
      slave1.Master = master2;
      master2.Slave = slave1;
    }

    [Test]
    public void NestedCallsTest()
    {
      MasterWithLinkReference master1 = new MasterWithLinkReference("Master 1");
      MasterWithLinkReference master2 = new MasterWithLinkReference("Master 2");
      SlaveWithLinkReference slave1 = new SlaveWithLinkReference("Slave 1");
      SlaveWithLinkReference slave2 = new SlaveWithLinkReference("Slave 2");
      master1.SlaveReference.Changing+= delegate {
        master2.Slave = slave2;
        Assert.IsTrue(master2.Slave==slave2);
        Assert.IsTrue(slave2.Master==master2);
      };
      master1.Slave = slave1;
      Assert.IsTrue(master1.Slave==slave1);
      Assert.IsTrue(slave1.Master==master1);
    }

    [Test]
    public void OneToOnePerformanceTest()
    {
      OneToOneMaster master1 = new OneToOneMaster("Master1");
      OneToOneSlave slave1 = new OneToOneSlave("Slave1");
      OneToOneSlave slave2 = new OneToOneSlave("Slave2");
      Log.Info("Running test...");
      PerformanceTest = true;
      int cycleCount = 100000;
      int operationsPerCycle = 3;
      try {
        using (new Measurement("OneToOne link test", cycleCount*operationsPerCycle)) {
          for (int i = 0; i < cycleCount; i++) {
            master1.Slave = slave1;
            EventStage = LinkEventStage.Prologue;
            master1.Slave = slave2;
            EventStage = LinkEventStage.Prologue;
            master1.Slave = null;
            EventStage = LinkEventStage.Prologue;
          }
        }
      }
      finally {
        PerformanceTest = false;
      }
    }

    [Test]
    public void OneToManyPerformanceTest()
    {
      OneToManyOwner owner1 = new OneToManyOwner("Owner 1");
      OneToManyItem item1 = new OneToManyItem("Item 1");
      OneToManyItem item2 = new OneToManyItem("Item 2");
      OneToManyItem item3 = new OneToManyItem("Item 3");

      Log.Info("Running test...");

      PerformanceTest = true;
      int cycleCount = 100000;
      int operationsPerCycle = 6;
      try {
        using (new Measurement("OneToMany link test", cycleCount*operationsPerCycle)) {
          for (int i = 0; i < cycleCount; i++) {
            owner1.Items.Add(item1);
            EventStage = LinkEventStage.Prologue;
            owner1.Items.Add(item2);
            EventStage = LinkEventStage.Prologue;
            owner1.Items.Add(item3);
            EventStage = LinkEventStage.Prologue;
            owner1.Items.Remove(item1);
            EventStage = LinkEventStage.Prologue;
            owner1.Items.Remove(item2);
            EventStage = LinkEventStage.Prologue;
            owner1.Items.Remove(item3);
          }
        }
      }
      finally {
        PerformanceTest = false;
      }
    }

    [Test]
    public void OneToOneLinkPerformanceTest()
    {
      MasterWithLinkReference master1 = new MasterWithLinkReference("Master1");
      SlaveWithLinkReference slave1 = new SlaveWithLinkReference("Slave1");
      SlaveWithLinkReference slave2 = new SlaveWithLinkReference("Slave2");
      Log.Info("Running test...");
      PerformanceTest = true;
      int cycleCount = 200000;
      int operationsPerCycle = 3;
      try {
        using (new Measurement("OneToOne [Link] link test", cycleCount*operationsPerCycle)) {
          for (int i = 0; i < cycleCount; i++) {
            master1.Slave = slave1;
            EventStage = LinkEventStage.Prologue;
            master1.Slave = slave2;
            EventStage = LinkEventStage.Prologue;
            master1.Slave = null;
            EventStage = LinkEventStage.Prologue;
          }
        }
      }
      finally {
        PerformanceTest = false;
      }
    }

  }
}