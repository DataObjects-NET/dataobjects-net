// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.23

using Xtensive.Core;
using Xtensive.Orm.Upgrade.Internals.Interfaces;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal static class ExtractedModelBuilderFactory
  {
    public static ISchemaExtractionResultBuilder GetBuilder(UpgradeServiceAccessor services, StorageModel model)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      ArgumentValidator.EnsureArgumentNotNull(services, "services");

      return new DomainExtractedModelBuilder(services, model);
    }

    public static ISchemaExtractionResultBuilder GetBuilder(UpgradeServiceAccessor services, StorageNode defaultNode)
    {
      ArgumentValidator.EnsureArgumentNotNull(services, "services");
      ArgumentValidator.EnsureArgumentNotNull(defaultNode, "defaultNode");

      return new NodeExtractedModelBuilder(services, defaultNode);
    }
  }
}