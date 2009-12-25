// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.15

using System;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using Xtensive.PluginManager.Resources;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Represents an object capable for <see cref="Attribute"/> activation (construction).
  /// </summary>
  [ReflectionPermission(SecurityAction.Demand)]
  public class AttributeActivator<T> where T: Attribute
  {
    /// <summary>
    /// Creates the instance of type T according to its <see cref="IAttributeInfo"/>.
    /// </summary>
    /// <param name="info"><see cref="IAttributeInfo"/>.</param>
    /// <returns>Newly created instance of type T.</returns>
    public T CreateInstance(IAttributeInfo info)
    {
      if (info == null)
        throw new ArgumentNullException("info");
      if (!typeof (T).IsAssignableFrom(info.Constructor.DeclaringType))
        throw new ArgumentException(
          string.Format(CultureInfo.CurrentCulture, Strings.Type1IsNotAssignableFromType2, typeof (T),
                        info.Constructor.DeclaringType));

      object[] parameters = new object[info.ConstructorArguments.Count];
      int i = 0;
      foreach (CustomAttributeTypedArgument arg in info.ConstructorArguments)
        parameters[i] = arg.Value;
      T instance = (T)info.Constructor.Invoke(parameters);

      if (info.NamedArguments.Count > 0)
        SetNamedParameters(instance, info);

      return instance;
    }

    private static void SetNamedParameters(T attribute, IAttributeInfo info)
    {
      foreach (CustomAttributeNamedArgument argument in info.NamedArguments) {
        PropertyInfo pi = argument.MemberInfo as PropertyInfo;
        if (pi != null) {
          pi.SetValue(attribute, argument.TypedValue.Value, null);
          continue;
        }
        FieldInfo fi = argument.MemberInfo as FieldInfo;
        if (fi != null) {
          fi.SetValue(attribute, argument.TypedValue.Value);
          continue;
        }
        MethodBase mb = argument.MemberInfo as MethodBase;
        if (mb != null) {
          mb.Invoke(attribute, new object[] {argument.TypedValue.Value});
          continue;
        }
      }
    }
  }
}