// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using System;
using System.Data;
using System.Data.Common;

namespace Xtensive.Sql.Firebird
{
  internal class Connection : SqlConnection
  {
    public override DbConnection UnderlyingConnection
    {
      get { throw new NotImplementedException(); }
    }

    public override DbTransaction ActiveTransaction
    {
      get { throw new NotImplementedException(); }
    }

    public override DbParameter CreateParameter()
    {
      throw new NotImplementedException();
    }

    public override void BeginTransaction()
    {
      throw new NotImplementedException();
    }

    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      throw new NotImplementedException();
    }

    protected override void ClearActiveTransaction()
    {
      throw new NotImplementedException();
    }


    // Constructors

    public Connection(SqlDriver driver, string connectionString)
      : base(driver, connectionString)
    {
      // Create underlying connection here
    }
  }
}