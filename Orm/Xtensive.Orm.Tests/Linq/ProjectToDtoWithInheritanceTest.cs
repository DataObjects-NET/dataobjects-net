// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.26

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.ProjectToDtoWithInheritanceTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace ProjectToDtoWithInheritanceTestModel
  {
    [HierarchyRoot]
    public class ProjectedEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Value { get; set; }

      [Field]
      public DateTime Timestamp { get; set; }
    }

    public class BaseEntityDto
    {
      public long Id { get; set; }

      public string Value { get; set; }
    }

    public class ChildEntityDto : BaseEntityDto
    {
      public DateTime Timestamp { get; set; }
    }
  }

  public class ProjectToDtoWithInheritanceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (ProjectedEntity));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new ProjectedEntity();
        new ProjectedEntity {Timestamp = DateTime.Now};
        var stampMin = new DateTime(2000, 1, 1);
        var query = session.Query.All<ProjectedEntity>()
          .Select(e => new ChildEntityDto {
            Id = e.Id,
            Value = e.Value,
            Timestamp = e.Timestamp
          })
          .Where(d => d.Timestamp >= stampMin)
          .Select(d => new BaseEntityDto {
            Id = d.Id,
            Value = d.Value
          });
        var result = query.ToList();
        Assert.That(result.Count, Is.EqualTo(1));
        tx.Complete();
      }
    }
  }
}