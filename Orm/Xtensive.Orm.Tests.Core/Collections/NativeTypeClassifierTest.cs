// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.02

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Tests;
using System.Linq;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class NativeTypeClassifierTest
  {
    [Test]
    public void CombinedTest()
    {
      var classifier = new NativeTypeClassifier<object>(false);

      var o = new object();
      classifier.Add(o);
      Assert.IsTrue(classifier.Contains(o));
      Assert.AreEqual(1, classifier.Count);
      Assert.AreEqual(1, classifier.ClassCount);
      Assert.AreEqual(1, classifier.GetItems<object>().Count());
      Assert.AreEqual(0, classifier.GetItems<IList>().Count());

      var l = new List<int>();
      classifier.Add(l);
      Assert.IsTrue(classifier.Contains(l));
      Assert.AreEqual(2, classifier.Count);
      Assert.AreEqual(8, classifier.ClassCount);
      Assert.AreEqual(2, classifier.GetItems<object>().Count());
      Assert.AreEqual(1, classifier.GetItems<IEnumerable<int>>().Count());

      classifier.Remove(l);
      Assert.IsTrue(classifier.Contains(o));
      Assert.AreEqual(1, classifier.Count);
      Assert.AreEqual(1, classifier.ClassCount);
      Assert.AreEqual(1, classifier.GetItems<object>().Count());
    }
  }
}