// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Serialization.Implementation
{
  /// <summary>
  /// Default <see cref="SerializationBinder"/> implementation.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  public class DefaultSerializationBinder : SerializationBinder
  {
    private static readonly DefaultSerializationBinder instance = 
      new DefaultSerializationBinder();
    private static ThreadSafeDictionary<Pair<string, string>, Assembly> cachedAssemblies =
      ThreadSafeDictionary<Pair<string, string>, Assembly>.Create(new object());

    /// <see cref="SingletonDocTemplate.Instance" copy="true" />
    public static DefaultSerializationBinder Instance {
      [DebuggerStepThrough]
      get { return instance; }
    }

    /// <inheritdoc/>
  #if NET40
    [SecuritySafeCritical]
  #endif
    public override Type BindToType(string assemblyName, string typeName) 
    {
      var a = cachedAssemblies.GetValue(
        new Pair<string, string>(assemblyName, typeName), 
        p => {
          Assembly assembly = null;
          try { assembly = Assembly.Load(p.First); } catch { }
          if (assembly==null)
            try { assembly = Assembly.LoadWithPartialName(p.First); } catch { }
          if (assembly==null)
            return null;
          return assembly;
        });
      Type type = null;
      try { type = FormatterServices.GetTypeFromAssembly(a, typeName); } catch { }
      return type;
    }


    // Constructors
    
    private DefaultSerializationBinder()
    {
    }
  }
}