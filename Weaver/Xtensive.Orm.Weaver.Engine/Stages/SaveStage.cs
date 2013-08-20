// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace Xtensive.Orm.Weaver
{
  internal sealed class SaveStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var configuration = context.Configuration;

      if (context.WeavingTasks.Count==0) {
        File.Copy(configuration.InputFile, configuration.OutputFile);
        if (configuration.UseDebugSymbols)
          File.Copy(FileUtility.GetDebugSymbolsFile(configuration.InputFile), FileUtility.GetDebugSymbolsFile(configuration.OutputFile));
        return ActionResult.Success;
      }

      var writerParameters = new WriterParameters();

      var strongNameKey = configuration.StrongNameKey;
      if (!string.IsNullOrEmpty(strongNameKey)) {
        if (File.Exists(strongNameKey)) {
          writerParameters.StrongNameKeyPair = LoadStrongNameKey(strongNameKey);
        }
        else {
          context.Logger.Write(MessageCode.ErrorStrongNameKeyIsNotFound);
          return ActionResult.Failure;
        }
      }

      if (configuration.UseDebugSymbols) {
        writerParameters.WriteSymbols = true;
        writerParameters.SymbolStream = File.OpenWrite(FileUtility.GetDebugSymbolsFile(configuration.OutputFile));
        writerParameters.SymbolWriterProvider = new PdbWriterProvider();
      }

      context.TargetModule.Write(configuration.OutputFile, writerParameters);

      return ActionResult.Success;
    }

    private StrongNameKeyPair LoadStrongNameKey(string fileName)
    {
      using (var file = File.OpenRead(fileName))
        return new StrongNameKeyPair(file);
    }
  }
}