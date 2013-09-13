// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.IO;
using System.Linq;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class WriteStatusStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      if (!context.Configuration.WriteStatusFile)
        return ActionResult.Success;

      var statusFile = FileHelper.GetStatusFile(context.OutputFile);

      using (var writer = new StreamWriter(statusFile))
        DumpTypes(context, writer);

      return ActionResult.Success;
    }

    private static void DumpTypes(ProcessorContext context, StreamWriter writer)
    {
      const string indent = "  ";

      foreach (var type in context.PersistentTypes) {
        WriteWithWrapping(writer, string.Empty, indent, type.ToString());
        foreach (var property in type.Properties.Values)
          WriteWithWrapping(writer, indent, indent, property.ToString());
      }
    }

    private static void WriteWithWrapping(StreamWriter writer, string baseIndent, string indent, string line)
    {
      var items = line.Split();
      if (items.Length==0)
        return;
      writer.Write(baseIndent);
      writer.WriteLine(items[0]);
      for (var i = 1; i < items.Length; i++) {
        writer.Write(baseIndent);
        writer.Write(indent);
        writer.Write(items[i]);
        writer.WriteLine();
      }
      writer.WriteLine();
    }
  }
}