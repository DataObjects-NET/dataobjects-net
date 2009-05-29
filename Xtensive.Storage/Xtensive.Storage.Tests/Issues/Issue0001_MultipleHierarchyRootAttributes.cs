// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0001_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0001_Model
{
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, KeyField]
    public int ID { get; private set; }
  }

  [HierarchyRoot]
  public class Y : X
  {
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0001_MultipleHierarchyRootAttributes : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (X).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain result = null;
      AssertEx.Throws<DomainBuilderException>(() => result = base.BuildDomain(configuration));
      return result;
    }
  }
}