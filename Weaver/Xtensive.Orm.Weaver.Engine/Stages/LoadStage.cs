// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace Xtensive.Orm.Weaver
{
  internal sealed class LoadStage : ProcessorStage
  {
    private static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

    public override ActionResult Execute(ProcessorContext context)
    {
      return LoadTargetModule(context) && LoadOrmModule(context)
        ? ActionResult.Success
        : ActionResult.Failure;
    }

    private bool LoadTargetModule(ProcessorContext context)
    {
      var configuration = context.Configuration;
      var inputFile = configuration.InputFile;

      if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile)) {
        context.Logger.Write(MessageCode.ErrorInputFileIsNotFound, inputFile);
        return false;
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

      return true;
    }

    private bool LoadOrmModule(ProcessorContext context)
    {
      var configuration = context.Configuration;

      var ormReference = context.TargetModule.AssemblyReferences
        .FirstOrDefault(r => Comparer.Equals(r.FullName, WellKnown.OrmAssemblyFullName));

      if (ormReference==null) {
        context.Logger.Write(MessageCode.ErrorTargetAssemblyHasNoReferenceToOrm);
        return false;
      }

      context.References.OrmAssembly = ormReference;

      var referencedAssemblies = context.Configuration.ReferencedAssemblies ?? new List<string>();

      var ormAssemblyFile = configuration.ReferencedAssemblies
        .FirstOrDefault(r => Comparer.Equals(Path.GetFileName(r), WellKnown.OrmAssemblyFile));

      if (ormAssemblyFile==null) {
        context.Logger.Write(MessageCode.ErrorUnableToLocateOrmAssembly);
        return false;
      }

      var assemblyResolver = new AssemblyResolver(referencedAssemblies);
      var readerParameters = new ReaderParameters {
        ReadingMode = ReadingMode.Deferred,
        AssemblyResolver = assemblyResolver,
        MetadataResolver = new MetadataResolver(assemblyResolver),
      };

      context.OrmModule = ModuleDefinition.ReadModule(ormAssemblyFile, readerParameters);

      return true;
    }
  }
}