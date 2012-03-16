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
  public class StreamPageProviderPerformanceTest : IndexPageProviderTestBase
  {
    private bool useMemoryStream;

    protected override bool IsSerializable
    {
      get { return true; }
    }

    protected override Stream Serialize<TKey, TValue>(ref Index<TKey, TValue> tree)
    {
      Stream stream;
      if (useMemoryStream)
        stream = new MemoryStream();
      else
        stream = new FileStream("Tree.bin", FileMode.Create);
      StreamPageProvider<TKey, TValue> streamPageProvider = new StreamPageProvider<TKey, TValue>(stream);
      Index<TKey, TValue> storedTree = new Index<TKey, TValue>(streamPageProvider);
      storedTree.Serialize(tree);

      // Using stored tree as deserialized (partially deserialization).
      tree = storedTree;
      return stream;
    }

    [Test]
    public void Test()
    {
      useMemoryStream = true;
      BaseTest();
      useMemoryStream = false;
      BaseTest();
    }
  }
}
