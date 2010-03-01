// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.07.27

using System;
using Xtensive.Core.Aspects;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedSet;

namespace Xtensive.Core.Aspects.Tests
{
  public class LinkSample: ILinkOwner
  {
    #region ILinkOwner implementation (fake)

    public LinkedOperationRegistry Operations
    {
      get { throw new NotImplementedException(); }
    }

    #endregion

    [Link("Items", "Items", LinkType.ManyToMany)]
    private LinkedSet<LinkSample, LinkSample> items;

    public LinkedSet<LinkSample, LinkSample> Items
    {
      get { return items; }
    }

    public LinkSample()
    {
      items = new LinkedSet<LinkSample, LinkSample>(this, "Items");
    }
  }
}