// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.31

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Validation
{
  public class ValidationTest : AutoBuildTest
  {
    [HierarchyRoot(typeof (Generator), "ID")]
    public class Mouse : Entity
    {
      [Field]
      public int ID { get; set; }

      [Field]
      public int ButtonCount { get; set; }

      [Field]
      public int ScrollingCount { get; set; }

      public override void OnValidate()
      {
        base.OnValidate();

        if (ButtonCount<1)
          throw new InvalidOperationException("Button count can't be less than one.");

        if (ScrollingCount > ButtonCount)
          throw new InvalidOperationException("Scrolling count can't be greater then button count.");
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.Validation");
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      using (Domain.OpenSession()) {

        Mouse mouse;
        using(var t = Transaction.Open()) {
          mouse = new Mouse {ButtonCount = 1, ScrollingCount = 1};
          t.Complete();
        }

        try {
          using (var t = Transaction.Open()) {
            new Mouse {ButtonCount = 2, ScrollingCount = 3}; // Error
            new Mouse {ButtonCount = 5, ScrollingCount = 3};
            new Mouse {ButtonCount = 6, ScrollingCount = 3};
            new Mouse(); // Error
            new Mouse {ButtonCount = 7, ScrollingCount = 3};
            mouse.ScrollingCount = 2; // error

            t.Complete();
          }
        }
        catch (AggregateException e) {
          Assert.AreEqual(3 , e.Exceptions.Count);
        }
        
        using (Transaction.Open()) {
          new Mouse {ButtonCount = 2, ScrollingCount = 3};
        }
      }
    }
  }
}