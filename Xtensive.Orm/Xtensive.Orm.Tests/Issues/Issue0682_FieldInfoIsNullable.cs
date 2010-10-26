// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.19

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0680;

namespace Xtensive.Orm.Tests.Issues.Issue0680
{
  [HierarchyRoot]
  public class Base : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 64, Nullable = false)]
    public string SysName { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue680Test : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Base).Assembly, typeof (Base).Namespace);
      return config;
    }

    [Test]
    public void BuildTest()
    {
      var qweType = typeof (Base);
      var qweTypeInfo = Domain.Model.Types[qweType];
      var sysNameFieldInfo = qweTypeInfo.Fields["SysName"];
      Assert.IsFalse(sysNameFieldInfo.IsNullable);
    }
  }
}