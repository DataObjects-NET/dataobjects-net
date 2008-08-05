// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.21

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ImplementConstructorEpilogueAspectTest
  {
    public class HandlerTargetBase
    {
      public int HandlerInvocationCount;
      public int CtorInvocationCount;

      protected void Handler(Type type)
      {
        Log.Info("In handler, Type={0}, GetType={1}", type.GetShortName(), GetType().GetShortName());
        HandlerInvocationCount++;
        if (type==GetType())
          CtorInvocationCount++;
      }

      [ImplementConstructorEpilogueAspect(typeof(HandlerTargetBase), "Handler")]
      public HandlerTargetBase()
      {
      }
    }

    public class HandlerTarget: HandlerTargetBase
    {
      [ImplementConstructorEpilogueAspect(typeof(HandlerTargetBase), "Handler")]
      public HandlerTarget()
      {
      }
    }

    [Test]
    public void Test()
    {
      var t = new HandlerTargetBase();
      Assert.AreEqual(1, t.HandlerInvocationCount);
      Assert.AreEqual(1, t.CtorInvocationCount);

      t = new HandlerTarget();
      Assert.AreEqual(2, t.HandlerInvocationCount);
      Assert.AreEqual(1, t.CtorInvocationCount);
    }
  }
}