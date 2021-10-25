// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.14

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class StageModelFilter : IModelFilter
  {
    private readonly IReadOnlyDictionary<Assembly, IUpgradeHandler> handlers;
    private readonly UpgradeStage stage;

    public bool IsFieldAvailable(PropertyInfo field) =>
      handlers.TryGetValue(field.DeclaringType.Assembly, out var handler) && handler.IsFieldAvailable(field, stage);

    public bool IsTypeAvailable(Type type) =>
      handlers.TryGetValue(type.Assembly, out var handler)
        && DomainTypeRegistry.IsPersistentType(type)
        && handler.IsTypeAvailable(type, stage);

    // Constructors

    public StageModelFilter(IReadOnlyDictionary<Assembly, IUpgradeHandler> handlers, UpgradeStage stage)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");

      this.handlers = handlers;
      this.stage = stage;
    }
  }
}