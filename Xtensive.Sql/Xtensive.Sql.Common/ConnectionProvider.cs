// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.IO;
using Xtensive.Core;
using Xtensive.PluginManager;
using Xtensive.Sql.Common.Resources;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// The <see cref="ConnectionProvider"/> is responsible for identification
  /// of suitable RDBMS driver by the specified <see cref="ConnectionInfo">conection URL</see>
  /// and for creating valid <see cref="Connection"/> instance via selected driver.
  /// </summary>
  public class ConnectionProvider
  {
    private readonly Type type;
    private readonly Dictionary<string, Driver> driverCache = new Dictionary<string, Driver>(2);
    private PluginManager<ProtocolAttribute> pluginManager;
    private string path;
    private static AttributeActivator<ProtocolAttribute> attributeActivator;

    /// <summary>
    /// Gets the plugin manager.
    /// </summary>
    /// <value>The plugin manager.</value>
    protected PluginManager<ProtocolAttribute> PluginManager
    {
      get
      {
        if (pluginManager == null) {
          pluginManager = new PluginManager<ProtocolAttribute>(type, path);
          attributeActivator = new AttributeActivator<ProtocolAttribute>();
        }
        return pluginManager;
      }
    }

    /// <summary>
    /// Creates a <see cref="Connection"/> instance by the 
    /// specified <paramref name="url"/>.
    /// </summary>
    /// <param name="url">The URL containing all necessary <see cref="ConnectionInfo">information</see>
    /// to creare and tuneup a <see cref="Connection"/>.</param>
    /// <returns>Newly created <see cref="Connection"/> instance if
    /// suitable <see cref="Driver"/> found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> value can not be <see langword="null"/>.</exception>
    public Connection CreateConnection(string url)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");
      ConnectionInfo connectionInfo = new ConnectionInfo(url);

      Driver driver;
      if (driverCache.ContainsKey(connectionInfo.Protocol))
        driver = driverCache[connectionInfo.Protocol];
      else {
        Type driverType = PluginManager[new ProtocolAttribute(connectionInfo.Protocol)];

        if (driverType == null)
          return null;
        else {
          driver = (Driver)Activator.CreateInstance(driverType, connectionInfo);
          driverCache[connectionInfo.Protocol] = driver;

          // Trying to load all plugin types in already loaded assembly
          Uri loadedAssemblyUri = new Uri(driverType.Assembly.GetName().CodeBase);

          IDictionary<ProtocolAttribute, ITypeInfo> foundTypes = PluginManager.GetFoundTypes();
          foreach (ITypeInfo typeInfo in foundTypes.Values) {
            if (loadedAssemblyUri == new Uri(typeInfo.AssemblyName.CodeBase)) {
              IAttributeInfo[] attributeInfos = typeInfo.GetAttributes();
              for (int index = 0, count = attributeInfos.Length; index < count; index++) {
                ProtocolAttribute attribute = attributeActivator.CreateInstance(attributeInfos[index]);
                if (attribute != null)
                  driverCache[attribute.Name] = driver;
              }
            }
          }
        }
      }

      return driver.CreateConnection(connectionInfo);
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionProvider"/> class.
    /// </summary>
    public ConnectionProvider()
      : this(AppDomain.CurrentDomain.BaseDirectory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionProvider"/> class.
    /// </summary>
    /// <param name="path">The path to search for drivers.</param>
    /// <remarks>
    ///   <para>This version of constructor creates a <see cref="ConnectionProvider"/>
    /// instance that will lookup for direct non <see langword="abstract"/>
    ///     <see cref="Driver"/> descendants while searching a driver
    /// by the specified connection URL.</para>
    ///   <para>
    /// You must change this default behavior in the <see cref="ConnectionProvider"/>
    /// descendants by using the <see cref="ConnectionProvider(Type, string)"/>
    /// constructor version where you can specify a base <see cref="Type"/>
    /// of your drivers.
    /// </para>
    /// </remarks>
    /// <seealso cref="CreateConnection(string)"/>
    protected ConnectionProvider(string path)
      : this(typeof (Driver), path)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionProvider"/> class.
    /// </summary>
    /// <param name="type">A <see cref="Type"/> that is the immediate
    /// ancestor of the <see cref="Driver"/> to be selected by a
    /// connection URL.</param>
    /// <param name="path">The path to search for drivers.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> value can not be <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The specified <paramref name="type"/> is not
    /// <see cref="Driver"/> descendant.</exception>
    protected ConnectionProvider(Type type, string path)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (!typeof (Driver).IsAssignableFrom(type))
        throw new ArgumentException(Strings.ExInvalidDriverType, "type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(path, "path");
      if (!Directory.Exists(path))
        throw new DirectoryNotFoundException("path");
      this.type = type;
      this.path = path;
    }
  }
}