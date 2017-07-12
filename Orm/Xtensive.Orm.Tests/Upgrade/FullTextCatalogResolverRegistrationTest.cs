﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.FullTextCatalogResolverRegistrationTestModel;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.FullTextCatalogResolverRegistrationTestModel
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
    private IFulltextCatalogResolver resolver;

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      resolver = UpgradeContext.Services.FulltextCatalogResolver;
    }

    public override void OnComplete(Domain domain)
    {
      domain.Extensions.Set(resolver);
    }
  }

  public class EnabledCustomResolver1 : FullTextCatalogResolver
  {
    public override bool IsEnabled
    {
      get { return true; }
    }
  }
  public class EnabledCustomResolver2 : FullTextCatalogResolver
  {
    public override bool IsEnabled
    {
      get { return true; }
    }
  }

  public class DisabledCustomResolver1 : FullTextCatalogResolver
  {
    public override bool IsEnabled
    {
      get { return false; }
    }
  }

  public class DisabledCustomResolver2 : FullTextCatalogResolver
  {
    public override bool IsEnabled
    {
      get { return false; }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  public class FullTextCatalogResolverRegistrationTest
  {
    [Test]
    public void NoUserImplementationsTest()
    {
      RunTest(Enumerable.Empty<Type>().ToArray(), typeof(FullTextCatalogResolver));
    }

    [Test]
    public void NoEnabledUserImplementationsTest()
    {
      var resolvers = new[] {
        typeof (DisabledCustomResolver1),
        typeof (DisabledCustomResolver2)
      };

      RunTest(resolvers, typeof(FullTextCatalogResolver));
    }

    [Test]
    public void SingleEnabledUserImplementationTest()
    {
      var resolvers = new[] {
        typeof (DisabledCustomResolver1),
        typeof (DisabledCustomResolver2),
        typeof (EnabledCustomResolver1)
      };

      RunTest(resolvers, typeof(EnabledCustomResolver1));
    }

    [Test]
    [ExpectedException(typeof (DomainBuilderException))]
    public void MultipleEnabledUserImplementationsTest()
    {
      var resolvers = new[] {
        typeof (DisabledCustomResolver1),
        typeof (DisabledCustomResolver2),
        typeof (EnabledCustomResolver1),
        typeof (EnabledCustomResolver2),
      };

      RunTest(resolvers, null);
    }

    private void RunTest(Type[] resolvers, Type expectedResolverType)
    {
      using(var domain = Domain.Build(BuildConfiguration(resolvers))) {
        var resolver = domain.Extensions.Get<IFulltextCatalogResolver>();
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
