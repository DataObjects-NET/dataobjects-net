// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.04

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DuplicateIndex;

namespace Xtensive.Orm.Tests.Storage
{
  namespace DuplicateIndex
  {
    public interface ISlave : IEntity
    {
      [Field]
      Master Master { get; set;}
    }

    [HierarchyRoot]
    public class Slave : Entity, ISlave
    {
      [Field, Key]
      public int Id { get; private set; }
      public Master Master { get; set; }
    }

    [HierarchyRoot]
    public class Master : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }
  }

  public class DuplicateIndexTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Master).Assembly, typeof(Master).Namespace);
      return config;
    }

    [Test]
    public void Test()
    {
      
    }
  }
}