// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class LoadAssemblyStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var configuration = context.Configuration;
      var inputFile = configuration.InputFile;

      if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile)) {
        context.Logger.Write(MessageCode.ErrorInputFileIsNotFound, inputFile);
        return ActionResult.Failure;
      }

      var debugSymbolsFile = string.Empty;

      if (configuration.UseDebugSymbols) {
        debugSymbolsFile = FileHelper.GetDebugSymbolsFile(inputFile);
        if (!File.Exists(debugSymbolsFile)) {
          configuration.UseDebugSymbols = false;
          context.Logger.Write(MessageCode.WarningDebugSymbolsFileIsNotFound, debugSymbolsFile);
        }
      }

      var referencedAssemblies = context.Configuration.ReferencedAssemblies ?? new List<string>();
      var assemblyResolver = new AssemblyResolver(referencedAssemblies);
      var readerParameters = new ReaderParameters {
        ReadingMode = ReadingMode.Deferred,
        AssemblyResolver = assemblyResolver,
        MetadataResolver = new MetadataResolver(assemblyResolver),
      };

      Stream debugSymbolsStream = null;

      if (configuration.UseDebugSymbols) {
        debugSymbolsStream = File.OpenRead(debugSymbolsFile);
        readerParameters.ReadSymbols = true;
        readerParameters.SymbolReaderProvider = new PdbReaderProvider();
        readerParameters.SymbolStream = debugSymbolsStream;
      }

      using (debugSymbolsStream) {
        context.TargetModule = ModuleDefinition.ReadModule(inputFile, readerParameters);
      }

      return ActionResult.Success;
    }
  }
}