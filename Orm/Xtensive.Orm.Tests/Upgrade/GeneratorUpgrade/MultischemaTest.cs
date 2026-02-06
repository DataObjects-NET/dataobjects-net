// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.10

using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade
{
  public class MultischemaTest : SimpleSchemaTest
  {
    private const string DefaultSchema = WellKnownSchemas.SqlServerDefaultSchema;
    private const string AlternativeSchema = WellKnownSchemas.Schema1;

    protected override void ApplyCustomConfigurationSettings(DomainConfiguration configuration)
    {
      base.ApplyCustomConfigurationSettings(configuration);
      configuration.DefaultSchema = DefaultSchema;
      var namespaces = configuration.Types
        .Where(t => t.Namespace.Contains("Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade"))
        .GroupBy(t => t.Namespace)
        .Select(g => g.Key)
        .ToArray();
      if (namespaces.Length == 0) {
        configuration.MappingRules.Map("Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1").ToSchema(DefaultSchema);
        configuration.MappingRules.Map("Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2").ToSchema(AlternativeSchema);
      }
      else {
        configuration.MappingRules.Map(namespaces[0]).ToSchema(DefaultSchema);
        configuration.MappingRules.Map(namespaces[1]).ToSchema(AlternativeSchema);
      }
    }
  }
}