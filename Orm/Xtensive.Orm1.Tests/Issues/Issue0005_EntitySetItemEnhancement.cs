// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.03

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.Issue0009_Model;

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0005_EntitySetItemEnhancement : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Book).Assembly, typeof (Book).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      TypeInfo type = Domain.Model.Types["Book-Authors-Author"];
      Assert.IsNotNull(type.Fields["Master"]);
      Assert.IsNotNull(type.Fields["Slave"]);
    }
  }
}