// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.04.16

using System;
using System.Collections.Concurrent;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Tests.Issues.IssueJira0647_StoredDomainModelMappingsUpdateBugModel;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Issues.IssueJira0647_StoredDomainModelMappingsUpdateBugModel
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

  public class Upgrader : UpgradeHandler
  {
    public override void OnComplete(Domain domain)
    {
      var capturer = CaptureScope.CurrentCapturer;
      if (capturer!=null) {
        var nodeName = (UpgradeContext.NodeConfiguration!=null)
          ? UpgradeContext.NodeConfiguration.NodeId
          : WellKnown.DefaultNodeId;
        var extractedModel = UpgradeContext.ExtractedDomainModel;
        var currentModel = domain.Model.ToStoredModel();
        currentModel.UpdateMappings(UpgradeContext.NodeConfiguration);//this operation suppose to be performed by DataObjects internally
        capturer.Capture(nodeName, extractedModel, currentModel);
      }
    }
  }

  #region Supporting classes
  public class CaptureScope : Scope<ModelCapturer>
  {
    public static ModelCapturer CurrentCapturer
    {
      get { return CurrentContext; }
    }

    public ModelCapturer Capturer
    {
      get { return Context; }
    }

    public CaptureScope(ModelCapturer capturer)
      : base(capturer)
    {
    }
  }

  public class ModelInfo
  {
    public string NodeName { get; private set; }
    public StoredDomainModel ExtractedModel { get; private set; }
    public StoredDomainModel CurrentModel { get; private set; }

    public ModelInfo(string nodeName, StoredDomainModel extractedDomainModel, StoredDomainModel currentDomainModel)
    {
      NodeName = nodeName;
      ExtractedModel = extractedDomainModel;
      CurrentModel = currentDomainModel;
    }
  }

  public class ModelCapturer
  {
    private readonly ConcurrentDictionary<string, ModelInfo> capturedItems = new ConcurrentDictionary<string, ModelInfo>();

    public ModelInfo this[string name]
    {
      get {
        ModelInfo info;
        if (capturedItems.TryGetValue(name, out info))
          return info;
        return null;
      }
    }

    public CaptureScope Activate()
    {
      return new CaptureScope(this);
    }

    public void Capture(string name, StoredDomainModel extractedDomainModel, StoredDomainModel currentDomainModel)
    {
      capturedItems.AddOrUpdate(name, new ModelInfo(name, extractedDomainModel, currentDomainModel), (key, info) => info);
    }
  }
  #endregion
}

namespace Xtensive.Orm.Tests.Issues
{
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

    [Test]
    public void MainTest()
    {
      var configuration = BuildConfiguration(alpha);
      var capturer = new ModelCapturer();//will capture models after domain or node is built

      using (capturer.Activate())
      using (var domain = Domain.Build(configuration)) {
        Assert.That(domain.Configuration.IsMultischema, Is.EqualTo(true));
        Assert.That(domain.Configuration.IsMultidatabase, Is.EqualTo(false));

        var sameNodeAsDomain = BuildNodeConfiguration(domain.Configuration, alpha);
        domain.StorageNodeManager.AddNode(sameNodeAsDomain);

        var anotherNewNode = BuildNodeConfiguration(domain.Configuration, beta);
        domain.StorageNodeManager.AddNode(anotherNewNode);
      }
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      BuildNodeConfigurationsMetadata();
    }

    private NodeConfiguration BuildNodeConfiguration(DomainConfiguration domainConfiguration, ClientNodeConfiguration nodeConfiguration)
    {
      var node = new NodeConfiguration(nodeConfiguration.Name);
      node.ConnectionInfo = nodeConfiguration.ConnectionInfo;
      node.ConnectionInitializationSql = nodeConfiguration.InitializationSql;
      node.UpgradeMode = DomainUpgradeMode.Recreate;
      if (!nodeConfiguration.DefaultSchema.IsNullOrEmpty())
        node.SchemaMapping.Add(domainConfiguration.DefaultSchema, nodeConfiguration.DefaultSchema);
      return node;
    }

    private DomainConfiguration BuildConfiguration(ClientNodeConfiguration nodeConfig)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.ConnectionInfo = nodeConfig.ConnectionInfo;
      configuration.ConnectionInitializationSql = nodeConfig.InitializationSql;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = nodeConfig.DefaultSchema;
      configuration.Types.Register(typeof (SuperUniqueTestClass1).Assembly, typeof (SuperUniqueTestClass1).Namespace);
      return configuration;
    }

    private void BuildNodeConfigurationsMetadata()
    {
      var defaultConnection = DomainConfigurationFactory.Create().ConnectionInfo;
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
      var authenticationPartTemplate = "{0}:{2}@";
     
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
