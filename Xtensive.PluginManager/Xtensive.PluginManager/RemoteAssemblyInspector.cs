// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.12

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Represents an object that is capable for assembly inspection.
  /// </summary>
  [ReflectionPermission(SecurityAction.Demand)]
  [FileIOPermission(SecurityAction.Demand)]
  public class RemoteAssemblyInspector: MarshalByRefObject, IAssemblyInspector
  {
    #region Events

    /// <summary>
    /// Occurs when assembly inspection process starts.
    /// </summary>
    public event EventHandler<AssemblyInspectorEventArgs> InspectionStart;

    /// <summary>
    /// Occurs when assembly inspection process completed.
    /// </summary>
    public event EventHandler<AssemblyInspectorEventArgs> InspectionComplete;

    /// <summary>
    /// Occurs when requested type is found in assembly that is being inspected.
    /// </summary>
    public event EventHandler<TypeFoundEventArgs> TypeFound;

    private void OnInspectionStart(string file)
    {
      if (InspectionStart != null)
        InspectionStart(this, new AssemblyInspectorEventArgs(file));
    }

    private void OnInspectionComplete(string file)
    {
      if (InspectionComplete != null)
        InspectionComplete(this, new AssemblyInspectorEventArgs(file));
    }

    private void OnTypeFound(ITypeInfo typeInfo)
    {
      if (TypeFound != null)
        TypeFound(this, new TypeFoundEventArgs(typeInfo));
    }

    #endregion

    /// <summary>
    /// Searches for types accepted by the given filter and filter criteria. 
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="filterCriteria">The filter criteria.</param>
    public void FindTypes(string file, TypeFilter filter, object filterCriteria)
    {
      OnInspectionStart(file);
      Assembly assembly = LoadFrom(file);
      if (assembly != null) {
        Type[] types = GetExportedTypes(assembly);
        for (int i = 0, count = types.Length; i < count; i++) {
          if (filter != null && filter(types[i], filterCriteria))
            OnTypeFound(new TypeInfo(types[i]));
        }
      }
      OnInspectionComplete(file);
    }

    /// <summary>
    /// Searches for types inherited from baseType.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="baseType">The base type.</param>
    public void FindTypes(string file, Type baseType)
    {
      FindTypes(file, baseType, null);
    }

    /// <summary>
    /// Searches for types inherited from baseType and optionally marked with attributes of attributeType.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="baseType">The base type.</param>
    /// <param name="attributeType">The attribute type.</param>
    public void FindTypes(string file, Type baseType, Type attributeType)
    {
      if (baseType==null)
        throw new ArgumentNullException("baseType");
      OnInspectionStart(file);
      Assembly assembly = LoadFrom(file);
      if (assembly != null) {
        Type remoteBaseType = Type.ReflectionOnlyGetType(baseType.AssemblyQualifiedName, false, false);
        bool isInterface = remoteBaseType.IsInterface;
        Type[] types = GetExportedTypes(assembly);
        for (int i = 0, count = types.Length; i < count; i++) {
          if (isInterface && remoteBaseType.IsAssignableFrom(types[i]) ||
              (!isInterface && types[i].IsSubclassOf(remoteBaseType))) {
            if (attributeType == null) {
              OnTypeFound(new TypeInfo(types[i]));
              continue;
            }
            Type remoteAttributeType = Type.ReflectionOnlyGetType(attributeType.AssemblyQualifiedName, false, false);
            IAttributeInfo[] attributes = FindAttributes(types[i], remoteAttributeType);
            if (attributes.Length > 0)
              OnTypeFound(new TypeInfo(types[i], attributes));
          }
        }
      }
      OnInspectionComplete(file);
    }

    private static AttributeInfo[] FindAttributes(Type type, Type attributeType)
    {
      AttributeInfo[] attributes = new AttributeInfo[0];

      IList<CustomAttributeData> allAttributes = CustomAttributeData.GetCustomAttributes(type);
      if (allAttributes == null || allAttributes.Count == 0)
        return attributes;

      Collection<AttributeInfo> foundAttributes = new Collection<AttributeInfo>();
      foreach (CustomAttributeData attribute in allAttributes) {
        if (attribute.Constructor.DeclaringType == attributeType)
          foundAttributes.Add(new AttributeInfo(attribute));
      }
      if (foundAttributes.Count > 0) {
        attributes = new AttributeInfo[foundAttributes.Count];
        foundAttributes.CopyTo(attributes, 0);
      }
      return attributes;
    }

    private static Assembly LoadFrom(string file)
    {
      if (string.IsNullOrEmpty(file))
        throw new ArgumentNullException("file");
      if (!File.Exists(file))
        throw new FileNotFoundException(file);
      try {
        return Assembly.ReflectionOnlyLoadFrom(file);
      }
      catch (IOException) {
      }
      catch (BadImageFormatException) {
      }
      return null;
    }

    private static Type[] GetExportedTypes(Assembly assembly)
    {
      try {
        return assembly.GetExportedTypes();
      }
      catch (FileNotFoundException) {
        return new Type[0];
      }
    }

    private static Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
    {
      return Assembly.ReflectionOnlyLoad(args.Name);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteAssemblyInspector"/> class.
    /// </summary>
    public RemoteAssemblyInspector()
    {
      AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve +=
        new ResolveEventHandler(ReflectionOnlyAssemblyResolve);
    }
  }
}