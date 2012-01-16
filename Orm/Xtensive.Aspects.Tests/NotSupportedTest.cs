// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.10.30

using System;
using Xtensive.Aspects.Helpers;
using NUnit.Framework;
using Xtensive.Testing;

namespace Xtensive.Aspects.Tests
{
  [TestFixture]
  public class NotSupportedTest
  {
    class TestClass
    {
      private string value;

      public string Value
      {
        get
        {
          return value;
        }
        [NotSupported("Property setter is not supported!.")]
        set
        {
          this.value = value;
        }
      }

      [NotSupported("Method is not supported!.")]
      public string Method(int i)
      {
        return i.ToString();
      }

      public TestClass(string value)
      {
        this.value = value;
      }
    }

    [Test]
    public void Test()
    {
      var testClass = new TestClass("345");
      Assert.AreEqual("345", testClass.Value);
      AssertEx.ThrowsInvalidOperationException(() => testClass.Value = "12");
      Assert.AreEqual("345", testClass.Value);
      AssertEx.ThrowsInvalidOperationException(() => testClass.Method(2));
    }
  }
}