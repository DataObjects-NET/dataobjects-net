// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.27

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.TranscationsTest
{
  public class TranscationsTest  : AutoBuildTest
  {    
    [HierarchyRoot(typeof (Generator), "ID")]
    public class Hexagon : Entity
    {
      [Field]
      public int ID { get; set; }

      [Field]
      public int Kwanza { get; set;}

      public void Wobble(int newKanza)
      {
        Kwanza = newKanza;
        throw new InvalidOperationException();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.TranscationsTest");
      return config;
    }

    [Test]
    public void RollbackCreationTest()
    {
      using (Domain.OpenSession()) {
        Hexagon hexagon;
        using (Transaction.Open()) {
          hexagon = new Hexagon();
        }
        Assert.AreEqual(PersistenceState.Inconsistent, hexagon.PersistenceState);
        AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 15; });
      }
    }

    [Test]
    public void RollbackRemovingTest()
    {
      using (Domain.OpenSession()) {
        Hexagon hexagon;
        using (var t = Transaction.Open()) {
          hexagon = new Hexagon();
          hexagon.Kwanza = 36;
          t.Complete();
        }
        
        using (Transaction.Open()) {
          hexagon.Remove();
          AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 20; });
        }

        hexagon.Kwanza = 14;
        Assert.AreEqual(14, hexagon.Kwanza);
      }
    }

    [Test]
    public void RollbackModifyingTest()
    {
      using (Domain.OpenSession()) {
        Hexagon hexagon;

        using (var t = Transaction.Open()) {
          hexagon = new Hexagon();
          hexagon.Kwanza = 3;
          t.Complete();
        }
        Assert.AreEqual(3, hexagon.Kwanza);
        
        using (Transaction.Open()) {
          hexagon.Kwanza = 11;
        }
        Assert.AreEqual(3, hexagon.Kwanza);
        Assert.AreEqual(PersistenceState.Persisted, hexagon.PersistenceState);

        using (Transaction.Open()) {
          hexagon.Kwanza = 12;
          Session.Current.Persist();
        }
        Assert.AreEqual(3, hexagon.Kwanza);

        try {
          hexagon.Wobble(18);
        }
        catch (InvalidOperationException) {}
        Assert.AreEqual(3, hexagon.Kwanza);
      }
    }
  }
}