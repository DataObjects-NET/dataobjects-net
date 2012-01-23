// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.07.24

using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Storage.Tests.Issues.IssueJira0165_Model;

namespace Xtensive.Storage.Tests.Issues.IssueJira0165_Model
{
  [HierarchyRoot]
  public class Class1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; set; }

    [Field]
    [FieldMapping("AnotherValue")]
    public int VaLue { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0165_CaseSensitiveFieldNames : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Class1).Assembly, typeof (Class1).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
          

          // Rollback
        }
      }
    }
  }
}