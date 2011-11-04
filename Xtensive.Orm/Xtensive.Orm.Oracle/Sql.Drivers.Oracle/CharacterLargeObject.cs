// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data.Common;

namespace Xtensive.Sql.Oracle
{
  internal sealed class CharacterLargeObject : ICharacterLargeObject
  {
    private readonly OracleConnection connection;
    private OracleClob lob;
    
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

    public void Write(char[] buffer, int offset, int count)
    {
      EnsureLobIsNotNull();
      lob.Write(buffer, offset, count);
    }

    public void BindTo(DbParameter parameter)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.Value = lob ?? OracleClob.Null;
      nativeParameter.OracleDbType = OracleDbType.NClob;
    }

    #region Private / internal methods

    private void EnsureLobIsNotNull()
    {
      if (lob!=null)
        return;
      lob = new OracleClob(connection, false, true);
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

    public CharacterLargeObject(OracleConnection connection)
    {
      this.connection = connection;
    }
  }
}