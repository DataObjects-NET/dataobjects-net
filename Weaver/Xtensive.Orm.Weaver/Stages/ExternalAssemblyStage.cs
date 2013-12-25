// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.25

using System;
using System.Linq;
using System.Reflection;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ExternalAssemblyStage : ProcessorStage
  {
    private readonly string assemblyName;

    public override ActionResult Execute(ProcessorContext context)
    {
      var assembly = Assembly.Load(assemblyName);
      var stages = assembly.GetCustomAttributes(typeof (ProcessorStageAttribute), false)
        .Cast<ProcessorStageAttribute>()
        .Where(a => a.StageType!=null)
        .OrderBy(a => a.Priority)
        .Select(a => Activator.CreateInstance(a.StageType))
        .Cast<ProcessorStage>()
        .ToList();

      foreach (var stage in stages)
        if (stage.CanExecute(context) && stage.Execute(context)==ActionResult.Failure)
          return ActionResult.Failure;

      return ActionResult.Success;
    }

    public ExternalAssemblyStage(string assemblyName)
    {
      this.assemblyName = assemblyName;
    }
  }
}