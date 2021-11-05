// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using Xtensive.Sql.Info;
using SqlServerConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer.Azure
{
  internal class Driver : v12.Driver
  {
    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    protected override bool TryProvideErrorContext(int errorCode, string errorMessage, SqlExceptionInfo info)
    {
      return false;
    }

    // Constructors

    public Driver(CoreServerInfo coreServerInfo, ErrorMessageParser errorMessageParser, bool checkConnectionIsAlive)
      : base(coreServerInfo, errorMessageParser, checkConnectionIsAlive)
    {
    }
  }
}