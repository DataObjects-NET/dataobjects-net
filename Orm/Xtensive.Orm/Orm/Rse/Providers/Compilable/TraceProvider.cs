using System;
using Xtensive.Orm.Tracing;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that stores query tracing information.
  /// </summary>
  [Serializable]
  public sealed class TraceProvider : UnaryProvider
  {
    /// <summary>
    /// Tracing information.
    /// </summary>
    public TraceInfo TraceInfo { get; }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="traceInfo">Tracing information.</param>
    public TraceProvider(CompilableProvider source, TraceInfo traceInfo) : base(ProviderType.Trace, source)
    {
      TraceInfo = traceInfo;
      Initialize();
    }
  }
}