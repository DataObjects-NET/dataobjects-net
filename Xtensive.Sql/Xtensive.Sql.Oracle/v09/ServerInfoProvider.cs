// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Oracle.DataAccess.Client;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Oracle.v09
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private VersionInfo version;

    public override EntityInfo GetCollationInfo()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetCharacterSetInfo()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetTranslationInfo()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetTriggerInfo()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      throw new NotImplementedException();
    }

    public override SequenceInfo GetSequenceInfo()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetDatabaseInfo()
    {
      throw new NotImplementedException();
    }

    public override ColumnInfo GetColumnInfo()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetViewInfo()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetSchemaInfo()
    {
      throw new NotImplementedException();
    }

    public override TableInfo GetTableInfo()
    {
      throw new NotImplementedException();
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      throw new NotImplementedException();
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      throw new NotImplementedException();
    }

    public override ConstraintInfo GetPrimaryKeyInfo()
    {
      throw new NotImplementedException();
    }

    public override ConstraintInfo GetUniqueConstraintInfo()
    {
      throw new NotImplementedException();
    }

    public override IndexInfo GetIndexInfo()
    {
      throw new NotImplementedException();
    }

    public override ReferenceConstraintInfo GetReferentialConstraintInfo()
    {
      throw new NotImplementedException();
    }

    public override QueryInfo GetQueryInfo()
    {
      throw new NotImplementedException();
    }

    public override IdentityInfo GetIdentityInfo()
    {
      throw new NotImplementedException();
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      throw new NotImplementedException();
    }

    public override VersionInfo GetVersionInfo()
    {
      return version;
    }

    public override IsolationLevels GetIsolationLevels()
    {
      throw new NotImplementedException();
    }

    public override EntityInfo GetDomainInfo()
    {
      throw new NotImplementedException();
    }

    public override ConstraintInfo GetAssertionInfo()
    {
      throw new NotImplementedException();
    }

    public override int GetStringIndexingBase()
    {
      throw new NotImplementedException();
    }

    // Constructors

    public ServerInfoProvider(OracleConnection connection, Version version)
    {
      this.version = new VersionInfo(version);
    }
  }
}