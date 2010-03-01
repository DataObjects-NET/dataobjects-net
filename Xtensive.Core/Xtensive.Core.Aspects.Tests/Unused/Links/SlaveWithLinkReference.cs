// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using Xtensive.Core.Aspects;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedReference;
using System;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class SlaveWithLinkReference : ISlave, ILinkOwner
  {
    #region ILinkOwner Members
    public LinkedOperationRegistry Operations
    {
      get { return null;}
    }
    #endregion

    [Link("Master", "Slave", LinkType.OneToOne)]
    LinkedReference<SlaveWithLinkReference, IMaster> master;
    
    private string name;

    public string Name
    {
      get { return name;}
      set { name = value;}
    }

    public IMaster Master
    {
      get { return master.Value;}
      set { master.Value =value;}
    }

    public override string ToString()
    {
      return Name;
    }

    public LinkedReference<SlaveWithLinkReference, IMaster> MasterReference
    {
      get { return master;}
    }


    public SlaveWithLinkReference(string name)
    {
      this.name = name;
      master = new LinkedReference<SlaveWithLinkReference, IMaster>(this, "Master");
    }

   /* static LinkedOperationRegistry CreateOperationRegistry(LinkedOperationRegistry operations)
    {
      LinkedOperationRegistry registry = new LinkedOperationRegistry(operations);
      LinkedReference<SlaveWithLinkReference, IMaster>.ExportOperations(
        registry, "Master", "Slave", LinkType.OneToOne,
        delegate(SlaveWithLinkReference owner) {
          return owner.master;
        });
      return registry;
    }*/

  }
}
