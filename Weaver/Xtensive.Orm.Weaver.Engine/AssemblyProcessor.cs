// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Xtensive.Orm.Weaver.Stages;

namespace Xtensive.Orm.Weaver
{
  public sealed class AssemblyProcessor : IAssemblyProcessor
  {
    public ActionResult Execute(ProcessorConfiguration configuration, IMessageWriter messageWriter)
    {
      if (configuration==null)
        throw new ArgumentNullException("configuration");
      if (messageWriter==null)
        throw new ArgumentNullException("messageWriter");

      var logger = new MessageLogger(configuration.ProjectId, messageWriter);
      var referencedAssemblies = configuration.ReferencedAssemblies ?? Enumerable.Empty<string>();
      var assemblyResolver = new AssemblyResolver(referencedAssemblies, logger);

      var context = new ProcessorContext {
        Configuration = configuration,
        Logger = logger,
        AssemblyResolver = assemblyResolver,
        MetadataResolver = new MetadataResolver(assemblyResolver),
      };

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
        new FindPersistentTypesStage(),
        new ModifyPersistentTypesStage(),
        new MarkAssemblyStage(),
        new DumpStateStage(),
        new ExecuteWeavingTasksStage(),
        new SaveAssemblyStage(),
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
  }
}
