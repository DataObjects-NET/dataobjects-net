// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.15

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class PriorityQueueTest
  {
    [Test]
    public void FastEnqueeDequeTest()
    {
      PriorityQueue<string, string> pq = new PriorityQueue<string, string>();
      int count = 10000;
      using (new Measurement("Fast Enqueue test", count)) {
        for (int i = 0; i < count; i++)
        {
          pq.Enqueue(DateTime.Now.ToString(), Guid.NewGuid().ToString());
        }
      }
      Assert.AreEqual(pq.Count, count);
      using (new Measurement("Fast Dequeue test", count)) {
        while (pq.Count > 0)
          pq.Dequeue();
      }
    }

    [Test]
    public void EnqueueDequeTest()
    {
      PriorityQueue<string, string> pqAsc = new PriorityQueue<string, string>(Direction.Negative);
      for (int i = 0; i < 10000; i++)
      {
        string data = Guid.NewGuid().ToString();
        pqAsc.Enqueue(data, data);
      }
      for (int i = 0; i < pqAsc.Count - 1; i++)
      {
        Assert.Greater(pqAsc[i], pqAsc[i + 1]);
      }

      PriorityQueue<string, string> pqDesc = new PriorityQueue<string, string>(Direction.Positive);
      for (int i = 0; i < 10000; i++)
      {
        string data = Guid.NewGuid().ToString();
        pqDesc.Enqueue(data, data);
      }
      for (int i = 0; i < pqDesc.Count - 1; i++)
      {
        Assert.Less(pqDesc[i], pqDesc[i + 1]);
      }
    
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RemoveTest()
    {
      PriorityQueue<string, string> pq = new PriorityQueue<string, string>();
      string item = "item";
      pq.Enqueue("1", "s1");
      pq.Enqueue(item, "s2");
      pq.Enqueue("3", "s3");
      pq.Remove("fff");
    }

    [Test]
    public void DequeRangeTest()
    {
      PriorityQueue<string, string> pq = new PriorityQueue<string, string>(Direction.Positive);
      pq.Enqueue("item1.1", "s1");
      pq.Enqueue("item1.2", "s1");
      pq.Enqueue("item2.1", "s2");
      pq.Enqueue("item2.2", "s2");
      pq.Enqueue("item3", "s3");
      string[] result = pq.DequeueRange("s2");
      Assert.AreEqual(5, pq.Count + result.Length);
      Assert.AreEqual(4, result.Length);

      PriorityQueue<string, string> pqDesc = new PriorityQueue<string, string>(Direction.Negative);
      pqDesc.Enqueue("item1.1", "s1");
      pqDesc.Enqueue("item1.2", "s1");
      pqDesc.Enqueue("item2.1", "s2");
      pqDesc.Enqueue("item2.2", "s2");
      pqDesc.Enqueue("item3", "s3");
      string[] resultDesc = pqDesc.DequeueRange("s2");
      Assert.AreEqual(5, pqDesc.Count + resultDesc.Length);
      Assert.AreEqual(3, resultDesc.Length);
    }
  }
}