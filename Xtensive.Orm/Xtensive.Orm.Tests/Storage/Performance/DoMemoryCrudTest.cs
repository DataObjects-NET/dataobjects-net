// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.30

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.Performance
{
  [TestFixture]
  [Explicit]
  public class DoMemoryCrudTest : DoCrudTest
  {
    protected override DomainConfiguration CreateConfiguration()
    {
      return DomainConfigurationFactory.CreateForCrudTest("memory");
    }
  }
}