// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.10.25

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;

namespace Xtensive.Core.Aspects.Internals
{
  /// <summary>
  /// Implement private field accessor aspect - provides get and set accessors for private fields of a type.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Field)]
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementPrivateFieldAccessorAspect : LaosTypeLevelAspect
  {
    private static readonly Dictionary<Type, ImplementPrivateFieldAccessorAspect> aspects = new Dictionary<Type, ImplementPrivateFieldAccessorAspect>();
    private Dictionary<string, bool> fields = new Dictionary<string, bool>();

    ///<summary>
    /// Gets collection of field names that
    /// need to have acessors.
    ///</summary>
    public Dictionary<string, bool> Fields
    {
      get { return fields; }
    }

    /// <summary>
    /// Findes an existent or creates a new
    /// instance of this class for certain type
    /// and adds field name to the <see cref="Fields"/>
    /// collection.
    /// </summary>
    /// <param name="field"><see cref="FieldInfo"/> to add.</param>
    /// <returns><see cref="ImplementPrivateFieldAccessorAspect"/> instance.</returns>
    public static ImplementPrivateFieldAccessorAspect FindOrCreate(MemberInfo field)
    {
      Type type = field.DeclaringType;
      lock (aspects) {
        ImplementPrivateFieldAccessorAspect aspect;
        aspects.TryGetValue(type, out aspect);
        if (aspect == null) {
          aspect = new ImplementPrivateFieldAccessorAspect();
          aspects.Add(type, aspect);
        }
        aspect.fields.Add(field.Name, true);
        return aspect;
      }
    }

    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      requirements.PlugIns.Add("Xtensive.Core.Weaver");
      requirements.Tasks.Add("Xtensive.Core.Weaver.WeaverFactory");
      return requirements;
    }


    // Constructors

    internal ImplementPrivateFieldAccessorAspect()
    {
    }
  }
}