// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.IO;

namespace Xtensive.Orm.Weaver
{
  internal sealed class ValidateStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var inputFile = context.Parameters.InputFile;

      if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile)) {
        context.Logger.Write(MessageCode.ErrorInputFileNotFound, inputFile);
        return ActionResult.Failure;
      }

      return ActionResult.Success;
    }
  }
}