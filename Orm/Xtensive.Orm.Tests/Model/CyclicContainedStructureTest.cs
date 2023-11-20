// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Model.ReferenceTestModel
{
  #region Cyclic referenced structures

  [Serializable]
  public class S1 : Structure
  {
    [Field]
    public S2 Value { get; set; }
  }

  [Serializable]
  public class S2 : Structure
  {
    [Field]
    public S3 Value { get; set; }
  }

  [Serializable]
  public class S3 : Structure
  {
    [Field]
    public S1 Value { get; set; }
  }

  #endregion

  #region Cyclic contained structures with inheritance

  [Serializable]
  public class Parent : Structure
  {
    [Field]
    public Child Value { get; set; }
  }

  [Serializable]
  public class Child : Parent
  {
  }

  #endregion
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class CyclicContainedStructureTest
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Model.ReferenceTestModel");
      var ex = Assert.Throws<DomainBuilderException>(() => Domain.Build(config));
      Assert.That(ex.Message.StartsWith("At least one loop have been found"), Is.True);
    }
  }
}