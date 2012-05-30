﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.25

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ValidationContextClientProfileModel;
using Xtensive.Testing;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Orm.Tests.Storage
{
  namespace ValidationContextClientProfileModel
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

      public Validatable(Session session)
        : base(session)
      {
      }
    }
  }

  public class ValidationContextClientProfileTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Sessions.Default.Options = SessionOptions.ClientProfile;
      configuration.Types.Register(typeof (Validatable));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        Validatable first = null;
        Validatable second;

        AssertEx.Throws<AggregateException>(() => {
          using (var region = session.DisableValidation()) {
            first = new Validatable(session);
            region.Complete();
          }
        });

        using (var region = session.DisableValidation()) {
          first.IsValid = true;
          region.Complete();
        }
      }
    }

    [Test]
    public void CatchErrorAndRecoverTest()
    {
      using (var session = Domain.OpenSession()) {
        session.DisableValidation();
        var v = new Validatable(session);
        AssertEx.Throws<AggregateException>(session.Validate);
        v.IsValid = true;
        session.Validate();
      }
    }

    [Test]
    public void ForceValidationTest()
    {
      using (var session = Domain.OpenSession()) {
        AssertEx.Throws<Exception>(() => {
          using (var region = session.DisableValidation()) {
            var obj = new Validatable(session) {IsValid = false};
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
        try {
          using (var region = session.DisableValidation()) {
            var obj = new Validatable(session);
            region.Complete();
          }
        }
        catch (Exception) {
        }

        AssertEx.Throws<AggregateException>(session.SaveChanges);
      }
    }
  }
}