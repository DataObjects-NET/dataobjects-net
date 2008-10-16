// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.BugReports.Bug0002_Model;

namespace Xtensive.Storage.Tests.BugReports.Bug0002_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class X : Entity
  {
    [Field(LazyLoad = true)]
    public int ID { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.BugReports
{
  [TestFixture]
  public class Bug0002_PrimaryKeyWithLazyLoad : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(X).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      TypeInfo ti = Domain.Model.Types[typeof (X)];
      Assert.IsFalse(ti.Fields["ID"].IsLazyLoad);
    }
  }
}