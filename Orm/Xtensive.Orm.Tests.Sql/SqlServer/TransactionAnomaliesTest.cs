// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  public class TransactionAnomaliesTest : Sql.TransactionAnomaliesTestBase
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    // If some Isolation levels are not presented they are either not supported
    // or RDBMS time-outs on attempts to represent anomalies, which implicitly means they are impossible

    #region Via DO driver

    [Test]
    public async Task DriverReadUncommitedTest()
    {
      var dirtyRead = await TryPerformDirtyRead(IsolationLevel.ReadUncommitted);
      Assert.That(dirtyRead, Is.True);
      var fuzzyRead = await TryPerformFuzzyRead(IsolationLevel.ReadUncommitted);
      Assert.That(fuzzyRead, Is.True);
      var phantomRead = await TryPerfromPhantomRead(IsolationLevel.ReadUncommitted);
      Assert.That(phantomRead, Is.True);
    }

    [Test]
    public async Task DriverReadCommitedTest()
    {
      var dirtyRead = await TryPerformDirtyRead(IsolationLevel.ReadCommitted);
      Assert.That(dirtyRead, Is.False);
      var fuzzyRead = await TryPerformFuzzyRead(IsolationLevel.ReadCommitted);
      Assert.That(fuzzyRead, Is.True);
      var phantomRead = await TryPerfromPhantomRead(IsolationLevel.ReadCommitted);
      Assert.That(phantomRead, Is.True);
    }

    [Test]
    public async Task DriverSnapshotTest()
    {
      var dirtyRead = await TryPerformDirtyRead(IsolationLevel.Snapshot);
      Assert.That(dirtyRead, Is.False);
      var fuzzyRead = await TryPerformFuzzyRead(IsolationLevel.Snapshot);
      Assert.That(fuzzyRead, Is.False);
      var phantomRead = await TryPerfromPhantomRead(IsolationLevel.Snapshot);
      Assert.That(phantomRead, Is.False);
    }

    #endregion

    #region Via ADO.NET

    [Test]
    public async Task NativeReadUncommitedTest()
    {
      var dirtyRead = await TryPerformDirtyReadNative(IsolationLevel.ReadUncommitted);
      Assert.That(dirtyRead, Is.True);
      var fuzzyRead = await TryPerformFuzzyReadNative(IsolationLevel.ReadUncommitted);
      Assert.That(fuzzyRead, Is.True);
      var phantomRead = await TryPerfromPhantomReadNative(IsolationLevel.ReadUncommitted);
      Assert.That(phantomRead, Is.True);
    }

    [Test]
    [Explicit("May lock depending on test database settings")]
    public async Task NativeReadCommitedTest()
    {
      var dirtyRead = await TryPerformDirtyReadNative(IsolationLevel.ReadCommitted);
      Assert.That(dirtyRead, Is.False);
      var fuzzyRead = await TryPerformFuzzyReadNative(IsolationLevel.ReadCommitted);
      Assert.That(fuzzyRead, Is.True);
      var phantomRead = await TryPerfromPhantomReadNative(IsolationLevel.ReadCommitted);
      Assert.That(phantomRead, Is.True);
    }

    [Test]
    public async Task NativeSnapshotTest()
    {
      var dirtyRead = await TryPerformDirtyReadNative(IsolationLevel.Snapshot);
      Assert.That(dirtyRead, Is.False);
      var fuzzyRead = await TryPerformFuzzyReadNative(IsolationLevel.Snapshot);
      Assert.That(fuzzyRead, Is.False);
      var phantomRead = await TryPerfromPhantomReadNative(IsolationLevel.Snapshot);
      Assert.That(phantomRead, Is.False);
    }

    #endregion
  }
}
