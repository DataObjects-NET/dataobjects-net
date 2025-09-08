// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using M1 = Xtensive.Orm.Tests.Issues.Issue_0694_SchemaUpgradeBug.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.Issue_0694_SchemaUpgradeBug.Model.Version2;
using M3 = Xtensive.Orm.Tests.Issues.Issue_0694_SchemaUpgradeBug.Model.Version3;
using NUnit.Framework;



namespace Xtensive.Orm.Tests.Issues.Issue_0694_SchemaUpgradeBug
{
  [TestFixture]
  public class UpgradeTest
  {
    [SetUp]
    public void SetUp()
    {
      using (var domain = BuildDomain("1", DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var status = new M1.Status() { Title = "Status" };
        var media = new M1.Media() { Title = "Media", Data = "Data" };
        _ = media.Statuses.Add(status);
        tx.Complete();
      }
    }

    [Test]
    [IgnoreIfGithubActions(StorageProvider.Firebird)]
    public void UpgradeToVersion2Test()
    {
      using (var domain = BuildDomain("2", DomainUpgradeMode.Perform))
      using (var session = domain.OpenSession())
      using (session.OpenTransaction()) {
        var status = session.Query.All<M2.Status>().SingleOrDefault();
        var newMedia = session.Query.All<M2.NewMedia>().SingleOrDefault();
        var newMediaTricky = session.Query.All<M2.Content>().Where(c => c.Title == "Media").SingleOrDefault();

        var statusCount = session.Query.All<M2.Status>().Count();
        var statusAssociationCount = status != null ? status.AssociatedContent.Count() : 0;
        var newMediaCount = session.Query.All<M2.NewMedia>().Count();

        Assert.IsNotNull(status);
        Assert.IsNull(newMedia);
        Assert.IsNull(newMediaTricky);
        Assert.AreEqual(1, statusCount);
        Assert.AreEqual(0, newMediaCount);
        Assert.AreEqual(0, statusAssociationCount);
      }
    }

    [Test, Ignore("Default behavior changed. Namespace-only renames are tracked automatically.")]
    public void UpgradeToVersion3Test()
    {
      using (var domain = BuildDomain("3", DomainUpgradeMode.Perform))
      using (var session = domain.OpenSession())
      using (session.OpenTransaction()) {
        // No version-to-version hints, so all the types are removed
        var status = session.Query.All<M3.Status>().FirstOrDefault();
        var newMedia = session.Query.All<M3.NewMedia>().FirstOrDefault();
        var newMediaTricky = session.Query.All<M3.Content>().FirstOrDefault();

        var statusCount = session.Query.All<M3.Status>().Count();
        var statusAssociationCount = status != null ? status.AssociatedContent.Count() : 0;
        var newMediaCount = session.Query.All<M3.NewMedia>().Count();

        Assert.IsNull(status);
        Assert.IsNull(newMedia);
        Assert.IsNull(newMediaTricky);
        Assert.AreEqual(0, statusCount);
        Assert.AreEqual(0, newMediaCount);
        Assert.AreEqual(0, statusAssociationCount);
      }
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var ns = typeof(M1.Content).Namespace;
      var nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.BuildInParallel = false;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        return Domain.Build(configuration);
      }
    }
  }
}