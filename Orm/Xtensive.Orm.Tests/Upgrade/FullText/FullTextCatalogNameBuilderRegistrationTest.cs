// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.07.12

using System;
using System.Linq;
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

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      nameBuilder = UpgradeContext.Services.FulltextCatalogNameBuilder;
    }

    public override void OnComplete(Domain domain)
    {
      domain.Extensions.Set(nameBuilder);
    }
  }

  public class EnabledCustomBuilder1 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled
    {
      get { return true; }
    }
  }
  public class EnabledCustomBuilder2 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled
    {
      get { return true; }
    }
  }

  public class DisabledCustomBuilder1 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled
    {
      get { return false; }
    }
  }

  public class DisabledCustomBuilder2 : FullTextCatalogNameBuilder
  {
    public override bool IsEnabled
    {
      get { return false; }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  public class FullTextCatalogNameBuilderRegistrationTest
  {
    [Test]
    public void NoUserImplementationsTest()
    {
      RunTest(Enumerable.Empty<Type>().ToArray(), typeof (FullTextCatalogNameBuilder));
    }

    [Test]
    public void NoEnabledUserImplementationsTest()
    {
      var resolvers = new[] {
        typeof (DisabledCustomBuilder1),
        typeof (DisabledCustomBuilder2)
      };

      RunTest(resolvers, typeof(FullTextCatalogNameBuilder));
    }

    [Test]
    public void SingleEnabledUserImplementationTest()
    {
      var resolvers = new[] {
        typeof (DisabledCustomBuilder1),
        typeof (DisabledCustomBuilder2),
        typeof (EnabledCustomBuilder1)
      };

      RunTest(resolvers, typeof(EnabledCustomBuilder1));
    }

    [Test]
    public void MultipleEnabledUserImplementationsTest()
    {
      var resolvers = new[] {
        typeof (DisabledCustomBuilder1),
        typeof (DisabledCustomBuilder2),
        typeof (EnabledCustomBuilder1),
        typeof (EnabledCustomBuilder2),
      };

      Assert.Throws<DomainBuilderException>(() => RunTest(resolvers, null));
    }

    private void RunTest(Type[] resolvers, Type expectedResolverType)
    {
      using(var domain = Domain.Build(BuildConfiguration(resolvers))) {
        var resolver = domain.Extensions.Get<IFullTextCatalogNameBuilder>();
        Assert.That(resolver, Is.InstanceOf(expectedResolverType));
      }
    }

    private DomainConfiguration BuildConfiguration(Type[] resolvers)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (ResolverCatcher));
      resolvers.ForEach(r => configuration.Types.Register(r));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
