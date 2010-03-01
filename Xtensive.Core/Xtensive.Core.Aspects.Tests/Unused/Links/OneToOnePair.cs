// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.09
 
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedReference;
using Xtensive.Core.Links.LinkedReference.Operations;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class OneToOnePair : TestObjectBase, ILinkOwner
  {
    static LinkedOperationRegistry linkedOperations = new LinkedOperationRegistry();
    public LinkedOperationRegistry Operations
    {
      get
      {
        return linkedOperations;
      }
    }


    private OneToOnePair pair;

    public OneToOnePair Pair
    {
      get { return pair; }
      set {
        PropertyChangeArg<OneToOnePair, OneToOnePair> arg = new PropertyChangeArg<OneToOnePair, OneToOnePair>(
          this, pair, value);
        pairSet.Execute(ref arg);
      }
    }

    private static SetReferenceOperation<OneToOnePair, OneToOnePair> pairSet = 
      new SetReferenceOperation<OneToOnePair, OneToOnePair>(
        linkedOperations, "Pair", "Pair", LinkType.OneToOne,
        delegate(OneToOnePair owner, OneToOnePair oldValue, OneToOnePair newValue) {
          owner.OnChanging("Pair", oldValue, newValue);
        },
        delegate(OneToOnePair owner, OneToOnePair oldValue, OneToOnePair newValue) {
          owner.pair = newValue;
          owner.OnSet("Pair", oldValue, newValue);
        },
        delegate(OneToOnePair owner, OneToOnePair oldValue, OneToOnePair newValue) {
          owner.OnChanged("Pair", oldValue, newValue);
        },
        delegate(OneToOnePair owner)
        {
          return owner.Pair;
        }
      );

    // Constructors

    public OneToOnePair(string name)
      : base(name)
    {
    }
  }
}