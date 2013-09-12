﻿// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class SaveAssemblyStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var configuration = context.Configuration;

      var inputFile = context.InputFile;
      var outputFile = context.OutputFile;
      var outputIsInput = inputFile==outputFile;

      if (!context.HasTransformations) {
        if (!outputIsInput)
          FileHelper.CopyWithPdb(context, inputFile, outputFile);
        return ActionResult.Success;
      }

      if (context.Configuration.MakeBackup && outputIsInput)
        FileHelper.CopyWithPdb(context, inputFile, FileHelper.GetBackupFile(inputFile));

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

      if (configuration.ProcessDebugSymbols) {
        writerParameters.WriteSymbols = true;
        writerParameters.SymbolWriterProvider = new PdbWriterProvider();
      }

      context.TargetModule.Write(outputFile, writerParameters);

      return ActionResult.Success;
    }

    private StrongNameKeyPair LoadStrongNameKey(string fileName)
    {
      using (var file = File.OpenRead(fileName))
        return new StrongNameKeyPair(file);
    }
  }
}