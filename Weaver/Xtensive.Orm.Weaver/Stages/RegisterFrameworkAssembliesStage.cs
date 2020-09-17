// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
}