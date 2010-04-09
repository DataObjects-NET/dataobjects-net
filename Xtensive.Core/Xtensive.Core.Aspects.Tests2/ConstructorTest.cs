// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.09

using NUnit.Framework;
using Xtensive.Core.Aspects.Tests;

namespace Xtensive.Core.Aspects.Tests2
{
  [TestFixture]
  public class ConstructorTest
  {
    public class Derived : Base
    {
      
    }

    [Test]
    public void CombinedTest()
    {
      var derived = new Derived();
    }
  }
}