// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.04.16

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using ModelNamespace = Xtensive.Orm.Tests.Issues.IssueJira0647_StoredDomainModelMappingsUpdateBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0647_StoredDomainModelMappingsUpdateBugModel
{
  namespace MainTestModel
  {
    [HierarchyRoot]
    public class SuperUniqueTestClass1 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Text { get; set; }

      [Field]
      public double Value { get; set; }
    }

    [HierarchyRoot]
    public class SuperUniqueTestClass2 : Entity
    {
      [Field, Key]
      public int Id { get; set; }
    }
  }

  namespace HintTest
  {

    #region BaseVersion

    namespace BaseVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public long Number { get; set; }

        [Field]
        public string Text { get; set; }
      }
    }

    #endregion

    #region ChangeFieldTypeVersion

    namespace ChangeFieldTypeVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Number { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new ChangeFieldTypeHint(typeof (Order), "Number"));
        }
      }
    }

    #endregion

    #region CopyFieldVersion

    namespace CopyFieldVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public long Number { get; set; }

        [Field]
        public string Text { get; set; }

        [Field]
        public string Description { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new CopyFieldHint(typeof (BaseVersion.Order), "Text", typeof (Order), "Description"));
        }
      }
    }

    #endregion

    #region MoveFieldVersion

    namespace MoveFieldVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public string Description { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public long Number { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new MoveFieldHint(typeof (BaseVersion.Order), "Text", typeof (Customer), "Description"));
        }
      }
    }

    #endregion

    #region RecycledTypeVersion

    namespace RecycledTypeVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public long Number { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RecycledTypeHint(typeof (Order)));
        }
      }
    }

    #endregion

    #region RemoveFieldVersion

    namespace RemoveFieldVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Text { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveFieldHint(typeof (BaseVersion.Order), "Number"));
        }
      }
    }

    #endregion

    #region RemoveTypeVersion

    namespace RemoveTypeVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0647_StoredDomainModelMappingsUpdateBugModel.HintTest.BaseVersion.Order"));
        }
      }
    }

    #endregion

    #region RenameFieldVersion

    namespace RenameFieldVersion
    {
      [HierarchyRoot]
      public class Customer : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string LastName { get; set; }

        [Field]
        public string FirstName { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public long Number { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameFieldHint(typeof (Customer), "Name", "LastName"));
        }
      }
    }

    #endregion

    #region RenameTypeVersion

    namespace RenameTypeVersion
    {
      [HierarchyRoot]
      public class Person : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Order : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public long Number { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0647_StoredDomainModelMappingsUpdateBugModel.HintTest.BaseVersion.Customer", typeof (Person)));
        }
      }
    }

    #endregion
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public sealed class IssueJira0647_StoredDomainModelMappingsUpdateBug
  {
    private class ClientNodeConfiguration
    {
      public string Name { get; set; }
      public ConnectionInfo ConnectionInfo { get; set; }
      public string InitializationSql { get; set; }
      public string DefaultSchema { get; set; }
    }

    private ClientNodeConfiguration alpha;
    private ClientNodeConfiguration beta;
    private ClientNodeConfiguration main;

#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      BuildNodeConfigurationsMetadata();
    }

    [Test]
    public void MainTest()
    {
      var configuration = BuildConfiguration(alpha, DomainUpgradeMode.Recreate, typeof (ModelNamespace.MainTestModel.SuperUniqueTestClass1));

      using (var domain = Domain.Build(configuration)) {
        Assert.That(domain.Configuration.IsMultischema, Is.EqualTo(true));
        Assert.That(domain.Configuration.IsMultidatabase, Is.EqualTo(false));
        domain.StorageNodeManager.AddNode(BuildNodeConfiguration(domain.Configuration, alpha, DomainUpgradeMode.Recreate));
        domain.StorageNodeManager.AddNode(BuildNodeConfiguration(domain.Configuration, beta, DomainUpgradeMode.Recreate));

        CheckNode(domain, alpha.Name, false);
        CheckNode(domain, beta.Name, true);
      }
    }

    [Test]
    public void ChangeFieldTypeHintTest()
    {
      BuildBaseVersion();
      BuildUpgradedVersion(typeof (ModelNamespace.HintTest.ChangeFieldTypeVersion.Customer));
    }

    [Test]
    public void CopyFieldHintTest()
    {
      BuildBaseVersion();
      BuildUpgradedVersion(typeof (ModelNamespace.HintTest.CopyFieldVersion.Customer));
    }

    [Test]
    public void MoveFieldHintTest()
    {
      BuildBaseVersion();
      BuildUpgradedVersion(typeof (ModelNamespace.HintTest.MoveFieldVersion.Customer));
    }

    [Test]
    public void RemoveFieldHintTest()
    {
      BuildBaseVersion();
      BuildUpgradedVersion(typeof (ModelNamespace.HintTest.RemoveFieldVersion.Customer));
    }

    [Test]
    public void RemoveTypeHintTest()
    {
      BuildBaseVersion();
      BuildUpgradedVersion(typeof (ModelNamespace.HintTest.RemoveTypeVersion.Customer));
    }

    [Test]
    public void RenameFieldHintTest()
    {
      BuildBaseVersion();
      BuildUpgradedVersion(typeof (ModelNamespace.HintTest.RenameFieldVersion.Customer));
    }

    [Test]
    public void RenameTypeHintTest()
    {
      BuildBaseVersion();
      BuildUpgradedVersion(typeof (ModelNamespace.HintTest.RenameTypeVersion.Person));
    }

    private void BuildBaseVersion()
    {
      var configuration = BuildConfiguration(main, DomainUpgradeMode.Recreate, typeof (ModelNamespace.HintTest.BaseVersion.Customer));
      using (var domain = Domain.Build(configuration)) {
        domain.StorageNodeManager.AddNode(BuildNodeConfiguration(domain.Configuration, alpha, DomainUpgradeMode.Recreate));
        domain.StorageNodeManager.AddNode(BuildNodeConfiguration(domain.Configuration, beta, DomainUpgradeMode.Recreate));

        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(alpha.Name);
          using (var tx = session.OpenTransaction()) {
            new ModelNamespace.HintTest.BaseVersion.Customer { Name = "CustomerName" };
            new ModelNamespace.HintTest.BaseVersion.Customer { Name = "Groznov" };
            new ModelNamespace.HintTest.BaseVersion.Order { Number = 99, Text = "Test order number 99" };
            new ModelNamespace.HintTest.BaseVersion.Order { Number = 1, Text = "Test order number 1" };
            tx.Complete();
          }
        }
      }
    }

    private void BuildUpgradedVersion(Type type)
    {
      var configuration = BuildConfiguration(main, DomainUpgradeMode.PerformSafely, type);
      using (var domain = Domain.Build(configuration)) {
        domain.StorageNodeManager.AddNode(BuildNodeConfiguration(domain.Configuration, alpha, DomainUpgradeMode.PerformSafely));
        domain.StorageNodeManager.AddNode(BuildNodeConfiguration(domain.Configuration, beta, DomainUpgradeMode.PerformSafely));
      }
    }

    private void CheckNode(Domain domain, string nodeName, bool spoiledExpected)
    {
      var node = domain.StorageNodeManager.GetNode(nodeName);
      Assert.IsNotNull(node);

      var expectedSchema = node.Configuration.SchemaMapping.Apply(domain.Configuration.DefaultSchema);

      var spoiledModel = domain.Model.ToStoredModel(node.TypeIdRegistry);
      spoiledModel.UpdateReferences();

      var currentModel = domain.Model.ToStoredModel(node.TypeIdRegistry);
      currentModel.UpdateReferences();
      currentModel.UpdateMappings(node.Configuration); //this operation suppose to be performed by DataObjects internally

      foreach (var typeInfo in currentModel.Types.Where(t => !t.IsSystem))
        Assert.AreEqual(typeInfo.MappingSchema, expectedSchema);

      var func = spoiledExpected ? new Action<string, string>(Assert.AreNotEqual) : new Action<string, string>(Assert.AreEqual);
      foreach (var typeInfo in spoiledModel.Types.Where(t => !t.IsSystem))
        func(typeInfo.MappingSchema, expectedSchema);
    }

    private NodeConfiguration BuildNodeConfiguration(DomainConfiguration domainConfiguration, ClientNodeConfiguration nodeConfiguration, DomainUpgradeMode upgradeMode)
    {
      var node = new NodeConfiguration(nodeConfiguration.Name);
      node.ConnectionInfo = nodeConfiguration.ConnectionInfo;
      node.ConnectionInitializationSql = nodeConfiguration.InitializationSql;
      node.UpgradeMode = upgradeMode;
      if (!domainConfiguration.DefaultSchema.IsNullOrEmpty() && !nodeConfiguration.DefaultSchema.IsNullOrEmpty())
        node.SchemaMapping.Add(domainConfiguration.DefaultSchema, nodeConfiguration.DefaultSchema);
      return node;
    }

    private DomainConfiguration BuildConfiguration(ClientNodeConfiguration nodeConfig, DomainUpgradeMode upgradeMode, Type modelType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.ConnectionInfo = nodeConfig.ConnectionInfo;
      configuration.ConnectionInitializationSql = nodeConfig.InitializationSql;
      configuration.UpgradeMode = upgradeMode;
      configuration.DefaultSchema = nodeConfig.DefaultSchema;
      configuration.Types.Register(modelType.Assembly, modelType.Namespace);
      return configuration;
    }

    private void BuildNodeConfigurationsMetadata()
    {
      var defaultConnection = DomainConfigurationFactory.Create().ConnectionInfo;
      main = new ClientNodeConfiguration {
        Name = "main",
        ConnectionInfo = ComposeConnectionToMasterDatabase(defaultConnection),
        InitializationSql = "USE [DO-Tests-1]",
        DefaultSchema = "dbo"
      };
      alpha = new ClientNodeConfiguration {
        Name = "alpha",
        ConnectionInfo = ComposeConnectionToMasterDatabase(defaultConnection),
        InitializationSql = "USE [DO-Tests-1]",
        DefaultSchema = "Model1"
      };

      beta = new ClientNodeConfiguration {
        Name = "beta",
        ConnectionInfo = ComposeConnectionToMasterDatabase(defaultConnection),
        InitializationSql = "USE [DO-Tests-2]",
        DefaultSchema = "Model2"
      };
    }

    private ConnectionInfo ComposeConnectionToMasterDatabase(ConnectionInfo baseConnectionInfo)
    {
      if (baseConnectionInfo.ConnectionUrl==null)
        throw new InvalidOperationException("Can't convert connection string based ConnectionInfo");

      var provider = baseConnectionInfo.ConnectionUrl.Protocol;
      var user = baseConnectionInfo.ConnectionUrl.User;
      var password = baseConnectionInfo.ConnectionUrl.Password;
      var host = baseConnectionInfo.ConnectionUrl.Host;
      var port = baseConnectionInfo.ConnectionUrl.Port;
      var database = "master";
      var parameters = baseConnectionInfo.ConnectionUrl.Params;

      string urlTemplate = "{0}://{1}{2}{3}/{4}{5}";
      var authenticationPartTemplate = "{0}:{1}@";

      var authentication = user.IsNullOrEmpty()
        ? string.Empty
        : string.Format(authenticationPartTemplate, user, password);

      var portPart = port==0
        ? string.Empty
        : ":" + port;

      var parametersPart = string.Empty;
      if (parameters.Count > 0) {
        parametersPart += "?";
        parametersPart = parameters.Aggregate(parametersPart, (current, parameter) => current + (parameter.Key + "=" + parameter.Value + "&"));
        parametersPart = parametersPart.TrimEnd('&');
      }

      var newUrl = string.Format(urlTemplate, provider, authentication, host, portPart, database, parametersPart);
      return new ConnectionInfo(newUrl);
    }
  }
}
