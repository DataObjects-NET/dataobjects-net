// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer.v09
{
  internal class Driver : SqlServer.Driver
  {
    protected override SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    protected override SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }

    protected override Sql.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    protected override void RegisterCustomMappings(TypeMappingRegistryBuilder builder)
    {
      builder.Add(typeof (DateTimeOffset),
        builder.Mapper.ReadDateTimeOffset,
        builder.Mapper.BindDateTimeOffset,
        builder.Mapper.MapDateTimeOffset);
    }

    protected override void RegisterCustomReverseMappings(TypeMappingRegistryBuilder builder)
    {
      builder.AddReverse(SqlType.DateTimeOffset, typeof (DateTimeOffset));
    }


    // Constructors

    public Driver(CoreServerInfo coreServerInfo, ErrorMessageParser errorMessageParser)
      : base(coreServerInfo, errorMessageParser)
    {
    }
  }
}