// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Linq;

namespace Xtensive.Orm.Weaver
{
  internal sealed class TransformStage : ProcessorStage
  {
    public override ProcessorResult Execute(ProcessorContext context)
    {
      var failure = false;

      var sortedTasks = context.Tasks
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

      return failure ? ProcessorResult.Failure : ProcessorResult.Success;
    }
  }
}