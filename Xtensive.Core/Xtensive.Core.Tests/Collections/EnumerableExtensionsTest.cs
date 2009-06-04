// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Collections
{
  [TestFixture]
  public sealed class EnumerableExtensionsTest
  {
    [Test]
    public void BatchTest()
    {
      var source = InstanceGenerationUtils<int>.GetInstances(new Random(), 0.1).Take(258);
      const int initialBatchSize = 8;
      const int maximalBatchSize = 32;
      const int fastFirstCount = 10;
      var result = source.Batch(fastFirstCount, initialBatchSize, maximalBatchSize);
      Assert.AreEqual(fastFirstCount, result.TakeWhile(e => !(e is List<int>)).Count());
      var batchSize = initialBatchSize;
      Assert.IsTrue(result.Skip(fastFirstCount).All(e => {
        var r = ((List<int>) e).Count==batchSize;
        if(batchSize < maximalBatchSize)
          batchSize *= 2;
        return r;
      }));
      Assert.AreEqual(batchSize, 32);
    }

    [Test]
    public void ApplyBeforeAndAfter()
    {
      const int totalCount = 256;
      const int batchSize = 32;
      var source = InstanceGenerationUtils<int>.GetInstances(new Random(), 0.1).Take(totalCount);
      var batches = source.Batch(0, batchSize, batchSize);
      var count = 0;
      Assert.AreEqual(totalCount / batchSize, batches.ApplyBeforeAndAfter(() => count++, null).Count());
      Assert.AreEqual(totalCount / batchSize + 1, count);
      count = 0;
      Assert.AreEqual(totalCount / batchSize, batches.ApplyBeforeAndAfter(null, () => count++).Count());
      Assert.AreEqual(totalCount / batchSize + 1, count);
    }
  }
}