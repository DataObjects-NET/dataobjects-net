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

        try {
          using (var t = Transaction.Open()) {
            new Mouse {ButtonCount = 2, ScrollingCount = 3};
            new Mouse {ButtonCount = 5, ScrollingCount = 3};
            new Mouse {ButtonCount = 1, ScrollingCount = 2};

            t.Complete();
          }
        }
        catch (AggregateException e) {
          Assert.AreEqual(2, e.Exceptions.Count);
        }
        
        using (Transaction.Open()) {
          new Mouse {ButtonCount = 2, ScrollingCount = 3};
        }
      }
    }
  }
}