// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using System;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using InheritedKeyFieldModel;
using NUnit.Framework;

namespace InheritedKeyFieldModel
{
  public class H0 : Entity
  {
    [Field]
    public virtual int Id { get; private set; }
  }

  [HierarchyRoot]
  public class H1 : H0
  {
    [Key]
    public override int Id
    {
      get
      {
        { return base.Id;}
      }
    }
  }

  [HierarchyRoot]
  public class H2 : H0
  {
    [Key]
    public override int Id
    {
      get
      {
        return base.Id;
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture, Category("Model")]
  public class InheritedKeyFieldTest : DomainBuildabilityTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (H0));
      config.Types.Register(typeof (H2));
      return config;
    }
  }
}