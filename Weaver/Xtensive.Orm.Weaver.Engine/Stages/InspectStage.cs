// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Collections.Generic;
using Xtensive.Orm.Weaver.Inspections;

namespace Xtensive.Orm.Weaver
{
  internal sealed class InspectStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var failure = false;

      foreach (var inspector in GetInspectors()) {
        var actionResult = inspector.Execute(context);
        if (actionResult==ActionResult.Success)
          continue;
        failure = true;
        if (actionResult==ActionResult.FatalFailure)
          break;
      }

      return failure ? ActionResult.Failure : ActionResult.Success;
    }

    private static IEnumerable<Inspector> GetInspectors()
    {
      return new Inspector[] {
        new ReferenceImporter(),
        new EntityInspector(),
      };
    }
  }
}