// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.Issues.Issue0003_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0003_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class X : Entity
  {
    [Field]
    public int? ID { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0003_NullablePrimaryKey : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(X).Namespace);
      return config;
    }

    protected override Domain BuildDomain(Xtensive.Storage.Configuration.DomainConfiguration configuration)
    {
      Domain domain = null;
      // Model builder mark primary key fields as not nullable automatically
      // AssertEx.Throws<DomainBuilderException>(() => domain = base.BuildDomain(configuration));
      domain = base.BuildDomain(configuration);
      Assert.IsFalse(domain.Model.Types["X"].Fields["ID"].IsNullable);
      return domain;
    }
  }
}