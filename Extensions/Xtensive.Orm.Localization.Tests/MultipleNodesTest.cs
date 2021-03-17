// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.04

using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using TestCommon;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Localization.Tests.Model;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Localization.Tests
{
  public class MultipleNodesTest : CommonModelTest
  {
    private const string Node1Id = "Node1";
    private const string Node2Id = "Node2";
    private const string Node3Id = "Node3";
    private const string Node4Id = "Node4";

    private const string DefaultNodeSchema = "dbo";
    private const string FirstNodeSchema = "Model1";
    private const string SecondNodeSchema = "Model2";
    private const string ThridNodeSchema = "Model3";
    private const string ForthNodeSchema = "Model4";

    private static readonly CultureInfo EnglishCulture = new CultureInfo("en-US");
    private static readonly string EnglishTitle = "Welcome!";
    private static readonly string EnglishContent = "My dear guests, welcome to my birthday party!";

    private static readonly CultureInfo SpanishCulture = new CultureInfo("es-ES");
    private static readonly string SpanishTitle = "Bienvenido!";
    private static readonly string SpanishContent = "Mis amigos mejores! Bienvenido a mi cumpleanos!";

    protected override void CheckRequirements() =>
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Multischema);

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      foreach (var nodeConfiguration in BuildNodeConfigurations()) {
        _ = domain.StorageNodeManager.AddNode(nodeConfiguration);
      }
      return domain;
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(ILocalizable<>).Assembly);
      configuration.Types.Register(typeof(LocalizationBaseTest).Assembly, typeof(LocalizationBaseTest).Namespace);
      configuration.DefaultSchema = DefaultNodeSchema;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void Node1Test01()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
        welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
        welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
        welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node1Test02()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        var localization = welcomePage.Localizations[EnglishCulture];
        localization.Title = EnglishTitle;
        localization.Content = EnglishContent;

        localization = welcomePage.Localizations[SpanishCulture];
        localization.Title = SpanishTitle;
        localization.Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node2Test01()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node1Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
        welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
        welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
        welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node2Test02()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node1Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        var localization = welcomePage.Localizations[EnglishCulture];
        localization.Title = EnglishTitle;
        localization.Content = EnglishContent;

        localization = welcomePage.Localizations[SpanishCulture];
        localization.Title = SpanishTitle;
        localization.Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node3Test01()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node2Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
        welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
        welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
        welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node3Test02()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node2Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        var localization = welcomePage.Localizations[EnglishCulture];
        localization.Title = EnglishTitle;
        localization.Content = EnglishContent;

        localization = welcomePage.Localizations[SpanishCulture];
        localization.Title = SpanishTitle;
        localization.Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node4Test01()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node3Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
        welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
        welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
        welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node4Test02()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node3Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        var localization = welcomePage.Localizations[EnglishCulture];
        localization.Title = EnglishTitle;
        localization.Content = EnglishContent;

        localization = welcomePage.Localizations[SpanishCulture];
        localization.Title = SpanishTitle;
        localization.Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node5Test01()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node4Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
        welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
        welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
        welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

        session.SaveChanges();
      }
    }

    [Test]
    public void Node5Test02()
    {
      var selectedNode = Domain.StorageNodeManager.GetNode(Node4Id);
      using (var session = selectedNode.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var welcomePage = new Page(session);
        var localization = welcomePage.Localizations[EnglishCulture];
        localization.Title = EnglishTitle;
        localization.Content = EnglishContent;

        localization = welcomePage.Localizations[SpanishCulture];
        localization.Title = SpanishTitle;
        localization.Content = SpanishContent;

        session.SaveChanges();
      }
    }

    private IEnumerable<NodeConfiguration> BuildNodeConfigurations()
    {
      var first = new NodeConfiguration(Node1Id);
      first.SchemaMapping.Add(DefaultNodeSchema, FirstNodeSchema);
      first.UpgradeMode = DomainUpgradeMode.Recreate;

      var second = new NodeConfiguration(Node2Id);
      second.SchemaMapping.Add(DefaultNodeSchema, SecondNodeSchema);
      second.UpgradeMode = DomainUpgradeMode.Recreate;

      var third = new NodeConfiguration(Node3Id);
      third.SchemaMapping.Add(DefaultNodeSchema, ThridNodeSchema);
      third.UpgradeMode = DomainUpgradeMode.Recreate;

      var fourth = new NodeConfiguration(Node4Id);
      fourth.SchemaMapping.Add(DefaultNodeSchema, ForthNodeSchema);
      fourth.UpgradeMode = DomainUpgradeMode.Recreate;

      return new[] { first, second, third, fourth };
    }
  }
}
