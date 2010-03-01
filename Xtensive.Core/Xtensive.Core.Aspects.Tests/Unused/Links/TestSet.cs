// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using NUnit.Framework;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedSet;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class TestSet<TOwner, TItem> : LinkedSet<TOwner, TItem>
    where TOwner : ILinkOwner
    where TItem : ILinkOwner
  {
    public override void OnClearing()
    {
      base.OnClearing();
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
      Assert.AreEqual(LinkTests.EventStage, LinkEventStage.Prologue);
    }

    public override void OnClear()
    {
      base.OnClear();
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
    }

    public override void OnRemoving(TItem item)
    {
      base.OnRemoving(item);
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
      Assert.AreEqual(LinkTests.EventStage, LinkEventStage.Prologue);
    }

    public override void OnRemove(TItem item)
    {
      base.OnRemove(item);
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
    }

    public override void OnAdding(TItem item)
    {
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
      Assert.AreEqual(LinkTests.EventStage, LinkEventStage.Prologue);
    }

    public override void OnAdd(TItem item)
    {
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
    }

    public TestSet(TOwner owner, string ownerPropertyName)
      : base(owner, ownerPropertyName)
    {
    }
  }
}
