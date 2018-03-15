// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.23

using Xtensive.Core;
using Xtensive.Orm.Upgrade.Internals.Interfaces;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal static class ExtractedModelBuilderFactory
  {
    public static ISchemaExtractionResultBuilder GetBuilder(UpgradeContext context)
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      if (context.ParentDomain==null) {
        var makeShared = context.Configuration.ShareStorageSchemaOverNodes;
        return new DomainExtractedModelBuilder(context.Services, context.TargetStorageModel, makeShared);
      }
      var schemaIsShared = context.Configuration.ShareStorageSchemaOverNodes;
      var defaultStorageNode = context.ParentDomain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
      return new NodeExtractedModelBuilder(context.Services, defaultStorageNode, context.NodeConfiguration, schemaIsShared);
    }
  }
}