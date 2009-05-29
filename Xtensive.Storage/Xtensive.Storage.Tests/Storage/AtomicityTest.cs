// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.29

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Integrity.Aspects;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Atomicity
{
  [HierarchyRoot]
  public class Cake : Entity
  {
    [Field, KeyField]
    public int ID { get; private set; }

    [Field]
    public bool IsTasty { get; set;}

    [Field]
    public int Vanilla { get; set;}

    [Field]
    public int Raisin { get; set;}

    [Atomic]
    public void Cook(int amountOfVanilla, int raisinQuantity, Action beforeCheck)
    {
      Vanilla = amountOfVanilla;
      Raisin = raisinQuantity;

      beforeCheck();
     
      if (Vanilla + Raisin > 10)
        throw new InvalidOperationException("Too large cake!");

      IsTasty = amountOfVanilla > 0 && raisinQuantity > 0;
    }

    [Atomic]
    public void ThrowAway()
    {
      this.Remove();
      throw new InvalidOperationException("Food can not be thrown away!");
    }

    public Cake()
    {
      IsTasty = false;
      Vanilla = 0;
      Raisin = 0;
    }
  }

  public class AtomicityTest : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.Atomicity");
      return config;
    }
   
      
    [Test]
    [Ignore("Atomicity is not implemented")]
    public void ModifyingTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          Cake cake = new Cake();

          try {
            cake.Cook(6, 7, delegate {});
          }
          catch (InvalidOperationException) {}

          Assert.AreEqual(0, cake.Vanilla);
          Assert.AreEqual(0, cake.Raisin);

          try {
            cake.Cook(3, 9, delegate { Session.Current.Persist(); });
          }
          catch (InvalidOperationException) {}

          Assert.AreEqual(0, cake.Vanilla);
          Assert.AreEqual(0, cake.Raisin);
        }
      }
    }

    [Test]
    [Ignore("Atomicity is not implemented")]
    public void RemovingTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          
          Cake cake = new Cake();
          try {
            cake.ThrowAway();
          }
          catch (InvalidOperationException) {}

          Assert.AreNotEqual(PersistenceState.Removed, cake.PersistenceState);

          cake.Cook(1, 4, delegate {});          
        }
      }
    }

  }
}