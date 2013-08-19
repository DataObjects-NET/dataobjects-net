// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using Xtensive.Orm.Weaver.Inspections;

namespace Xtensive.Orm.Weaver
{
  internal sealed class InspectStage : ProcessorStage
  {
    private readonly Inspector[] inspectors;

    public override ActionResult Execute(ProcessorContext context)
    {
      var failure = false;

      foreach (var inspector in inspectors) {
        var actionResult = inspector.Execute(context);
        if (actionResult==ActionResult.Success)
          continue;
        failure = true;
        if (actionResult==ActionResult.FatalFailure)
          break;
      }

      return failure ? ActionResult.Failure : ActionResult.Success;
    }

    public InspectStage()
    {
      inspectors = new Inspector[] {
        new EntityInspector(),
      };
    }
  }
}