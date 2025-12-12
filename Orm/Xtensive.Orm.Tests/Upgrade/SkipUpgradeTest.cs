// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.03.05

using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Tests.Upgrade.SkipUpgradeTestModel;
using System.Threading.Tasks;

namespace Xtensive.Orm.Tests.Upgrade.SkipUpgradeTestModel
{
  // In honor of our build agents :-)

  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
  public class BuildAgent : Entity
  {
    [Key, Field(Length = 100)]
    public string Name { get; private set; }

    [Field, Association(
      PairTo = "CompatibleAgents",
      OnOwnerRemove = OnRemoveAction.Clear,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<BuildConfiguration> CompatibleConfigurations { get; private set; }

    public BuildAgent(string name)
      : base(name)
    {
    }
  }

  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
  public class BuildConfiguration : Entity
  {
    [Key, Field(Length = 100)]
    public string Name { get; private set; }

    [Field]
    public EntitySet<BuildAgent> CompatibleAgents { get; private set; }

    public BuildConfiguration(string name)
      : base(name)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class SkipUpgradeTest
  {
    [Test]
    public void CombinedTest()
    {
      var configurationTemplate = DomainConfigurationFactory.Create();
      configurationTemplate.Types.RegisterCaching(typeof(BuildAgent).Assembly, typeof(BuildAgent).Namespace);

      var initialConfiguration = configurationTemplate.Clone();
      initialConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      Dictionary<Type, int> typeIdentifiers;
      using (var initialDomain = Domain.Build(initialConfiguration)) {
        typeIdentifiers = initialDomain.Model.Types
          .ToDictionary(type => type.UnderlyingType, type => type.TypeId);
        using (var session = initialDomain.OpenSession())
        using (var ts = session.OpenTransaction()) {
          new BuildAgent("Agent Smith")
            .CompatibleConfigurations.Add(new BuildConfiguration("SQL Server"));
          new BuildAgent("Agent Thompson")
            .CompatibleConfigurations.Add(new BuildConfiguration("PostgreSQL"));
          new BuildAgent("Agent Johnson")
            .CompatibleConfigurations.Add(new BuildConfiguration("Oracle"));
          ts.Complete();
        }
      }

      var testedConfiguration = configurationTemplate.Clone();
      testedConfiguration.UpgradeMode = DomainUpgradeMode.Skip;
      using (var testedDomain = Domain.Build(testedConfiguration)) {
        Assert.That(testedDomain.Model.Types.Count, Is.EqualTo(typeIdentifiers.Count));
        foreach (var typeInfo in testedDomain.Model.Types) {
          var oldTypeId = typeIdentifiers[typeInfo.UnderlyingType];
          var newTypeId = typeInfo.TypeId;
          Assert.That(newTypeId, Is.EqualTo(oldTypeId));
        }
        using (var session = testedDomain.OpenSession())
        using (var ts = session.OpenTransaction()) {
          var trinity = new BuildAgent("Trinity");
          Assert.That(session.Query.All<BuildAgent>().Count(), Is.EqualTo(4));
          Assert.That(session.Query.All<BuildConfiguration>().Count(), Is.EqualTo(3));
          foreach (var agent in session.Query.All<BuildAgent>().Where(a => a != trinity).ToList()) {
            foreach (var configuration in agent.CompatibleConfigurations.ToList()) {
              agent.CompatibleConfigurations.Remove(configuration);
              trinity.CompatibleConfigurations.Add(configuration);
            }
          }
          Assert.That(trinity.CompatibleConfigurations.Count(), Is.EqualTo(3));
          ts.Complete();
        }
      }
    }

    [Test]
    public async Task CombinedAsyncTest()
    {
      var configurationTemplate = DomainConfigurationFactory.Create();
      configurationTemplate.Types.RegisterCaching(typeof(BuildAgent).Assembly, typeof(BuildAgent).Namespace);

      var initialConfiguration = configurationTemplate.Clone();
      initialConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      Dictionary<Type, int> typeIdentifiers;
      using (var initialDomain = Domain.Build(initialConfiguration)) {
        typeIdentifiers = initialDomain.Model.Types
          .ToDictionary(type => type.UnderlyingType, type => type.TypeId);
        using (var session = initialDomain.OpenSession())
        using (var ts = session.OpenTransaction()) {
          new BuildAgent("Agent Smith")
            .CompatibleConfigurations.Add(new BuildConfiguration("SQL Server"));
          new BuildAgent("Agent Thompson")
            .CompatibleConfigurations.Add(new BuildConfiguration("PostgreSQL"));
          new BuildAgent("Agent Johnson")
            .CompatibleConfigurations.Add(new BuildConfiguration("Oracle"));
          ts.Complete();
        }
      }

      var testedConfiguration = configurationTemplate.Clone();
      testedConfiguration.UpgradeMode = DomainUpgradeMode.Skip;
      using (var testedDomain = await Domain.BuildAsync(testedConfiguration)) {
        Assert.That(testedDomain.Model.Types.Count, Is.EqualTo(typeIdentifiers.Count));
        foreach (var typeInfo in testedDomain.Model.Types) {
          var oldTypeId = typeIdentifiers[typeInfo.UnderlyingType];
          var newTypeId = typeInfo.TypeId;
          Assert.That(newTypeId, Is.EqualTo(oldTypeId));
        }
        using (var session = testedDomain.OpenSession())
        using (var ts = session.OpenTransaction()) {
          var trinity = new BuildAgent("Trinity");
          Assert.That(session.Query.All<BuildAgent>().Count(), Is.EqualTo(4));
          Assert.That(session.Query.All<BuildConfiguration>().Count(), Is.EqualTo(3));
          foreach (var agent in session.Query.All<BuildAgent>().Where(a => a != trinity).ToList()) {
            foreach (var configuration in agent.CompatibleConfigurations.ToList()) {
              agent.CompatibleConfigurations.Remove(configuration);
              trinity.CompatibleConfigurations.Add(configuration);
            }
          }
          Assert.That(trinity.CompatibleConfigurations.Count(), Is.EqualTo(3));
          ts.Complete();
        }
      }
    }
  }
}