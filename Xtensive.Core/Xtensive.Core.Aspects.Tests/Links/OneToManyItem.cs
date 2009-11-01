// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedReference;
using Xtensive.Core.Links.LinkedReference.Operations;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class OneToManyItem : TestObjectBase, ILinkOwner
  {
    private OneToManyOwner owner;

    public OneToManyOwner Owner
    {
      get { return owner; }
      set {
        PropertyChangeArg<OneToManyItem, OneToManyOwner> arg = new PropertyChangeArg<OneToManyItem, OneToManyOwner>(
          this, owner, value );
        setOwner.Execute(ref arg);
      }
    }

    #region ILinkedOperationsOwner Members
    static LinkedOperationRegistry linkedOperations;
    public LinkedOperationRegistry Operations
    {
      get {return linkedOperations;}
    }
    #endregion

    private static SetReferenceOperation<OneToManyItem, OneToManyOwner> setOwner;

    static OneToManyItem()
    {
      if (linkedOperations!=null)
        return;
      LinkedOperationRegistry operations = new LinkedOperationRegistry();
      setOwner = new SetReferenceOperation<OneToManyItem, OneToManyOwner>(
         operations, "Owner", "Items", LinkType.OneToMany,
         delegate(OneToManyItem item, OneToManyOwner oldValue, OneToManyOwner newValue)
         {
           item.OnChanging("Owner", oldValue, newValue);
         },
         delegate(OneToManyItem item, OneToManyOwner oldValue, OneToManyOwner newValue)
         {
           item.owner = newValue;
           item.OnSet("Owner", oldValue, newValue);
         },
         delegate(OneToManyItem item, OneToManyOwner oldValue, OneToManyOwner newValue)
         {
           item.OnChanged("Owner", oldValue, newValue);
         },
         delegate(OneToManyItem owner)
         {
           return owner.Owner;
         }
      );
      linkedOperations = operations;
    }

    public OneToManyItem(string name) : base(name)
    {
    }
  }
}
