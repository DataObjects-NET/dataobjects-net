// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using Xtensive.Orm.Weaver.Tasks;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class MarkAssemblyStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      if (context.HasTransformations)
        context.WeavingTasks.Add(new AddAttributeTask(
          context.TargetModule.Assembly, context.References.ProcessedByWeaverAttributeConstructor));
      return ActionResult.Success;
    }
  }
}