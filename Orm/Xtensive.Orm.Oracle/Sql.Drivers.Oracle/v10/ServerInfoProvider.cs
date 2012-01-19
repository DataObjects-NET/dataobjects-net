// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Oracle.v10
{
  internal class ServerInfoProvider : v09.ServerInfoProvider
  {
    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = base.GetDataTypesInfo();
      types.Float = DataTypeInfo.Range(SqlType.Float, types.Float.Features,
        ValueRange.Float, "binary_float", "float");
      types.Double = DataTypeInfo.Range(SqlType.Double, types.Double.Features,
        ValueRange.Double, "binary_double", "double precision");
      return types;
    }


    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}