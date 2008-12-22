// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.22

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class ModelTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.ObjectModel.NorthwindDO");
      return config;
    }

    [Test]
    public void Test()
    {
      Domain.Model.Dump();  
    }
  }
}