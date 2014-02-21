// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.27

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class WriteStampStage : ProcessorStage
  {
    public override bool CanExecute(ProcessorContext context)
    {
      return context.Configuration.WriteStampFile;
    }

    public override ActionResult Execute(ProcessorContext context)
    {
      var stampFile = FileHelper.GetStampFile(context.OutputFile);
      FileHelper.Touch(stampFile);
      return ActionResult.Success;
    }
  }
}