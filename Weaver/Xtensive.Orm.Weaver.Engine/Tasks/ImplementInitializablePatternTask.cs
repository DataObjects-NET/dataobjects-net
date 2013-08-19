// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class ImplementInitializablePatternTask : WeavingTask
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      return ActionResult.Success;
    }
  }
}