// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Linq;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ExecuteWeavingTasksStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var failure = false;

      var sortedTasks = context.WeavingTasks
        .Select((t, i) => new {Task = t, Index = i})
        .OrderBy(item => item.Task.Priority)
        .ThenBy(item => item.Index)
        .Select(item => item.Task);

      foreach (var task in sortedTasks) {
        var actionResult = task.Execute(context);
        if (actionResult==ActionResult.Success)
          continue;
        failure = true;
        if (actionResult==ActionResult.FatalFailure)
          break;
      }

      return failure ? ActionResult.Failure : ActionResult.Success;
    }
  }
}