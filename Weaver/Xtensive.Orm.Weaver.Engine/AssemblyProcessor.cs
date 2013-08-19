// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using System.Threading;

namespace Xtensive.Orm.Weaver
{
  public sealed class AssemblyProcessor : IAssemblyProcessor
  {
    private readonly ProcessorStage[] stages;

    public ProcessorResult Execute(ProcessorParameterSet parameters, IMessageWriter messageWriter)
    {
      var context = new ProcessorContext {
        Parameters = parameters,
        MessageWriter = messageWriter,
        Tasks = new List<ProcessorTask>()
      };

      foreach (var stage in stages) {
        var stageResult = stage.Execute(context);
        if (stageResult!=ProcessorResult.Success)
          return stageResult;
      }

      for (var i = 0; i < 5; i++) {
        messageWriter.Write(new ProcessorMessage {
          File = "Foo.cs",
          MessageCode = "XW0001",
          MessageText = "What's the meaning of this?",
          Type = MessageType.Warning,
        });
        Thread.Sleep(TimeSpan.FromSeconds(1));
      }

      return ProcessorResult.Success;
    }

    public AssemblyProcessor()
    {
      stages = new ProcessorStage[] {
        new LoadStage(),
        new InspectStage(),
        new TransformStage(),
        new SaveStage(),
      };
    }
  }
}
