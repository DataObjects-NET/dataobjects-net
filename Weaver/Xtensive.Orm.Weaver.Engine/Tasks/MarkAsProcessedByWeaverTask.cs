// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class MarkAsProcessedByWeaverTask : WeavingTask
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      context.TargetModule.CustomAttributes.Add(new CustomAttribute(context.References.ProcessedByWeaverAttributeConstructor));
      return ActionResult.Success;
    }
  }
}