// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using Xtensive.Core.Links;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class OneToManyOwner : TestObjectBase, ILinkOwner
  {
    #region ILinkedOperationsOwner Members
    static LinkedOperationRegistry linkedOperations;
    public LinkedOperationRegistry Operations
    {
      get {
        if (linkedOperations==null)
          linkedOperations = CreateOperationRegistry();
        return linkedOperations;
      }
    }
    #endregion

    TestSet<OneToManyOwner, OneToManyItem> items;

    public TestSet<OneToManyOwner, OneToManyItem> Items
    {
      get {
        if (linkedOperations==null)
          linkedOperations = CreateOperationRegistry();
        return items;
      }
    }
    
    public OneToManyOwner(string name)
      : base(name)
    {
      if (linkedOperations==null)
        linkedOperations = CreateOperationRegistry();
      items = new TestSet<OneToManyOwner, OneToManyItem>(this, "Items");
    }

    private static LinkedOperationRegistry CreateOperationRegistry()
    {
      LinkedOperationRegistry operations = new LinkedOperationRegistry();
      TestSet<OneToManyOwner, OneToManyItem>.ExportOperations(
        operations, "Items", "Owner", LinkType.OneToMany,
        delegate(OneToManyOwner owner) { return owner.items; });
      return operations;
    }

  }
}
