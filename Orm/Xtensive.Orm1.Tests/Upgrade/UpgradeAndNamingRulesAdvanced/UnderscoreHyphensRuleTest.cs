// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.05

using System.Collections.Generic;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced
{
  public sealed class UnderscoreHyphensRuleTest : TestBase
  {
    protected override NamingConvention CreateNamingConvention()
    {
      var namingConvention = new NamingConvention {
        NamespacePolicy = NamespacePolicy.Synonymize,
        NamingRules = NamingRules.UnderscoreHyphens
      };
      MakeNamespaceSynonyms(namingConvention.NamespaceSynonyms);
      
      return namingConvention;
    }

    private void MakeNamespaceSynonyms(IDictionary<string, string> synonyms)
    {
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.AddNewFieldModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.AddNewTypeModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RenameFieldModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RenameTypeModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel", "Tests-Model");

      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.AddNewFieldModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.AddNewTypeModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RemoveFieldModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RemoveTypeModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RenameFieldModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RenameTypeModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.MoveFieldToLastDescendantModel", "Tests-Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.MoveFieldToFirstDescendantModel", "Tests-Model");
    }
  }
}