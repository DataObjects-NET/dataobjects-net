// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2018.05.31

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class RegisterNetCoreAssembliesStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      foreach (var assembly in NetCoreAssemblyList.Get()) {
        var items = assembly.Split();
        context.AssemblyChecker.RegisterNetCoreAssembly(items[0], items[1]);
      }
      return ActionResult.Success;
    }
  }
}