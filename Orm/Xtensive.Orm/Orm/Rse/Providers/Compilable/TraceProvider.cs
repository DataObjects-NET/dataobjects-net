using System;
using Xtensive.Orm.Tracing;

namespace Xtensive.Orm.Rse.Providers
{
  [Serializable]
  public sealed class TraceProvider : UnaryProvider
  {
    public TraceInfo TraceInfo { get; }

    public TraceProvider(CompilableProvider source, TraceInfo traceInfo) : base(ProviderType.Trace, source)
    {
      TraceInfo = traceInfo;
      Initialize();
    }
  }
}