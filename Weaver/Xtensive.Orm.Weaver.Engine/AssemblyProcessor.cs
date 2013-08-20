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
        foreach (var stage in GetStages()) {
          var stageResult = ExecuteStage(context, stage);
          if (stageResult!=ActionResult.Success)
            return stageResult;
        }
      }

      return ActionResult.Success;
    }

    private IEnumerable<ProcessorStage> GetStages()
    {
      return new ProcessorStage[] {
        new LoadStage(),
        new InspectStage(),
        new TransformStage(),
        new SaveStage(),
      };
    }

    private static ActionResult ExecuteStage(ProcessorContext context, ProcessorStage stage)
    {
      return stage.Execute(context);
    }
  }
}
