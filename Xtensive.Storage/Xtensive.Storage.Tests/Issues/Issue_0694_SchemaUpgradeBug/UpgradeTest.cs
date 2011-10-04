// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using M1 = Xtensive.Storage.Tests.Issues.Issue_0694_SchemaUpgradeBug.Model.Version1;
using M2 = Xtensive.Storage.Tests.Issues.Issue_0694_SchemaUpgradeBug.Model.Version2;
using M3 = Xtensive.Storage.Tests.Issues.Issue_0694_SchemaUpgradeBug.Model.Version3;
using NUnit.Framework;

namespace Xtensive.Storage.Tests.Issues.Issue_0694_SchemaUpgradeBug
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (Session.Open(domain)) {
        using (var tx = Transaction.Open()) {
          var status = new M1.Status() {Title = "Status"};
          var media = new M1.Media() {Title = "Media", Data = "Data"};
          media.Statuses.Add(status);
          tx.Complete();
        }
      }
    }

    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (Session.Open(domain))
      {
        using (Transaction.Open())
        {
          var status = Query.All<M2.Status>().SingleOrDefault();
          var newMedia = Query.All<M2.NewMedia>().SingleOrDefault();
          var newMediaTricky = Query.All<M2.Content>().Where(c => c.Title == "Media").SingleOrDefault();

          int statusCount = Query.All<M2.Status>().Count();
          int statusAssociationCount = status != null ? status.AssociatedContent.Count() : 0;
          int newMediaCount = Query.All<M2.NewMedia>().Count();

          Assert.IsNotNull(status);
          Assert.IsNull(newMedia);
          Assert.IsNull(newMediaTricky);
          Assert.AreEqual(1, statusCount);
          Assert.AreEqual(0, newMediaCount);
          Assert.AreEqual(0, statusAssociationCount);
        }
      }
    }
    
    [Test]
    public void UpgradeToVersion3Test()
    {
      BuildDomain("3", DomainUpgradeMode.Perform);
      using (Session.Open(domain)) {
        using (Transaction.Open()) {
          // No version-to-version hints, so all the types are removed
          var status = Query.All<M3.Status>().FirstOrDefault();
          var newMedia = Query.All<M3.NewMedia>().FirstOrDefault();
          var newMediaTricky = Query.All<M3.Content>().FirstOrDefault();

          int statusCount = Query.All<M3.Status>().Count();
          int statusAssociationCount = status!=null ? status.AssociatedContent.Count() : 0;
          int newMediaCount = Query.All<M3.NewMedia>().Count();

          Assert.IsNull(status);
          Assert.IsNull(newMedia);
          Assert.IsNull(newMediaTricky);
          Assert.AreEqual(0, statusCount);
          Assert.AreEqual(0, newMediaCount);
          Assert.AreEqual(0, statusAssociationCount);
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(M1.Content).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}