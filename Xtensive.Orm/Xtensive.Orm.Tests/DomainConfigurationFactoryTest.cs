// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.19

using NUnit.Framework;
using System.Text;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests
{
  [TestFixture]
  public class DomainConfigurationFactoryTest : AutoBuildTest
  {
    private DomainConfiguration config;

    [TestFixtureSetUp]
    public override void TestFixtureSetUp()
    {
      config = BuildConfiguration();
    }

    [Test]
    public void MainTest()
    {
      var result = new StringBuilder();
      result.Append("ConnectionString: ").AppendLine(config.ConnectionInfo.ToString());
      result.Append("ForeignKeyMode: ").AppendLine(config.ForeignKeyMode.ToString());
      Log.Error(result.ToString());
    }
  }
}