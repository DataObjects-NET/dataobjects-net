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
      Assert.That(classifier.Contains(o), Is.True);
      Assert.That(classifier.Count, Is.EqualTo(1));
      Assert.That(classifier.ClassCount, Is.EqualTo(1));
      Assert.That(classifier.GetItems<object>().Count(), Is.EqualTo(1));
      Assert.That(classifier.GetItems<IList>().Count(), Is.EqualTo(0));

      var l = new List<int>();
      classifier.Add(l);
      Assert.That(classifier.Contains(l), Is.True);
      Assert.That(classifier.Count, Is.EqualTo(2));
      Assert.That(classifier.ClassCount, Is.EqualTo(10));
      Assert.That(classifier.GetItems<object>().Count(), Is.EqualTo(2));
      Assert.That(classifier.GetItems<IEnumerable<int>>().Count(), Is.EqualTo(1));

      classifier.Remove(l);
      Assert.That(classifier.Contains(o), Is.True);
      Assert.That(classifier.Count, Is.EqualTo(1));
      Assert.That(classifier.ClassCount, Is.EqualTo(1));
      Assert.That(classifier.GetItems<object>().Count(), Is.EqualTo(1));
    }
  }
}