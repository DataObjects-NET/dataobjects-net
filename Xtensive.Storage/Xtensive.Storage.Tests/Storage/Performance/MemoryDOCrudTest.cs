// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.30

using System;
using System.Diagnostics;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  [Serializable]
  public class MemoryDOCrudTest : DOCrudTestBase
  {
    protected override DomainConfiguration CreateConfiguration()
    {
      return DomainConfigurationFactory.Create("memory");
    }
  }
}