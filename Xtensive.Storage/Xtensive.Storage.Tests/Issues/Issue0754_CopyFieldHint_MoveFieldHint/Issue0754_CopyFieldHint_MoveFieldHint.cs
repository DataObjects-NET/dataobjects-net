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
          var acticle = new ModelVersion1.B(){Reference = new ModelVersion1.X()};
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
          var result = Query.All<ModelVersion2.B>().ToList();
          Assert.AreEqual(1, result.Count);
          Assert.IsNotNull(result[0].Reference);
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