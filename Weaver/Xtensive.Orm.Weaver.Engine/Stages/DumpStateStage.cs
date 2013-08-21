// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class DumpStateStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var dumpFile = context.Configuration.DumpFile;
      if (string.IsNullOrEmpty(dumpFile))
        return ActionResult.Success;

      using (var writer = new StreamWriter(dumpFile)) {
        DumpTypes(writer, "Entity types:", context.EntityTypes);
        DumpTypes(writer, "Structure types:", context.StructureTypes);
      }

      return ActionResult.Success;
    }

    private static void DumpTypes(StreamWriter writer, string header, IEnumerable<PersistentType> types)
    {
      const string indent1 = "  ";
      const string indent2 = indent1 + indent1;

      writer.WriteLine(header);
      writer.WriteLine();
      foreach (var type in types) {
        writer.WriteLine(indent1 + type.Definition.FullName);
        foreach (var field in type.Properties)
          writer.WriteLine(indent2 + field.PropertyType + " " + field.Name);
      }
      writer.WriteLine();
    }
  }
}