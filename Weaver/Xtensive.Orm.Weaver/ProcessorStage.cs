// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

namespace Xtensive.Orm.Weaver
{
  public abstract class ProcessorStage
  {
    public virtual bool CanExecute(ProcessorContext context)
    {
      return !context.SkipProcessing;
    }

    public abstract ActionResult Execute(ProcessorContext context);
  }
}