// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.23

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0195_DeleteAndCreateWithValidationModel;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Issues.IssueJira0195_DeleteAndCreateWithValidationModel
{
  [HierarchyRoot]
  public class Guider : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    protected override void OnValidate()
    {
      if (IsRemoved)
        throw new InvalidOperationException("Should not validate removed entity");
    }

    public Guider(Guid id)
      : base(id)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0195_DeleteAndCreateWithValidation : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Guider).Assembly, typeof (Guider).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var id = Guid.NewGuid();
          new Guider(id) {Name = "123"};
          var g = session.Query.Single<Guider>(id);
          g.Remove();
          new Guider(id) {Name = "321"};
          Assert.IsTrue(g.IsRemoved); // Check that IsRemoved is accessible
          g.Validate(); // Check that Validate() is no-op for removed entities
          // rollback
        }
      }
    }
  
    [Test]
    public void RegressionTest()
    {
      // Ensure no regression:
      // it should be imposible to create entity with the same key
      // if old entity is not removed

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var id = Guid.NewGuid();
          var expected = new Guider(id) {Name = "123"};
          AssertEx.Throws<Exception>(() => new Guider(id) {Name = "321"});
          var actual = session.Query.All<Guider>().Single();
          Assert.AreSame(expected, actual);
          Assert.AreEqual("123", actual.Name);
        }
      }
    }
  }
}