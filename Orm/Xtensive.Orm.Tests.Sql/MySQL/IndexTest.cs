// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Orm.Tests.Sql.MySQL
{
  public sealed class IndexTest : Sql.IndexTest
  {
    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.MySql);
  }
}
