// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.27

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class RegisterFrameworkAssembliesStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      foreach (var assembly in FrameworkAssemblyList.Get()) {
        var items = assembly.Split();
        context.AssemblyChecker.RegisterFrameworkAssembly(items[0], items[1]);
      }
      return ActionResult.Success;
    }
  }

  internal sealed class RegisterNetStandardAssembliesStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      foreach (var assembly in NetStandardAssemblyList.Get()) {
        var items = assembly.Split();
        context.AssemblyChecker.RegisterFrameworkAssembly(items[0], items[1]);
      }
      return ActionResult.Success;
    }
  }
}