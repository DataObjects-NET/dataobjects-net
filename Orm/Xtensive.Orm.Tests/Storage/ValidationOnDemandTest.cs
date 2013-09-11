// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.30

using System;
using NUnit.Framework;
using Xtensive.Orm.Validation;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ValidationContextTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class ValidationOnDemandTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.ValidationMode = ValidationMode.OnDemand;
      config.Types.Register(typeof (Validatable));
      return config;
    }

    [Test]
    public void InvalidCommitTest()
    {
      using (var session = Domain.OpenSession()) {
        
        var transactionScope = session.OpenTransaction();

        var first = new Validatable {IsValid = true};
        var second = new Validatable {IsValid = false};
        var third = new Validatable {IsValid = true};

        transactionScope.Complete();

        AssertEx.Throws<InvalidOperationException>(transactionScope.Dispose);
      }
    }

    [Test]
    public void ValidCommitTest()
    {
      using (var session = Domain.OpenSession()) {
        using(var transactionScope = session.OpenTransaction()) {

          var entity = new Validatable {IsValid = false};
          entity.IsValid = true;

          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void EnforceValidationTest()
    {
      using (var session = Domain.OpenSession()) {
        var transactionScope = session.OpenTransaction();

        var entity = new Validatable {IsValid = false};
        AssertEx.Throws<ValidationFailedException>(session.Validate);
        AssertEx.Throws<ValidationFailedException>(session.Validate);

        entity.IsValid = true;
        session.Validate();

        entity.IsValid = false;
        AssertEx.Throws<ValidationFailedException>(session.Validate);
        
        transactionScope.Complete();
        AssertEx.Throws<ValidationFailedException>(transactionScope.Dispose);
      }
    }



  }
}