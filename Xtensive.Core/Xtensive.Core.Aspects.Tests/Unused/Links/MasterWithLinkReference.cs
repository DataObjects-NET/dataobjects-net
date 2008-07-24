// Copyright (C) 2007 Xtensive LLC.
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
  public class MasterWithLinkReference : IMaster
  {
    #region ILinkOwner Members
    public LinkedOperationRegistry Operations
    {
      get { return  null; }
    }
    #endregion

    [Link("Slave", "Master", LinkType.OneToOne)]
    protected LinkedReference<MasterWithLinkReference, ISlave> slave;
    
    private string name;

    public string Name
    {
      get { return name;}
      set { name = value;}
    }

    public ISlave Slave
    {
      get { return slave.Value;}
      set { slave.Value = value;}
    }

    public override string ToString()
    {
      return Name;
    }

    public LinkedReference<MasterWithLinkReference, ISlave> SlaveReference
    {
      get { return slave;}
    }

    public MasterWithLinkReference(string name)
    {
      this.name = name;
      slave = new LinkedReference<MasterWithLinkReference, ISlave>(this, "Slave");
    }
  }
}
