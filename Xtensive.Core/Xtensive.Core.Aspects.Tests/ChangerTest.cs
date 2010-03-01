// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.20

using NUnit.Framework;
using Xtensive.Core.Notifications;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ChangerTest
  {
    ChangerSample sample;

    [SetUp]
    public void Setup()
    {
      sample = new ChangerSample("Mr.", "Alex", 27);
    }

    [Test]
    public void PropertiesTest()
    {
      sample.ResetChangeCounters();
      sample.Name = "Another Alex";
      Assert.IsFalse(sample.IsChanging);
      Assert.AreEqual(sample.ChangeCount, 1);

      sample.ResetChangeCounters();
      sample.Age = 28;
      Assert.IsFalse(sample.IsChanging);
      Assert.AreEqual(sample.ChangeCount, 1);
    }

    [Test]
    public void MethodTest()
    {
      sample.ResetChangeCounters();
      sample.SetAll("Mr.", "Again Alex", 27);
      Assert.IsFalse(sample.IsChanging);
      Assert.AreEqual(sample.ChangeCount, 1);
    }

    [Test]
    public void ChangingGetterTest()
    {
      sample.ResetChangeCounters();
      Log.Info("Getting AgeWithChangingGetter: {0}", sample.AgeWithChangingGetter);
      Assert.IsFalse(sample.IsChanging);
      Assert.AreEqual(sample.ChangeCount, 1);
    }
  }
}