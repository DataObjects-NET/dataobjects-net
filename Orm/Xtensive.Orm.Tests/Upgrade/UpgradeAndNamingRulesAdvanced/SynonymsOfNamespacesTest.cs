// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.05

using System.Collections.Generic;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced
{
  public sealed class SynonymsOfNamespacesTest : TestBase
  {
    protected override NamingConvention CreateNamingConvention()
    {
      var namingConvention = new NamingConvention {
        NamespacePolicy = NamespacePolicy.Synonymize
      };
      MakeNamespaceSynonyms(namingConvention.NamespaceSynonyms);
      return namingConvention;
    }

    private void MakeNamespaceSynonyms(IDictionary<string, string> synonyms)
    {
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.AddNewFieldModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.AddNewTypeModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RenameFieldModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RenameTypeModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel", "Model");

      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.AddNewFieldModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.AddNewTypeModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RemoveFieldModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RemoveTypeModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RenameFieldModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.RenameTypeModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.MoveFieldToLastDescendantModel", "Model");
      synonyms.Add("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels.MoveFieldToFirstDescendantModel", "Model");
    }
  }
}