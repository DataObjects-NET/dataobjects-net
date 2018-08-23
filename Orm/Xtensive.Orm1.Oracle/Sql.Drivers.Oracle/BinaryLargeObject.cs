// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data.Common;

namespace Xtensive.Sql.Drivers.Oracle
{
  internal sealed class BinaryLargeObject : IBinaryLargeObject
  {
    private readonly OracleConnection connection;
    private OracleBlob lob;

    public bool IsNull { get { return lob==null; } }
    public bool IsEmpty { get { return lob!=null && lob.IsEmpty; } }

    public void Dispose()
    {
      FreeLob();
    }

    public void Erase()
    {
      EnsureLobIsNotNull();
      lob.Erase();
    }

    public void Nullify()
    {
      FreeLob();
    }

    public void Write(byte[] buffer, int offset, int count)
    {
      EnsureLobIsNotNull();
      lob.Write(buffer, offset, count);
    }

    public void BindTo(DbParameter parameter)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.Value = lob ?? OracleBlob.Null;
      nativeParameter.OracleDbType = OracleDbType.Blob;
    }

    #region Private / internal methods

    private void EnsureLobIsNotNull()
    {
      if (lob!=null)
        return;
      lob = new OracleBlob(connection);
    }

    private void FreeLob()
    {
      if (lob==null)
        return;
      lob.Close();
      lob.Dispose();
      lob = null;
    }

    #endregion

    // Constructors

    public BinaryLargeObject(OracleConnection connection)
    {
      this.connection = connection;
    }
  }
}