// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.07.27

using NUnit.Framework;
namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class LinkTest
  {
    [Test, Ignore("Not implemented yet.")]
    public void SimpleLinkTest()
    {
      LinkSample sample1 = new LinkSample();
      LinkSample sample2 = new LinkSample();
      sample1.Items.Add(sample1);
      Assert.IsTrue(sample1.Items.Count==1);
      Assert.IsTrue(sample2.Items.Count==1);
      Assert.IsTrue(sample1.Items.Contains(sample2));
      Assert.IsTrue(sample2.Items.Contains(sample1));
    }
  }
}