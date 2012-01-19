// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0003_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0003_Model
{
  [Serializable]
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public int? ID { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0003_NullablePrimaryKey : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(X).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain = null;
      AssertEx.Throws<DomainBuilderException>(() => domain = base.BuildDomain(configuration));
      return domain;
    }
  }
}