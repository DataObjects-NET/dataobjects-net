// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.30

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  [TestFixture]
  [Explicit]
  public class MemoryDOCrudTest : DOCrudTestBase
  {
    protected override DomainConfiguration CreateConfiguration()
    {
      return DomainConfigurationFactory.Create("memory");
    }
  }
}