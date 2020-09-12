// Copyright (C) 2018-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
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