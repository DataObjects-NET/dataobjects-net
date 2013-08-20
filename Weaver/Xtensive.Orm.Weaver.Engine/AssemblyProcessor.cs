// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;

namespace Xtensive.Orm.Weaver
{
  public sealed class AssemblyProcessor : IAssemblyProcessor
  {
    private readonly ProcessorStage[] stages;

    public ActionResult Execute(ProcessorConfiguration configuration, IMessageWriter messageWriter)
    {
      if (configuration==null)
        throw new ArgumentNullException("configuration");
      if (messageWriter==null)
        throw new ArgumentNullException("messageWriter");

      var context = new ProcessorContext {
        Configuration = configuration,
        Logger = new MessageLogger(configuration.ProjectId, messageWriter),
      };

      using (context) {
        foreach (var stage in stages) {
          var stageResult = ExecuteStage(context, stage);
          if (stageResult!=ActionResult.Success)
            return stageResult;
        }
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
        new LoadStage(),
        new InspectStage(),
        new TransformStage(),
        new SaveStage(),
      };
    }
  }
}
