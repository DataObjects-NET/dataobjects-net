// Copyright (C) 2003-2010 Xtensive LLC.
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
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Storage.Tests.Storage.ValidationContextTestModel
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
        using (var transactionScope = Transaction.Open()) {

          Validatable first = null;
          Validatable second;

          AssertEx.Throws<AggregateException>(() => {
            using (var region = Xtensive.Storage.Validation.Disable()) {
              first = new Validatable();
              region.Complete();
            }
          });

          using (var region = Xtensive.Storage.Validation.Disable()) {
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
      using (Session.Open(Domain)) {
        var transactionScope = Transaction.Open();

        AssertEx.Throws<Exception>(() => {
          using (var region = Xtensive.Storage.Validation.Disable()) {
            var obj = new Validatable {IsValid = false};
            Xtensive.Storage.Validation.Enforce();
            obj.IsValid = true;
            region.Complete();
          }
        });

      }
    }

    [Test]
    public void TransactionFailTest()
    {
      using (Session.Open(Domain)) {
        var transactionScope = Transaction.Open();

        try {
          using (var region = Xtensive.Storage.Validation.Disable()) {
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