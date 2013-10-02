// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.12.08

using System;
using System.Diagnostics;
using System.Reflection;


namespace Xtensive.Reflection
{
  /// <summary>
  /// Resources-related helper.
  /// </summary>
  public static class ResourceHelper
  {
    /// <summary>
    /// Gets the string resource by specified resource type and resource name.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="resourceName">The property name on the resource type.</param>
    /// <returns>String resource value.</returns>
    public static string GetStringResource(Type resourceType, string resourceName)
    {
      PropertyInfo property = resourceType.GetProperty(resourceName, 
        BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic);

      if (property == null)
        throw new InvalidOperationException(
          string.Format(Strings.ExTypeXDoesNotHavePropertyY, resourceType.GetShortName(), resourceName));
      
      if (property.PropertyType != typeof(string))
        throw new InvalidOperationException(
          string.Format(Strings.ExResourcePropertyXIsNotOfStringType, resourceName));
      
      return (string)property.GetValue(null, null);
      
    }
  }
}