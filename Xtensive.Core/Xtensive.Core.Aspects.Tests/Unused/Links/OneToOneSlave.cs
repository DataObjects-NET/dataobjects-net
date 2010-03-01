// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.09

using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedReference.Operations;
using Xtensive.Core.Links.LinkedReference;
using System;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class OneToOneSlave : TestObjectBase, ILinkOwner, ISlave
  {
    #region ILinkedOperationsOwner Members
    static LinkedOperationRegistry linkedOperations = new LinkedOperationRegistry();
    public LinkedOperationRegistry Operations
    {
      get {
        return linkedOperations;
      }
    }
    #endregion
    
    private IMaster master;

    public IMaster Master
    {
      get { return master; }
      set {
        PropertyChangeArg<OneToOneSlave, IMaster> arg = new PropertyChangeArg<OneToOneSlave, IMaster>(this, master, value);
        slaveSet.Execute(ref arg);
      }
    }

    private static SetReferenceOperation<OneToOneSlave, IMaster> slaveSet = 
      new SetReferenceOperation<OneToOneSlave, IMaster>(
        linkedOperations, "Master", "Slave", LinkType.OneToOne,
        delegate(OneToOneSlave owner, IMaster oldValue, IMaster newValue) {
          owner.OnChanging("Master", oldValue, newValue);
        },
        delegate(OneToOneSlave owner, IMaster oldValue, IMaster newValue) {
          owner.master = newValue;
          owner.OnSet("Master", oldValue, newValue);
        },
        delegate(OneToOneSlave owner, IMaster oldValue, IMaster newValue) {
          owner.OnChanged("Master", oldValue, newValue);
        },
        delegate(OneToOneSlave owner)
        {
          return owner.Master;
        }
      );

    // Constructors

    public OneToOneSlave(string name)
      : base(name)
    {
    }
  }
}