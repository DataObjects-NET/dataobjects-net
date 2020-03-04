using System;

namespace Xtensive.Orm.Rse.Providers
{
  [Serializable]
  public sealed class TraceProvider: UnaryProvider
  {
    public TraceData Data { get; }

    public TraceProvider(CompilableProvider source, TraceData data) : base(ProviderType.Trace, source)
    {
      Data = data;
      Initialize();
    }
  }

  public class TraceData
  {
    public string CallerMemberName { get; set; }
    public string CallerFilePath { get; set; }
    public int CallerLineNumber { get; set; }
  }
}