// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.26

using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.ExcludeFromAutoVersionTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace ExcludeFromAutoVersionTestModel
  {
    [HierarchyRoot]
    public class Versioned : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Version, Field]
      public int Version { get; private set; }

      [Field]
      public string Title { get; set; }

      [Field]
      public string Description { get; set; }

      protected override bool UpdateVersion(Entity changedEntity, Orm.Model.FieldInfo changedField)
      {
        if (changedField.Name=="Description")
          return false;
        return base.UpdateVersion(changedEntity, changedField);
      }
    }
  }

  public class ExcludeFromAutoVersionTest : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Versioned));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var v = new Versioned();
        var originalVersion = v.Version;
        v.Description = "foo";
        Assert.That(originalVersion, Is.EqualTo(v.Version));
        v.Title = "bar";
        Assert.That(originalVersion, Is.Not.EqualTo(v.Version));
      }
    }
  }
}