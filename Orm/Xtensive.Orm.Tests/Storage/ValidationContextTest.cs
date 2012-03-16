// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.15

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ValidationContextTestModel;
using AggregateException = Xtensive.Core.AggregateException;

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

          AssertEx.Throws<AggregateException>(() => {
            using (var region = session.DisableValidation()) {
              first = new Validatable();
              region.Complete();
            }
          });

          using (var region = session.DisableValidation()) {
            first.IsValid = true;
            region.Complete();
          }

          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void ForceValidationTest()
    {
      using (var session = Domain.OpenSession()) {
        var transactionScope = session.OpenTransaction();

        AssertEx.Throws<Exception>(() => {
          using (var region = session.DisableValidation()) {
            var obj = new Validatable {IsValid = false};
            session.Validate();
            obj.IsValid = true;
            region.Complete();
          }
        });

      }
    }

    [Test]
    public void TransactionFailTest()
    {
      using (var session = Domain.OpenSession()) {
        var transactionScope = session.OpenTransaction();

        try {
          using (var region = session.DisableValidation()) {
            var obj = new Validatable();
            region.Complete();
          }
        }
        catch (Exception) { }

        transactionScope.Complete();

        AssertEx.Throws<InvalidOperationException>(transactionScope.Dispose);
      }
    }
    
  }
}