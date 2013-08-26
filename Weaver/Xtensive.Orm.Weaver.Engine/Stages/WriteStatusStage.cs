// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class WriteStatusStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      if (!context.Configuration.WriteStatusFile)
        return ActionResult.Success;

      var statusFile = FileHelper.GetStatusFile(context.Configuration.OutputFile);

      using (var writer = new StreamWriter(statusFile)) {
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
        foreach (var property in type.Properties.Where(p => p.IsKey)) {
          writer.Write(indent2);
          WriteProperty(writer, property);
          writer.WriteLine();
        }
        foreach (var property in type.Properties.Where(p => !p.IsKey)) {
          writer.Write(indent2);
          WriteProperty(writer, property);
          writer.WriteLine();
        }
      }
      writer.WriteLine();
    }

    private static void WriteProperty(StreamWriter writer, PersistentProperty property)
    {
      if (property.IsKey)
        writer.Write("[Key] ");
      if (property.IsExplicitInterfaceImplementation)
        writer.WriteLine("[Explicit] ");
      var propertyDefinition = property.Definition;
      writer.Write(propertyDefinition.PropertyType.FullName);
      writer.Write(" ");
      writer.Write(propertyDefinition.Name);
      if (property.IsExplicitInterfaceImplementation)
        writer.Write(" ({0})", property.ExplicitlyImplementedInterface.FullName);
    }
  }
}