// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer.v10
{
  internal class Driver : SqlServer.Driver
  {
    private const string GeometryTypeName =
      "Microsoft.SqlServer.Types.SqlGeometry, " +
      "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

    private const string GeographyTypeName =
      "Microsoft.SqlServer.Types.SqlGeography, " +
      "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

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
      var mapper = builder.Mapper as TypeMapper;
      if (mapper==null)
        return;

      var geography = Type.GetType(GeographyTypeName);
      if (geography!=null)
        builder.Add(geography, mapper.ReadGeography, mapper.BindGeography, mapper.MapGeography);

      var geometry = Type.GetType(GeometryTypeName);
      if (geometry!=null)
        builder.Add(geometry, mapper.ReadGeometry, mapper.BindGeometry, mapper.MapGeometry);
    }


    // Constructors

    public Driver(CoreServerInfo coreServerInfo, ErrorMessageParser errorMessageParser)
      : base(coreServerInfo, errorMessageParser)
    {
    }
  }
}