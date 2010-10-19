// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.21

using System;
using NUnit.Framework;
using Xtensive.Aspects.Helpers;
using Xtensive.Reflection;
using Xtensive.Testing;

namespace Xtensive.Aspects.Tests
{
  [TestFixture]
  public class ImplementConstructorEpilogueTest
  {
    [ImplementConstructorEpilogue(typeof(HandlerTargetBase), "Handler", "Error")]
    public class HandlerTargetBase
    {
      public static HandlerTargetBase LastInstance = null;
      public int HandlerInvocationCount;
      public int ErrorHandlerInvocationCount;
      public int CtorInvocationCount;

      protected void Handler(Type type)
      {
        Log.Info("In handler, Type={0}, GetType={1}", type.GetShortName(), GetType().GetShortName());
        HandlerInvocationCount++;
        if (type==GetType())
          CtorInvocationCount++;
      }

      protected void Error(Type type, Exception exception)
      {
        LastInstance = this;
        Log.Info("In error handler, Type={0}, GetType={1}", type.GetShortName(), GetType().GetShortName());
        ErrorHandlerInvocationCount++;
        if (type==GetType())
          CtorInvocationCount++;
      }

      public HandlerTargetBase(Exception toThrow)
      {
        if (toThrow!=null)
          throw toThrow;
      }
    }

    [ImplementConstructorEpilogue(typeof(HandlerTargetBase), "Handler", "Error")]
    public class HandlerTarget: HandlerTargetBase
    {
      public HandlerTarget(Exception toThrow)
        : base(null)
      {
        if (toThrow!=null)
          throw toThrow;
      }
    }

    [Test]
    public void Test()
    {
      var t = new HandlerTargetBase(null);
      Assert.AreEqual(1, t.HandlerInvocationCount);
      Assert.AreEqual(0, t.ErrorHandlerInvocationCount);
      Assert.AreEqual(1, t.CtorInvocationCount);

      t = new HandlerTarget(null);
      Assert.AreEqual(2, t.HandlerInvocationCount);
      Assert.AreEqual(0, t.ErrorHandlerInvocationCount);
      Assert.AreEqual(1, t.CtorInvocationCount);

      AssertEx.Throws<ApplicationException>(() => {
        new HandlerTargetBase(new ApplicationException("Test error."));
      });
      t = HandlerTargetBase.LastInstance;
      Assert.AreEqual(0, t.HandlerInvocationCount);
      Assert.AreEqual(1, t.ErrorHandlerInvocationCount);
      Assert.AreEqual(1, t.CtorInvocationCount);

      AssertEx.Throws<ApplicationException>(() => {
        new HandlerTarget(new ApplicationException("Test error."));
      });
      t = HandlerTargetBase.LastInstance;
      Assert.AreEqual(1, t.HandlerInvocationCount);
      Assert.AreEqual(1, t.ErrorHandlerInvocationCount);
      Assert.AreEqual(1, t.CtorInvocationCount);
    }
  }
}