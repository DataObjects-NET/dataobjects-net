// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.14

using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  [TestFixture]
  [Explicit]
  public class DoOracleCrudTest : DoCrudTest
  {
    protected override DomainConfiguration CreateConfiguration()
    {
      return DomainConfigurationFactory.Create("oracle");
    }
  }
}