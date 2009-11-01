// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.18

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Descibes the object that is capable for finding and loading plugins by their base <see cref="Type"/>
  /// and <see cref="Attribute"/>s.
  /// </summary>
  /// <typeparam name="T">The type of attribute.</typeparam>
  [FileIOPermission(SecurityAction.Demand)]
  public class PluginManager<T>: IPluginManager<T>
    where T: Attribute
  {
    private readonly Type pluginType;
    private readonly string path;
    private readonly PluginRegistry<T> pluginRegistry;
    private PluginManagerState state;
    private readonly AttributeActivator<T> activator;

    /// <summary>
    /// Finds the plugins of specified type and marked with specified attribute.
    /// </summary>
    private void Find()
    {
      string[] files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
      Type attributeType = typeof (T);
      using (AssemblyInspector ai = new AssemblyInspector()) {
        ai.TypeFound += TypeFound;
        for (int i = 0, count = files.Length; i < count; i++) {
          ai.FindTypes(files[i], pluginType, attributeType);
        }
        ai.TypeFound -= TypeFound;
      }
      state = PluginManagerState.Ready;
    }

    private void TypeFound(object sender, TypeFoundEventArgs e)
    {
      ITypeInfo typeInfo = e.TypeInfo;
      T[] attributes = new T[typeInfo.GetAttributes().Length];
      int i = 0;
      foreach (IAttributeInfo attributeInfo in typeInfo.GetAttributes()) {
        attributes[i] = activator.CreateInstance(attributeInfo);
        i++;
      }
      pluginRegistry.Register(attributes, typeInfo);
    }

    #region IPluginManager<T> Members

    /// <summary>
    /// Gets the base plugin type.
    /// </summary>
    /// <value>The base plugin type.</value>
    public Type PluginType
    {
      get { return pluginType; }
    }

    /// <summary>
    /// Gets the type of the plugin attribute.
    /// </summary>
    /// <value>The type of the attribute.</value>
    public Type AttributeType
    {
      get { return typeof (T); }
    }

    /// <summary>
    /// Gets the path to search for plugins.
    /// </summary>
    /// <value>The path.</value>
    public string Path
    {
      get { return path; }
    }

    /// <summary>
    /// Gets a <see cref="IDictionary{TKey,TValue}"/> of all types found by this instance.
    /// </summary>
    /// <remarks>The thread that requests all types will be frozen untill the plugin discovery process will be completed.</remarks>
    /// <returns>An <see cref="IDictionary{TKey,TValue}"/> with all types found by <see cref="PluginManager{T}"/>.</returns>
    public IDictionary<T, ITypeInfo> GetFoundTypes()
    {
      switch (state) {
        case PluginManagerState.Initial:
          lock (this) {
            Thread workThread = new Thread(Find);
            workThread.IsBackground = true;
            workThread.Start();
            state = PluginManagerState.Searching;
          }
          return GetFoundTypes();
        case PluginManagerState.Searching:
          while (state != PluginManagerState.Ready) {
            Thread.Sleep(100);
          }
          return pluginRegistry.GetFoundTypes();
      }
      return pluginRegistry.GetFoundTypes();
    }


    /// <summary>
    /// Determines whether plugin with specified attribute exists.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns><see langword="true"/> if a plugin <see cref="Type"/> with specified attribute exists, otherwise <see langword="false"/>.</returns>
    public bool Exists(T attribute)
    {
      return pluginRegistry.Exists(attribute);
    }

    /// <summary>
    /// Gets the plugin <see cref="Type"/> by specified attribute.
    /// </summary>
    /// <param name="attribute">The plugin attribute.</param>
    /// <returns>The plugin <see cref="Type"/> if exists, otherwise <see langword="null"/>.</returns>
    public Type this[T attribute]
    {
      get
      {
        if (Exists(attribute))
          return pluginRegistry[attribute];

        switch (state) {
          case PluginManagerState.Initial:
            lock (this) {
              Thread workThread = new Thread(Find);
              workThread.IsBackground = true;
              workThread.Start();
              state = PluginManagerState.Searching;
            }
            return this[attribute];
          case PluginManagerState.Searching:
            while (state != PluginManagerState.Ready) {
              if (Exists(attribute))
                return pluginRegistry[attribute];
              else
                Thread.Sleep(100);
            }
            return this[attribute];
        }

        return null;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="pluginType">Plugin base type.</param>
    /// <param name="path">The path to search for plugins.</param>
    public PluginManager(Type pluginType, string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("path");
      if (!Directory.Exists(path))
        throw new DirectoryNotFoundException(path);
      if (pluginType == null)
        throw new ArgumentNullException("pluginType");
      this.pluginType = pluginType;
      this.path = path;
      pluginRegistry = new PluginRegistry<T>();
      state = PluginManagerState.Initial;
      activator = new AttributeActivator<T>();
    }

    #endregion
  }
}