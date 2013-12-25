// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ExecuteWeavingTasksStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var failure = false;

      foreach (var task in context.WeavingTasks) {
        if (task.Execute(context)==ActionResult.Failure)
          failure = true;
      }

      return failure ? ActionResult.Failure : ActionResult.Success;
    }
  }
}