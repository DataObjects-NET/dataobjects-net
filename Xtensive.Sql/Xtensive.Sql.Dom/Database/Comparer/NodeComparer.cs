// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Provides delegates allowing to call compare methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="INodeComparer{T}"/> generic argument.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class NodeComparer<T> : MethodCacheBase<INodeComparer<T>>
  {
    private static readonly object syncRoot = new object();
    private static NodeComparer<T> @default;

    /// <summary>
    /// Gets default comparer for type <typeparamref name="T"/>
    /// (uses <see cref="NodeComparerProvider.Default"/> <see cref="NodeComparerProvider"/>).
    /// </summary>
    internal static NodeComparer<T> Default
    {
      [DebuggerStepThrough]
      get
      {
        if (@default==null)
          lock (syncRoot)
            if (@default==null) {
              try {
                var nodeComparer = NodeComparerProvider.Default.GetNodeComparer<T>();
                Thread.MemoryBarrier();
                @default = nodeComparer;
              }
              catch (Exception e) {
                Log.Info(e, String.Format(Resources.Strings.LogUnableToGetDefaultNodeComparerForTypeXxx, typeof(T).FullName));
              }
            }
        return @default;
      }
    }

    /// <summary>
    /// Gets the provider underlying comparer is associated with.
    /// </summary>
    public readonly INodeComparerProvider Provider;

    /// <summary>
    /// Gets <see cref="INodeComparer{T}.Compare"/> method delegate.
    /// </summary>
    public readonly Func<T, T, IComparisonResult<T>> Compare;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Comparer to provide the delegates for.</param>
    public NodeComparer(INodeComparer<T> implementation)
      : base(implementation)
    {
      Provider = Implementation.Provider;
      Compare = Implementation.Compare;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    public NodeComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      Compare = Implementation.Compare;
    }
  }
}