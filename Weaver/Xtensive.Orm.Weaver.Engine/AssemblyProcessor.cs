// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Weaver
{
  public sealed class AssemblyProcessor : IAssemblyProcessor
  {
    private readonly ProcessorStage[] stages;

    public ActionResult Execute(ProcessorParameterSet parameters, IMessageWriter messageWriter)
    {
      if (parameters==null)
        throw new ArgumentNullException("parameters");
      if (messageWriter==null)
        throw new ArgumentNullException("messageWriter");

      var context = new ProcessorContext {
        Parameters = parameters,
        Logger = new MessageLogger(parameters.ProjectId, messageWriter),
        WeavingTasks = new List<WeavingTask>()
      };

      foreach (var stage in stages) {
        var stageResult = ExecuteStage(context, stage);
        if (stageResult!=ActionResult.Success)
          return stageResult;
      }

      return ActionResult.Success;
    }

    private static ActionResult ExecuteStage(ProcessorContext context, ProcessorStage stage)
    {
      return stage.Execute(context);
    }

    public AssemblyProcessor()
    {
      stages = new ProcessorStage[] {
        new ValidateStage(),
        new LoadStage(),
        new InspectStage(),
        new TransformStage(),
        new SaveStage(),
      };
    }
  }
}
