// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.15

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.ValidationContextTestModel;

namespace Xtensive.Storage.Tests.Storage.ValidationContextTestModel
{
  [HierarchyRoot]
  public class Validatable : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public bool IsValid { get; set; }

    protected override void OnValidate()
    {
      if (!IsValid)
        throw new Exception("Instance is invalid.");
    }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class ValidationContextTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Validatable).Assembly, typeof (Validatable).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        var transactionScope = Transaction.Open();

        Validatable first;
        Validatable second;

        AssertEx.Throws<AggregateException>(() => {
          using (var region = Xtensive.Storage.Validation.Disable()) {
            first = new Validatable();
            region.Complete();
          }
        });

        using (var region = Xtensive.Storage.Validation.Disable()) {
          second = new Validatable {IsValid = true};
          region.Complete();
        }

        transactionScope.Complete();

        AssertEx.Throws<InvalidOperationException>(transactionScope.Dispose);
      }
    }
  }
}