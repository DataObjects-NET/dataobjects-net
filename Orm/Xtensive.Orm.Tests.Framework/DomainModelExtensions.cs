// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests
{
  public static class DomainModelExtensions
  {
    public static void Dump(this DomainModel target)
    {
      Console.WriteLine("----------------------------------------");
      Console.WriteLine("Model dump");
      Console.WriteLine("----------------------------------------");
      Console.WriteLine("Structures:");
      foreach (TypeInfo type in target.Types.Structures) {
        type.DumpName(1);
        type.Dump(2);
      }
      Console.WriteLine("Hierarchies:");
      foreach (HierarchyInfo hierarchy in target.Hierarchies) {
        hierarchy.Root.DumpName(1);
        hierarchy.Dump(2);
      }
      Console.WriteLine("Entities:");
      foreach (TypeInfo type in target.Types.Entities) {
        type.DumpName(1);
        type.Dump(2);
      }
      Console.WriteLine("Interfaces:");
      foreach (TypeInfo type in target.Types.Interfaces) {
        type.DumpName(1);
        type.Dump(2);
      }
      Console.WriteLine("Associations:");
      foreach (AssociationInfo association in target.Associations) {
        association.DumpName(1);
        association.Dump(2);
      }
      Console.WriteLine("Indexes:");
      foreach (IndexInfo index in target.Types.Where(type => type.IsEntity).SelectMany(type => type.Indexes)) {
        index.DumpName(1);
        index.Dump(2);
      }
      foreach (IndexInfo index in target.Types.Where(type => type.IsInterface).SelectMany(type => type.Indexes)) {
        index.DumpName(1);
        index.Dump(2);
      }
    }

    public static void Dump(this TypeInfoCollection target)
    {
      Console.WriteLine("Structures:");
      foreach (TypeInfo type in target.Structures) {
        type.DumpName(1);
        type.DumpAncestor(2);
        type.DumpDescendants(2);
      }
      Console.WriteLine("Entities:");
      foreach (TypeInfo type in target.Entities) {
        type.DumpName(1);
        type.DumpAncestor(2);
        type.DumpDescendants(2);
        type.DumpInterfaces(2);
      }
      Console.WriteLine("Interfaces:");
      foreach (TypeInfo type in target.Interfaces) {
        type.DumpName(1);
        type.DumpInterfaces(2);
        type.DumpDescendants(2);
        type.DumpImplementors(2);
      }
    }

    public static void DumpAncestor(this TypeInfo target, int indent)
    {
      TypeInfo ancestor = target.Ancestor;
      if (ancestor!=null)
        WriteLine(indent + 1, "Ancestor: " + ancestor.Name);
      else {
        WriteLine(indent + 1, "Ancestor: None");
      }
    }

    public static void DumpDescendants(this TypeInfo target, int indent)
    {
      WriteLine(indent, "Descendants:");
      HashSet<TypeInfo> direct = new HashSet<TypeInfo>(target.Descendants);
      foreach (TypeInfo descendant in target.RecursiveDescendants) {
        if (direct.Contains(descendant))
          WriteLine(indent + 1, descendant.Name + " (direct)");
        else
          WriteLine(indent + 1, descendant.Name);
      }
    }

    public static void DumpInterfaces(this TypeInfo target, int indent)
    {
      WriteLine(indent, "Interfaces:");
      var direct = target.Interfaces;
      foreach (TypeInfo @interface in target.RecursiveInterfaces) {
        if (direct.Contains(@interface))
          WriteLine(indent + 1, @interface.Name + " (direct)");
        else
          WriteLine(indent + 1, @interface.Name);
      }
    }

    public static void DumpImplementors(this TypeInfo target, int indent)
    {
      WriteLine(indent, "Implementors:");
      HashSet<TypeInfo> direct = new HashSet<TypeInfo>(target.Implementors);
      foreach (TypeInfo implementor in target.RecursiveImplementors) {
        if (direct.Contains(implementor))
          WriteLine(indent + 1, implementor.Name + " (direct)");
        else
          WriteLine(indent + 1, implementor.Name);
      }
    }

    public static void DumpName(this Node target, int indent)
    {
      WriteLine(indent, target.Name);
    }

    public static void DumpMappingName(this MappedNode target, int indent)
    {
      if (target.MappingName.IsNullOrEmpty())
        return;
      WriteLine(indent, "MappingName: " + target.MappingName);
    }

    public static void Dump(this HierarchyInfo target, int indent)
    {
      WriteLine(indent, "InheritanceSchema: " + target.InheritanceSchema);
      WriteLine(indent, "KeyFields:");
      foreach (var pair in target.Root.Fields.Where(f => f.IsPrimaryKey)) {
        WriteLine(indent + 1, pair.Name + "(" + pair.ValueType + ") ");
      }
    }

    public static void Dump(this AssociationInfo target, int indent)
    {
      WriteLine(indent, "Referencing type: " + target.OwnerType.Name);
      WriteLine(indent, "Referencing field: " + target.OwnerField.Name);
      WriteLine(indent, "Referenced type: " + target.TargetType.Name);
      WriteLine(indent, "Multiplicity: " + target.Multiplicity);
      WriteLine(indent, "On Delete: " + target.OnTargetRemove);
      WriteLine(indent, "Master: " + target.IsMaster);
      if (target.Reversed!=null)
        WriteLine(indent, "Reversed: " + target.Reversed.Name);
    }

    public static void Dump(this TypeInfo target, int indent)
    {
      if (target.IsEntity) {
        WriteLine(indent, "Hierarchy: " + target.Hierarchy.Root.Name);
        if (target.Hierarchy.Root!=target)
          WriteLine(indent, "Ancestor: " + target.Ancestor.Name);
      }
      else if (target.IsInterface) {
        WriteLine(indent, "Implementors: " + target.Implementors.Select(t => t.Name).ToCommaDelimitedString());
      }

      target.DumpMappingName(indent);
      WriteLine(indent, "UnderlyingType: " + target.UnderlyingType.FullName);
      WriteLine(indent, "Attributes: " + target.Attributes);
      WriteLine(indent, "Fields:");
      foreach (FieldInfo field in target.Fields) {
        field.DumpName(indent + 1);
        field.Dump(indent + 2);
      }
      WriteLine(indent, "Columns:");
      foreach (ColumnInfo column in target.Columns) {
        column.DumpName(indent + 1);
        column.Dump(indent + 2);
      }
      if (target.IsEntity && target.FieldMap.Count > 0) {
        WriteLine(indent + 1, "FieldMap:");
        foreach (KeyValuePair<FieldInfo, FieldInfo> pair in target.FieldMap)
          WriteLine(indent + 2, pair.Key.ReflectedType.Name + "." + pair.Key.Name + " => " + pair.Value.Name);
      }
      if (target.IsEntity || target.IsInterface) {
        if (target.Indexes.Count > 0) {
          WriteLine(indent, "Indexes:");
          foreach (var index in target.Indexes)
            index.DumpName(indent + 1);
        }
      }
    }

    public static void Dump(this IndexInfo target, int indent)
    {
      WriteLine(indent, "ShortName: " + target.ShortName);
      WriteLine(indent, "Attributes: " + target.Attributes);
      WriteLine(indent, "DeclaringType: " + target.DeclaringType.Name);
      WriteLine(indent, "ReflectedType: " + target.ReflectedType.Name);
      if (target.IsVirtual) {
        WriteLine(indent, "BaseIndexes:");
        foreach (IndexInfo baseIndex in target.UnderlyingIndexes) {
          baseIndex.DumpName(indent + 1);
          WriteLine(indent + 2, "Attributes: " + baseIndex.Attributes);
        }
      }
      WriteLine(indent, "Column group:");
      WriteLine(indent + 1, "Columns: " + target.Group.Columns.Select(i => target.Columns[i]).ToCommaDelimitedString());

      WriteLine(indent, "KeyColumns:");
      foreach (KeyValuePair<ColumnInfo, Direction> pair in target.KeyColumns) {
        WriteLine(indent + 1, pair.Key.Name);
        WriteLine(indent + 2, "Direction: " + pair.Value);
        pair.Key.Dump(indent + 2);
      }
      WriteLine(indent, "IncludedColumns:");
      foreach (ColumnInfo column in target.IncludedColumns) {
        column.DumpName(indent + 1);
        column.Dump(indent + 2);
      }
      WriteLine(indent, "ValueColumns:");
      foreach (ColumnInfo column in target.ValueColumns) {
        column.DumpName(indent + 1);
        column.Dump(indent + 2);
      }
    }

    public static void Dump(this FieldInfo target, int indent)
    {
      WriteLine(indent, "OriginalName: " + target.OriginalName);
      WriteLine(indent, "ValueType: " + target.ValueType.Name);
      WriteLine(indent, "Attributes: " + target.Attributes);
      if (target.DeclaringType!=target.ReflectedType)
        WriteLine(indent, "DeclaringType: " + target.DeclaringType.Name);
      target.DumpMappingName(indent);
      if (target.UnderlyingProperty!=null)
        WriteLine(indent, "UnderlyingProperty: " + target.UnderlyingProperty.Name);
      if (target.Length.HasValue)
        WriteLine(indent, "Length: " + target.Length);
      if (target.Column!=null)
        WriteLine(indent, "Column: " + target.Column.Name);
      WriteLine(indent, "MappingInfo: " + target.MappingInfo);
    }

    public static void Dump(this ColumnInfo target, int indent)
    {
      WriteLine(indent, "ValueType: " + target.ValueType.Name);
      WriteLine(indent, "Attributes: " + target.Attributes);
      if (target.Length.HasValue)
        WriteLine(indent, "Length: " + target.Length);
    }

    private static void WriteLine(int indent, string value)
    {
      Console.WriteLine(GetIndent(indent) + value);
    }

    private static string GetIndent(int indent)
    {
      return new string(' ', indent * 2);
    }
  }
}