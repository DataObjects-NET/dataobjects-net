namespace Xtensive.Orm.Tracing
{
  public class TraceInfo
  {
    public string CallerMemberName { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    public TraceInfo(string callerMemberName, string callerFilePath, int callerLineNumber)
    {
      CallerMemberName = callerMemberName;
      CallerFilePath = callerFilePath;
      CallerLineNumber = callerLineNumber;
    }
  }
}