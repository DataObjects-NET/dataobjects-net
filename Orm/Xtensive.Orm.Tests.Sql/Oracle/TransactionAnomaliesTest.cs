// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Oracle
{
  internal class TransactionAnomaliesTest : Sql.TransactionAnomaliesTestBase
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIs(StorageProvider.Oracle);
    }

    // If some Isolation levels are not presented they are either not supported
    // or RDBMS time-outs on attempts to represent anomalies, which implicitly means they are impossible

    #region Via DO driver

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
    public async Task DriverSerializableTest()
    {
      var dirtyRead = await TryPerformDirtyRead(IsolationLevel.Serializable);
      Assert.That(dirtyRead, Is.False);
      var fuzzyRead = await TryPerformFuzzyRead(IsolationLevel.Serializable);
      Assert.That(fuzzyRead, Is.False);
      var phantomRead = await TryPerfromPhantomRead(IsolationLevel.Serializable);
      Assert.That(phantomRead, Is.False);
    }

    #endregion


    #region Via ADO.NET

    [Test]
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
    public async Task NativeSerializableTest()
    {
      var dirtyRead = await TryPerformDirtyReadNative(IsolationLevel.Serializable);
      Assert.That(dirtyRead, Is.False);
      var fuzzyRead = await TryPerformFuzzyReadNative(IsolationLevel.Serializable);
      Assert.That(fuzzyRead, Is.False);
      var phantomRead = await TryPerfromPhantomReadNative(IsolationLevel.Serializable);
      Assert.That(phantomRead, Is.False);
    }

    #endregion
  }
}
