// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.09

using System;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedReference;
using Xtensive.Core.Links.LinkedReference.Operations;

namespace Xtensive.Core.Aspects.Tests.Links
{ 
  public class OneToOneMaster: TestObjectBase, ILinkOwner, IMaster
  {
    #region ILinkedOperationsOwner Members
    static LinkedOperationRegistry linkedOperations = new LinkedOperationRegistry();
    public LinkedOperationRegistry Operations
    {
      get { return linkedOperations; }
    }
    #endregion

    private ISlave slave;

    public ISlave Slave
    {
      get { return slave; }
      set {
        PropertyChangeArg<OneToOneMaster, ISlave> arg = new PropertyChangeArg<OneToOneMaster, ISlave>(this, slave, value);
        setSlaveOperation.Execute(ref arg);
      }
    }

    public static SetReferenceOperation<OneToOneMaster, ISlave> setSlaveOperation = new SetReferenceOperation<OneToOneMaster, ISlave>(
        linkedOperations, "Slave", "Master", LinkType.OneToOne,
        delegate(OneToOneMaster owner, ISlave oldValue, ISlave newValue) {
          owner.OnChanging("Slave", oldValue, newValue);
        },
        delegate(OneToOneMaster owner, ISlave oldValue, ISlave newValue) {
          owner.slave = newValue;
          owner.OnSet("Slave", oldValue, newValue);
        },
        delegate(OneToOneMaster owner, ISlave oldValue, ISlave newValue) {
          owner.OnChanged("Slave", oldValue, newValue);
        },
        delegate(OneToOneMaster owner) {
          return owner.Slave;
        }
      );

    // Constructors

    public OneToOneMaster(string name)
      : base(name)
    {
    }
  }
}