// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xtensive.Orm.Weaver.Stages;

namespace Xtensive.Orm.Weaver
{
  public sealed class AssemblyProcessor
  {
    private readonly ProcessorConfiguration configuration;
    private readonly IMessageWriter messageWriter;

    public ActionResult Execute()
    {
      var logger = new MessageLogger(configuration.ProjectId, messageWriter);
      var referencedAssemblies = configuration.ReferencedAssemblies ?? Enumerable.Empty<string>();
      var assemblyResolver = new AssemblyResolver(referencedAssemblies, logger);

      var context = new ProcessorContext {
        Configuration = configuration,
        ApplicationDirectory = Path.GetDirectoryName(GetType().Assembly.Location),
        Logger = logger,
        AssemblyResolver = assemblyResolver,
        MetadataResolver = new MetadataResolver(assemblyResolver),
      };

      context.InputFile = FileHelper.ExpandPath(configuration.InputFile);
      context.OutputFile = FileHelper.ExpandPath(configuration.OutputFile);
      if (string.IsNullOrEmpty(context.OutputFile))
        context.OutputFile = context.InputFile;

      using (context) {
        foreach (var stage in GetStages()) {
          var stageResult = ExecuteStage(context, stage);
          if (stageResult!=ActionResult.Success)
            return stageResult;
        }
      }

      return ActionResult.Success;
    }

    private IEnumerable<ProcessorStage> GetStages()
    {
      return new ProcessorStage[] {
        new LoadAssemblyStage(),
        new ImportReferencesStage(),
        new RegisterFrameworkAssembliesStage(),
        new FindPersistentTypesStage(),
        new ValidateLicenseStage(),
        new ModifyPersistentTypesStage(),
        new MarkAssemblyStage(),
        new WriteStatusStage(),
        new ExecuteWeavingTasksStage(),
        new SaveAssemblyStage(),
        new WriteStampStage(),
      };
    }

    private static ActionResult ExecuteStage(ProcessorContext context, ProcessorStage stage)
    {
      try {
        return stage.Execute(context);
      }
      catch (StageFailedException) {
        return ActionResult.Failure;
      }
    }

    public AssemblyProcessor(ProcessorConfiguration configuration, IMessageWriter messageWriter)
    {
      this.configuration = configuration;
      this.messageWriter = messageWriter;
    }
  }
}
