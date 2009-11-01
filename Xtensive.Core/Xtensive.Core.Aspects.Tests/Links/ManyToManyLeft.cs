// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using Xtensive.Core.Links;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class ManyToManyLeft : TestObjectBase, ILinkOwner
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

    TestSet<ManyToManyLeft, ManyToManyRight> rightItems;

    public TestSet<ManyToManyLeft, ManyToManyRight> RightItems
    {
      get { return rightItems; }
    }

    public ManyToManyLeft(string name)
      : base(name)
    {
      this.rightItems = new TestSet<ManyToManyLeft, ManyToManyRight>(this, "RightItems");
    }

    static ManyToManyLeft()
    {
      TestSet<ManyToManyLeft, ManyToManyRight>.ExportOperations(
        linkedOperations, "RightItems", "LeftItems", LinkType.ManyToMany,
        delegate(ManyToManyLeft owner) {
          return owner.RightItems;
        });
    }
  }
}
