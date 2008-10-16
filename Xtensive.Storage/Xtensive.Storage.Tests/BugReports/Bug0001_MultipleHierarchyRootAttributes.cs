// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.BugReports.Bug0001_Model;

namespace Xtensive.Storage.Tests.BugReports.Bug0001_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class X : Entity
  {
    [Field]
    public int ID { get; private set; }
  }

  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Y : X
  {
  }
}

namespace Xtensive.Storage.Tests.BugReports
{
  [TestFixture]
  public class Bug0001_MultipleHierarchyRootAttributes : AutoBuildTest
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
      AssertEx.Throws<AggregateException>(() => result = base.BuildDomain(configuration));
      return result;
    }
  }
}