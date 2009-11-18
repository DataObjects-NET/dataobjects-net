// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data.Common;

namespace Xtensive.Sql.Oracle
{
  internal sealed class BinaryLargeObject : IBinaryLargeObject
  {
    private OracleBlob lob;
    private OracleConnection connection;

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

    public BinaryLargeObject(DbConnection connection)
    {
      this.connection = (OracleConnection) connection;
    }
  }
}