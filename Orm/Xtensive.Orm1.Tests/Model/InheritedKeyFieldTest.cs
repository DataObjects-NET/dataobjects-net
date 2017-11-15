// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using System;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using InheritedKeyFieldModel;

namespace InheritedKeyFieldModel
{
  [Serializable]
  public class H0 : Entity
  {
    [Field]
    public virtual int Id { get; private set; }
  }

  [Serializable]
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

  [Serializable]
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
  public class InheritedKeyFieldTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (H0).Assembly, typeof (H0).Namespace);
      return config;
    }

//    protected override Domain BuildDomain(DomainConfiguration configuration)
//    {
//      Domain domain = null;
//      try {
//        domain = Domain.Build(configuration);
//      }
//      catch (DomainBuilderException e) {
//        Assert.AreEqual(1, e.Exceptions.Count);
//      }
//      return domain;
//    }
  }
}