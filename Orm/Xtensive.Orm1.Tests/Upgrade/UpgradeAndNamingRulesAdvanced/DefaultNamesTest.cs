// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.05

using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced
{
  public sealed class DefaultNamesTest : TestBase
  {
    protected override NamingConvention CreateNamingConvention()
    {
      var namingConvension = new NamingConvention {
        LetterCasePolicy = LetterCasePolicy.Default
      };
      return namingConvension;
    }
  }
}