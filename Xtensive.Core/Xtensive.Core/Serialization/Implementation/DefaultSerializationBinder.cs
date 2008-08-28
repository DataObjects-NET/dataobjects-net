// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
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
    private static ThreadSafeDictionary<Pair<string, string>, Type> cachedTypes =
      ThreadSafeDictionary<Pair<string, string>, Type>.Create(new object());

    /// <see cref="SingletonDocTemplate.Instance" copy="true" />
    public static DefaultSerializationBinder Instance {
      [DebuggerStepThrough]
      get { return instance; }
    }

    /// <inheritdoc/>
    public override Type BindToType(string assemblyName, string typeName) 
    {
      var t = cachedTypes.GetValue(
        new Pair<string, string>(assemblyName, typeName), 
        p => {
          Assembly assembly = null;
          try { assembly = Assembly.Load(p.First); } catch { }
          if (assembly==null)
            try { assembly = Assembly.LoadWithPartialName(p.First); } catch { }
          if (assembly==null)
            return null;
          Type type = null;
          try { type = FormatterServices.GetTypeFromAssembly(assembly, typeName); } catch { }
          return type;
        });
      return t;
    }


    // Constructors
    
    private DefaultSerializationBinder()
    {
    }
  }
}