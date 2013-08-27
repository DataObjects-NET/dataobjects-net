// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.27

using System.IO;
using System.Linq;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class LoadSystemAssembliesListStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var listDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
      var listFile = Path.Combine(listDirectory, "SystemAssemblies.txt");
      if (File.Exists(listFile))
        foreach (var item in File.ReadLines(listFile).Where(l => !string.IsNullOrWhiteSpace(l)))
          context.SystemAssemblies.Add(item);
      return ActionResult.Success;
    }
  }
}