// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.12.04

using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using TestCommon;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Localization.Tests.Model;

namespace Xtensive.Orm.Localization.Tests
{
  public class MultipleNodesTest : CommonModelTest
  {
    private const string DefaultNodeSchema = "dbo";
    private const string FirstNodeSchema = "Model1";
    private const string SecondNodeSchema = "Model2";
    private const string ThridNodeSchema = "Model3";
    private const string ForthNodeSchema = "Model4";

    private static CultureInfo EnglishCulture = new CultureInfo("en-US");
    private static string EnglishTitle = "Welcome!";
    private static string EnglishContent = "My dear guests, welcome to my birthday party!";

    private static CultureInfo SpanishCulture = new CultureInfo("es-ES");
    private static string SpanishTitle = "Bienvenido!";
    private static string SpanishContent = "Mis amigos mejores! Bienvenido a mi cumpleanos!";

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      foreach (var nodeConfiguration in BuildNodeConfigurations())
        domain.StorageNodeManager.AddNode(nodeConfiguration);
      return domain;
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (ILocalizable<>).Assembly);
      configuration.Types.Register(typeof (AutoBuildTest).Assembly, typeof (AutoBuildTest).Namespace);
      configuration.DefaultSchema = DefaultNodeSchema;
      return configuration;
    }

    [Test]
    public void Node1Test01()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode(WellKnown.DefaultNodeId);
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
          welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
          welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
          welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node1Test02()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode(WellKnown.DefaultNodeId);
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          var localization = welcomePage.Localizations[EnglishCulture];
          localization.Title = EnglishTitle;
          localization.Content = EnglishContent;

          localization = welcomePage.Localizations[SpanishCulture];
          localization.Title = SpanishTitle;
          localization.Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node2Test01()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node1");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
          welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
          welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
          welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node2Test02()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node1");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          var localization = welcomePage.Localizations[EnglishCulture];
          localization.Title = EnglishTitle;
          localization.Content = EnglishContent;

          localization = welcomePage.Localizations[SpanishCulture];
          localization.Title = SpanishTitle;
          localization.Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node3Test01()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node2");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
          welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
          welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
          welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node3Test02()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node2");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          var localization = welcomePage.Localizations[EnglishCulture];
          localization.Title = EnglishTitle;
          localization.Content = EnglishContent;

          localization = welcomePage.Localizations[SpanishCulture];
          localization.Title = SpanishTitle;
          localization.Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node4Test01()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node3");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
          welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
          welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
          welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node4Test02()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node3");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          var localization = welcomePage.Localizations[EnglishCulture];
          localization.Title = EnglishTitle;
          localization.Content = EnglishContent;

          localization = welcomePage.Localizations[SpanishCulture];
          localization.Title = SpanishTitle;
          localization.Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node5Test01()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node4");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
          welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
          welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
          welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Node5Test02()
    {
      using (var session = Domain.OpenSession()) {
        session.SelectStorageNode("Node4");
        using (var transaction = session.OpenTransaction()) {
          var welcomePage = new Page(session);
          var localization = welcomePage.Localizations[EnglishCulture];
          localization.Title = EnglishTitle;
          localization.Content = EnglishContent;

          localization = welcomePage.Localizations[SpanishCulture];
          localization.Title = SpanishTitle;
          localization.Content = SpanishContent;

          session.SaveChanges();
          transaction.Complete();
        }
      }
    }

    private IEnumerable<NodeConfiguration> BuildNodeConfigurations()
    {
      var first = new NodeConfiguration("Node1");
      first.SchemaMapping.Add(DefaultNodeSchema, FirstNodeSchema);
      first.UpgradeMode = DomainUpgradeMode.Recreate;

      var second = new NodeConfiguration("Node2");
      second.SchemaMapping.Add(DefaultNodeSchema, SecondNodeSchema);
      second.UpgradeMode = DomainUpgradeMode.Recreate;

      var third = new NodeConfiguration("Node3");
      third.SchemaMapping.Add(DefaultNodeSchema, ThridNodeSchema);
      third.UpgradeMode = DomainUpgradeMode.Recreate;

      var fourth = new NodeConfiguration("Node4");
      fourth.SchemaMapping.Add(DefaultNodeSchema, ForthNodeSchema);
      fourth.UpgradeMode = DomainUpgradeMode.Recreate;

      return new[] { first, second, third, fourth };
    }
  }
}
