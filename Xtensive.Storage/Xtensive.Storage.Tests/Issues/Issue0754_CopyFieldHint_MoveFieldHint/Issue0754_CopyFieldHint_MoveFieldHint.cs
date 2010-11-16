using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using NUnit.Framework;


namespace Xtensive.Storage.Tests.Issues.Issue0754_CopyFieldHint_MoveFieldHint
{
  [TestFixture]
  public class Issue0754_CopyFieldHint_MoveFieldHint
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
      BuildDomain(typeof (ModelVersion1.A), DomainUpgradeMode.Recreate);
      using (Session.Open(domain)) {
        using (var tx = Transaction.Open()) {
          var acticle = new ModelVersion1.B();
          tx.Complete();
        }
      }
    }

    [Test]
    public void UpgradeTest()
    {
      BuildDomain(typeof (ModelVersion2.A), DomainUpgradeMode.PerformSafely);
      using (Session.Open(domain)) {
        using (Transaction.Open()) {
          Assert.AreEqual(1, Query.All<ModelVersion2.B>().Count());
        }
      }
    }

    private void BuildDomain(Type type, DomainUpgradeMode upgradeMode)
    {
      if (domain!=null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), type.Namespace);
      configuration.Types.Register(typeof (Upgrader));

      domain = Domain.Build(configuration);
    }
  }
}