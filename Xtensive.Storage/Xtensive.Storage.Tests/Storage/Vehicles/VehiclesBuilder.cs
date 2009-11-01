// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.09

using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  public class VehiclesBuilder : IDomainBuilder
  {
    public void Build(BuildingContext context, DomainDef domain)
    {
      TypeDef companyType;
      if (domain.Types.TryGetValue(typeof (Company), out companyType))
        companyType.DefineField("WebSite", typeof (string));
      domain.Types["Country"].Fields["Flag"].ValueType = typeof (byte[]);
      domain.Types["Company"].Fields["RegNumber"].ValueType = typeof (string);
      domain.Types["Truck"].Fields["Power"].ValueType = typeof (string);
      domain.Types["Division"].Fields["Number"].ValueType = typeof (string);
      domain.Types["Logo"].Fields["Image"].ValueType = typeof (byte[]);
    }
  }
}