// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class LoadStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      context.TargetModule = ModuleDefinition.ReadModule(context.Parameters.InputFile);
      return ActionResult.Success;
    }
  }
}