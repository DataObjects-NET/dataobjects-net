// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.12.02

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.Northwind;

namespace Xtensive.Storage.Tests
{
  public class NorthwindTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.ObjectModel.Northwind");
      return config;
    }

    [Test]
    public void FillTest()
    {
      DataBaseFiller.Fill(Domain);
    }
  }
}