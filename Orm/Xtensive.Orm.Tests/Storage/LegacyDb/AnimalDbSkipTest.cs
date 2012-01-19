// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.05

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Storage.LegacyDb
{
  public class AnimalDbSkipTest : AnimalDbBaseTest
  {
    protected override DomainUpgradeMode GetUpgradeMode()
    {
      return DomainUpgradeMode.LegacySkip;
    }
  }
}