// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2009.09.17

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model;


namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0408_EntitySetNullReference 
  {
    [Test]
    public void Main()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      
      // Initialize domain
      Domain domain;
      try
      {
        var domainConfig = DomainConfigurationFactory.Create();
        domainConfig.NamingConvention.NamespacePolicy = NamespacePolicy.AsIs;

        // Load assemblies with persistent classes from configuration :
        Log.Info("Loading plugins...");

        domainConfig.Types.Register(typeof(QueueProcessor).Assembly, typeof(QueueProcessor).Namespace);

        domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
        domain = Domain.Build(domainConfig);
      }
      catch (DomainBuilderException e)
      {
        Log.Error("Domain build failed: " + e.ToString());
        throw;
      }

      string key = CreateObjects.CreateTestEchoQueueProcessor(domain);

      IList<object> workList = QueueProcessor.GetWork(key, domain);
      foreach (object workUnit in workList)
      {
        QueueProcessor.Execute(key, workUnit, domain);
      }
    }
  }
}