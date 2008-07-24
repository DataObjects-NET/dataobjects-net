// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.01

using NUnit.Framework;

namespace Xtensive.Core.Aspects.Tests.Links
{
  [TestFixture]
  public class TableColumnsTests
  {
    [Test]
    public void Test()
    {
      Table t1 = new Table();
      Column c1 = new Column();
      Column c2 = new Column();
      c1.Table = t1;
      Assert.AreEqual(t1, c1.Table);
      Assert.IsTrue(t1.Columns.Contains(c1));
    }
  }
}