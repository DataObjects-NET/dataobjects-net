// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.19

using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  public class VehicleSingleTableBuilder : IDomainBuilder
  {
    public void Build(BuildingContext context, DomainDef domain)
    {
      domain.Hierarchies[typeof (Fleet)].Schema = InheritanceSchema.SingleTableInheritance;
    }
  }
}