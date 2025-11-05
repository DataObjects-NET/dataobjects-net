// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Firebird.v3_0
{
  internal class ServerInfoProvider : v2_5.ServerInfoProvider
  {
    // It seems that Firebird 2.5 uses max identifier length constraint only for identifiers of database objects but ignores length of column aliases
    // In Firebird 3.0 this behavior changed and the same length limit is applied for column aliases as well,
    // so we decrease limit by length of "#a." string, similar prefixes are common within queries
    private const int Fb30MaxIdentifierLength = 27;

    protected override int MaxIdentifierLength => Fb30MaxIdentifierLength;


    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
