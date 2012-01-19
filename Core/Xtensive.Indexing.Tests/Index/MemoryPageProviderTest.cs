// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.01.06

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Indexing;
using Xtensive.Indexing.Providers;

namespace Xtensive.Indexing.Tests.Index
{
  [TestFixture]
  public class MemoryProviderTest : IndexPageProviderTestBase
  {
    protected override bool IsSerializable
    {
      get { return true; }
    }

    protected override Stream Serialize<TKey, TValue>(ref Index<TKey, TValue> tree)
    {
      MemoryPageProvider<TKey, TValue> provider = new MemoryPageProvider<TKey, TValue>(false);
      Index<TKey, TValue> serializedTree = new Index<TKey, TValue>(provider);
      serializedTree.Serialize(tree);
      tree = serializedTree;
      return null;
    }

    [Test]
    public void Test()
    {
      BaseTest();
    }
  }
}
