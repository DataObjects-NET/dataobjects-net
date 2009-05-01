// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using InheritedKeyFieldModel;

namespace InheritedKeyFieldModel
{
  public class H0 : Entity
  {
    [Field]
    public int Id { get; private set; }
  }

  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class H1 : H0
  {
    
  }

  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class H2 : H0
  {
    
  }
}

namespace Xtensive.Storage.Tests.Model
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
//      catch (AggregateException e) {
//        Assert.AreEqual(1, e.Exceptions.Count);
//      }
//      return domain;
//    }
  }
}