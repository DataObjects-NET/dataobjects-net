// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Indexing.Storage.Model;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class StorageInfoTest
  {
    [Test]
    public void ConstructorTest()
    {
      StorageInfo storage = new StorageInfo("1");
      Assert.AreEqual("1", storage.Name);
    }
  }
}