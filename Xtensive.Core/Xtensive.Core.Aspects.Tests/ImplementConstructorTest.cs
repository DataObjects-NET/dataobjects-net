// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.01

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ImplementConstructorTest
  {
    [ImplementConstructor(typeof(string))]
    public class Base
    {
      public string Message { get; private set; }

      protected Base()
      {}

      protected Base(string message)
      {
        Message = message;
      }
    }

    public class Generic<T> : Base
    {
      public Generic()
      {}

      protected Generic(string message, bool ignore)
        : base(message)
      {}
    }

    public class GenericDescendant<T> : Generic<T>
    {
      public GenericDescendant()
      {}

      protected GenericDescendant(string message, bool ignore)
        : base(message, ignore)
      {}
    }

    public class Descendant : Generic<string>
    {
    }

    public class ManualDescendant : Generic<string>
    {
      protected ManualDescendant(string message)
        : base(message, true)
      {}
    }

    public class ManualDescendantInheritor : ManualDescendant
    {
      protected ManualDescendantInheritor(string message)
        : base(message)
      {}
    }

    [Test]
    public void CombinedTest()
    {
      var tDescendant = typeof (Descendant);
      var instance = tDescendant.Activate(null, "Created") as Descendant;
      Assert.IsNotNull(instance);
      Assert.AreEqual("Created", instance.Message);
    }
  }
}