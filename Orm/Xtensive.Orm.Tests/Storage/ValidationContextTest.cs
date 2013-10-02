// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.15

using System;
using NUnit.Framework;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ValidationContextTestModel;

namespace Xtensive.Orm.Tests.Storage.ValidationContextTestModel
{
  [Serializable]
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

namespace Xtensive.Orm.Tests.Storage
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
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          Validatable first = null;
          Validatable second;

          AssertEx.Throws<ValidationFailedException>(() => {
            first = new Validatable();
            session.Validate();
          });

          first.IsValid = true;
          session.Validate();

          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void ForceValidationTest()
    {
      using (var session = Domain.OpenSession()) {
        var transactionScope = session.OpenTransaction();

        AssertEx.Throws<ValidationFailedException>(() => {
          var obj = new Validatable {IsValid = false};
          session.Validate();
        });

      }
    }

    [Test]
    public void TransactionFailTest()
    {
      using (var session = Domain.OpenSession()) {
        var transactionScope = session.OpenTransaction();

        try {
          var obj = new Validatable();
          session.Validate();
        }
        catch (ValidationFailedException) {
        }

        transactionScope.Complete();

        AssertEx.Throws<ValidationFailedException>(transactionScope.Dispose);
      }
    }
  }
}