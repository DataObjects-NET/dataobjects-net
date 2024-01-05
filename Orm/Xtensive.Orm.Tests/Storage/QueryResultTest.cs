// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.QueryResultTestModel;

namespace Xtensive.Orm.Tests.Storage.QueryResultTestModel
{
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class QueryResultTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Order));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new Order { Name = "A" };
        _ = new Order { Name = "B" };
        _ = new Order { Name = "C" };
        _ = new Order { Name = "D" };

        transaction.Complete();
      }
    }

    [Test]
    public void MultipleReadersTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        foreach (var query1 in session.Query.Execute(q => q.All<Order>())) {
          foreach(var query2 in session.Query.Execute(q => q.All<Order>().OrderBy(e=>e.Name))) {

          }
        }
      }
    }

    [Test]
    public async Task MultipleReadersAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        foreach (var query1 in await session.Query.ExecuteAsync(q => q.All<Order>())) {
          foreach (var query2 in await session.Query.ExecuteAsync(q => q.All<Order>().OrderBy(e => e.Name))) {

          }
        }
      }
    }

    [Test]
    public void ExecuteWithoutTransactionTest()
    {
      using (var session = Domain.OpenSession()) {
        var ex = Assert.Throws<InvalidOperationException>(() => session.Query.Execute(q => q.All<Order>()));
        Assert.That(ex.Message,
          Is.EqualTo(Strings.ExActiveTransactionIsRequiredForThisOperationUseSessionOpenTransactionToOpenIt));
      }
    }

    [Test]
    public void ExecuteWithoutTransactionAsyncTest()
    {
      using (var session = Domain.OpenSession()) {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
          async() => await session.Query.ExecuteAsync(q => q.All<Order>()));
        Assert.That(ex.Message,
          Is.EqualTo(Strings.ExActiveTransactionIsRequiredForThisOperationUseSessionOpenTransactionToOpenIt));
      }
    }

    [Test]
    public void EnumerationOutsideTransactionTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var transaction = session.OpenTransaction()) {
          result = session.Query.Execute(q => q.All<Order>());
        }

        _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
      }
    }

    [Test]
    public async Task EnumerationOutsideTransactionAsyncTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var transaction = session.OpenTransaction()) {
          result = await session.Query.ExecuteAsync(q => q.All<Order>());
        }

        _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
      }
    }

    [Test]
    public void EnumerationInClientProfileTest()
    {
      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);
      using (var session = Domain.OpenSession(configuration)) {
        QueryResult<Order> result;
        result = session.Query.Execute(q => q.All<Order>());
        _ = result.ToList();
      }
    }

    [Test]
    public async Task EnumerationInClientProfileAsyncTest()
    {
      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);
      using (var session = Domain.OpenSession(configuration)) {
        QueryResult<Order> result;
        result = await session.Query.ExecuteAsync(q => q.All<Order>());
        _ = result.ToList();
      }
    }

    [Test]
    public void EnumerationOutsideTransactionClientProfileTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        QueryResult<Order> result;
        using (var transaction = session.OpenTransaction()) {
          result = session.Query.Execute(q => q.All<Order>());
        }

        _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
      }
    }

    [Test]
    public async Task EnumerationOutsideTransactionClientProfileAsyncTest()
    {
      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);
      using (var session = Domain.OpenSession(configuration)) {
        QueryResult<Order> result;
        using (var transaction = session.OpenTransaction()) {
          result = await session.Query.ExecuteAsync(q => q.All<Order>());
        }

        _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
      }
    }

    [Test]
    public void EnumerationOutsideSessionTest()
    {
      QueryResult<Order> result;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        result = session.Query.Execute(q => q.All<Order>());
      }

      _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
    }

    [Test]
    public async Task EnumerationOutsideSessionAsyncTest()
    {
      QueryResult<Order> result;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        result = await session.Query.ExecuteAsync(q => q.All<Order>());
      }

      _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
    }

    [Test]
    public void NewTransactionWithOpenedReaderTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          result = session.Query.Execute(q => q.All<Order>());
          _ = Assert.Throws<StorageException>(() => session.OpenTransaction(TransactionOpenMode.New));
        }
      }
    }

    [Test]
    public async Task NewTransactionWithOpenedReaderAsyncTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          result = await session.Query.ExecuteAsync(q => q.All<Order>());
          _ = Assert.Throws<StorageException>(() => session.OpenTransaction(TransactionOpenMode.New));
        }
      }
    }

    [Test]
    public void EnumerationInInnerTransactionTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          result = session.Query.Execute(q => q.All<Order>());
          using (var inner = session.OpenTransaction(TransactionOpenMode.New)) {
            _ = result.ToList();
          }
          
        }
      }
    }

    [Test]
    public async Task EnumerationInInnerTransactionAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          result = await session.Query.ExecuteAsync(q => q.All<Order>());
          using (var inner = session.OpenTransaction(TransactionOpenMode.New)) {
            _ = result.ToList();
          }
        }
      }
    }

    [Test]
    public void EnumerationInInnerVoidTransactionTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          result = session.Query.Execute(q => q.All<Order>());
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            _ = result.ToList();
          }
        }
      }
    }

    [Test]
    public async Task EnumerationInInnerVoidTransactionAsyncTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          result = await session.Query.ExecuteAsync(q => q.All<Order>());
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            _ = result.ToList();
          }
        }
      }
    }

    [Test]
    public void EnumerationInOuterTransactionAfterInnerRollbackTest()
    {
      // sql server does not pull results to memory in client library
      var directReader = StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        // no using intentionally
        var outerTx = session.OpenTransaction();
        var innerTx = session.OpenTransaction(TransactionOpenMode.New);

        result = session.Query.Execute(q => q.All<Order>());

        if (directReader) {
          _ = Assert.Throws<StorageException>(() => innerTx.Dispose());
          _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
          Assert.DoesNotThrow(() => outerTx.Dispose());
        }
        else {
          Assert.DoesNotThrow(() => innerTx.Dispose());
          _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
          Assert.DoesNotThrow(() => outerTx.Dispose());
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterInnerRollbackAsyncTest()
    {
      // sql server does not pull results to memory in client library
      var directReader = StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        // no using intentionally
        var outerTx = session.OpenTransaction();
        var innerTx = session.OpenTransaction(TransactionOpenMode.New);

        result = await session.Query.ExecuteAsync(q => q.All<Order>());

        if (directReader) {
          _ = Assert.Throws<StorageException>(() => innerTx.Dispose());
          _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
          Assert.DoesNotThrow(() => outerTx.Dispose());
        }
        else {
          Assert.DoesNotThrow(() => innerTx.Dispose());
          _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
          Assert.DoesNotThrow(() => outerTx.Dispose());
        }
      }
    }

    [Test]
    public void EnumerationInOuterTransactionAfterVoidInnerRollbackTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            result = session.Query.Execute(q => q.All<Order>());
          }
          _ = result.ToList();
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterVoidInnerRollbackAsyncTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            result = await session.Query.ExecuteAsync(q => q.All<Order>());
          }
          _ = result.ToList();
        }
      }
    }

    [Test]
    public void EnumerationInOuterTransactionAfterInnerCommitTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
            result = session.Query.Execute(q => q.All<Order>());
            innerTx.Complete();
          }
          _ = result.ToList();
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterInnerCommitAsyncTest()
    {
      Require.AllFeaturesSupported(Orm.Providers.ProviderFeatures.Savepoints);

      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
            result = await session.Query.ExecuteAsync(q => q.All<Order>());
            innerTx.Complete();
          }
          _ = result.ToList();
        }
      }
    }

    [Test]
    public void EnumerationInOuterTransactionAfterVoidInnerCommitTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            result = session.Query.Execute(q => q.All<Order>());
            innerTx.Complete();
          }
          _ = result.ToList();
        }
      }
    }

    [Test]
    public async Task EnumerationInOuterTransactionAfterVoidInnerCommitAsyncTest()
    {
      using (var session = Domain.OpenSession()) {
        QueryResult<Order> result;
        using (var outerTx = session.OpenTransaction()) {
          using (var innerTx = session.OpenTransaction(TransactionOpenMode.Auto)) {
            result = await session.Query.ExecuteAsync(q => q.All<Order>());
            innerTx.Complete();
          }
          _ = result.ToList();
        }
      }
    }

    private bool SupportsMars()
    {
      return StorageProviderInfo.Instance.CheckAllFeaturesSupported(Orm.Providers.ProviderFeatures.MultipleActiveResultSets);
    }
  }
}