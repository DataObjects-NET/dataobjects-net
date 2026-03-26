using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.MysqlStrangeDoubleSemicolumnInCommandIssueModel;

namespace Xtensive.Orm.Tests.Issues.MysqlStrangeDoubleSemicolumnInCommandIssueModel
{
  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class Product : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public Guid UIndentifier { get; set; }

    public Product(Session session)
      : base(session)
    {
    }
  }

  public class DerivedProduct : Product
  {
    [Field]
    public TimeSpan TimeSpan { get; set; }

    public DerivedProduct(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class MysqlStrangeDoubleSemicolumnInCommandIssue : AutoBuildTest
  {
    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.MySql);

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Product));
      config.Types.Register(typeof(DerivedProduct));
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      return config;
    }

    [Test]
    public void BatchingTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.DbCommandExecuting += BatchingHandler;
        using (var transaction = session.OpenTransaction()) {
          var product = new Product(session) {
            Name = "BatchingTestProduct",
            CreationDate = DateTime.UtcNow,
            UIndentifier = Guid.NewGuid(),
          };

          var dProduct = new DerivedProduct(session) {
            Name = "BatchingTestDerivedProduct",
            CreationDate = DateTime.UtcNow,
            UIndentifier = Guid.NewGuid(),
            TimeSpan = TimeSpan.FromDays(3)
          };
          session.SaveChanges(); // split persist and transaction commit
          transaction.Complete();
        }
        session.Events.DbCommandExecuting -= BatchingHandler;

        using (var transaction = session.OpenTransaction()) {
          var allProducts = session.Query.All<Product>().ToList();
          Assert.That(allProducts.Count, Is.EqualTo(2));

          var derivedProducts = session.Query.All<DerivedProduct>().ToList();
          Assert.That(derivedProducts.Count, Is.EqualTo(1));
        }
      }
    }

    private void BatchingHandler(object sender, DbCommandEventArgs e)
    {
      var commandText = e.Command.CommandText;
      if (commandText.Contains("INSERT")) {
        var firstInsertIdx = commandText.IndexOf("INSERT", StringComparison.OrdinalIgnoreCase);
        var lastInsertIdx = commandText.LastIndexOf("INSERT", StringComparison.OrdinalIgnoreCase);
        if (firstInsertIdx == lastInsertIdx) {
          throw new Exception("No batching happened");
        }
        Assert.That(commandText.Contains(";;"), Is.False, $"Command = {commandText}");
      }
    }
  }
}
