// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using Xtensive.Core.Links;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class ManyToManyRight : TestObjectBase, ILinkOwner
  {
    #region ILinkedOperationsOwner Members
    static LinkedOperationRegistry linkedOperations = new LinkedOperationRegistry();
    public LinkedOperationRegistry Operations
    {
      get
      {
        return linkedOperations;
      }
    }
    #endregion

    TestSet<ManyToManyRight, ManyToManyLeft> items;

    public TestSet<ManyToManyRight, ManyToManyLeft> LeftItems
    {
      get {return items;}
    }

    public ManyToManyRight(string name)
      : base(name)
    {
      this.items = new TestSet<ManyToManyRight, ManyToManyLeft>(this, "LeftItems");
    }

    static ManyToManyRight()
    {
      TestSet<ManyToManyRight, ManyToManyLeft>.ExportOperations(
        linkedOperations, "LeftItems", "RightItems", LinkType.ManyToMany,
        delegate(ManyToManyRight owner) {
          return owner.LeftItems;
        });
    }
  }
}
