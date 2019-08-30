// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.06.23

namespace Xtensive.Orm.Weaver
{
  /// <summary>
  /// Property checker for CSharp language.
  /// </summary>
  internal sealed class CsPropertyChecker : IPersistentPropertyChecker
  {
    ///<inheritdoc/>
    public bool HasSkippedProperties { get; private set; }

    ///<inheritdoc/>
    public bool ShouldProcess(PropertyInfo property, ProcessorContext context)
    {
      if (property.IsAutomatic)
        return true;
      var def = property.Definition;
      var getter = def.GetMethod;
      var setter = def.SetMethod;

      var baseMethod = getter ?? setter;

      // if the property has a human made body we shouldn't touch them
      if (!baseMethod.HasAttribute(WellKnown.CompilerGeneratedAttribute))
        return false;

      // if it's compiler-generated property though
      // we should check it contains both getter and setter
      // otherwise the whole idea is corrupted
      if (getter==null || setter==null) {
        context.Logger.Write(MessageCode.WarningPersistentPropertyHasNoSetterOrGetter, string.Format("'{0}' property of '{1}' type", property.Name, property.DeclaringType.FullName));
        HasSkippedProperties = true;
        return false;
      }
      return true;
    }
  }
}