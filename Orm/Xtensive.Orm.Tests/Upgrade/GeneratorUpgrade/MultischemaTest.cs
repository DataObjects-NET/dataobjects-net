// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using refModel = Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel;
using lessGeneratorsModel = Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.LessGenerators;

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
      var namespaces = configuration.Types.GroupBy(t => t.Namespace).Select(g => g.Key).ToArray();
      configuration.MappingRules.Map(namespaces[0]).ToSchema(DefaultSchema);
      configuration.MappingRules.Map(namespaces[1]).ToSchema(AlternativeSchema);
    }
  }
}