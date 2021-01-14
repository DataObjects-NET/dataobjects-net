// Copyright (C) 2017-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.07.12

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.FullTextCatalogNameBuilderRegistrationTestModel;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.FullTextCatalogNameBuilderRegistrationTestModel
{
  public class TestEntity : Entity
  {
    [Field,Key]
    public int Id { get; set; }

    [Field]
    public string Text { get; set; }
  }

  public class ResolverCatcher : UpgradeHandler
  {
    private IFullTextCatalogNameBuilder nameBuilder;

    public override bool CanUpgradeFrom(string oldVersion) => true;

    public override void OnPrepare() =>
      nameBuilder = UpgradeContext.Services.FulltextCatalogNameBuilder;

    public override void OnComplete(Domain domain) => domain.Extensions.Set(nameBuilder);
  }

  public class EnabledCustomBuilder1 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled => true;
  }
  public class EnabledCustomBuilder2 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled => true;
  }

  public class DisabledCustomBuilder1 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled => false;
  }

  public class DisabledCustomBuilder2 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled => false;
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  public class FullTextCatalogNameBuilderRegistrationTest
  {
    [Test]
    public void NoUserImplementationsTest() =>
      RunTest(Enumerable.Empty<Type>().ToArray(), typeof(FullTextCatalogNameBuilder));

    [Test]
    public async Task NoUserImplementationsAsyncTest() =>
      await RunTestAsync(Enumerable.Empty<Type>().ToArray(), typeof(FullTextCatalogNameBuilder));

    [Test]
    public void NoEnabledUserImplementationsTest()
    {
      var resolvers = new[] {
        typeof(DisabledCustomBuilder1),
        typeof(DisabledCustomBuilder2)
      };

      RunTest(resolvers, typeof(FullTextCatalogNameBuilder));
    }

    [Test]
    public async Task NoEnabledUserImplementationsAsyncTest()
    {
      var resolvers = new[] {
        typeof(DisabledCustomBuilder1),
        typeof(DisabledCustomBuilder2)
      };

      await RunTestAsync(resolvers, typeof(FullTextCatalogNameBuilder));
    }

    [Test]
    public void SingleEnabledUserImplementationTest()
    {
      var resolvers = new[] {
        typeof(DisabledCustomBuilder1),
        typeof(DisabledCustomBuilder2),
        typeof(EnabledCustomBuilder1)
      };

      RunTest(resolvers, typeof(EnabledCustomBuilder1));
    }

    [Test]
    public void MultipleEnabledUserImplementationsTest()
    {
      var resolvers = new[] {
        typeof(DisabledCustomBuilder1),
        typeof(DisabledCustomBuilder2),
        typeof(EnabledCustomBuilder1),
        typeof(EnabledCustomBuilder2),
      };

      _ = Assert.Throws<DomainBuilderException>(() => RunTest(resolvers, null));
    }

    [Test]
    public void MultipleEnabledUserImplementationsAsyncTest()
    {
      var resolvers = new[] {
        typeof(DisabledCustomBuilder1),
        typeof(DisabledCustomBuilder2),
        typeof(EnabledCustomBuilder1),
        typeof(EnabledCustomBuilder2),
      };

      _ = Assert.ThrowsAsync<DomainBuilderException>(async () => await RunTestAsync(resolvers, null));
    }

    private void RunTest(Type[] resolvers, Type expectedResolverType)
    {
      using(var domain = Domain.Build(BuildConfiguration(resolvers))) {
        var resolver = domain.Extensions.Get<IFullTextCatalogNameBuilder>();
        Assert.That(resolver, Is.InstanceOf(expectedResolverType));
      }
    }

    private async Task RunTestAsync(Type[] resolvers, Type expectedResolverType)
    {
      using (var domain = await Domain.BuildAsync(BuildConfiguration(resolvers))) {
        var resolver = domain.Extensions.Get<IFullTextCatalogNameBuilder>();
        Assert.That(resolver, Is.InstanceOf(expectedResolverType));
      }
    }

    private DomainConfiguration BuildConfiguration(Type[] resolvers)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(ResolverCatcher));
      resolvers.ForEach(r => configuration.Types.Register(r));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
