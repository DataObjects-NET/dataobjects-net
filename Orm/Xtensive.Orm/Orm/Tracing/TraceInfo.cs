namespace Xtensive.Orm.Tracing
{
  /// <summary>
  /// Query tracing information gathered when Trace() method is called on a LINQ query.
  /// </summary>
  public class TraceInfo
  {
    /// <summary>
    /// Gets the member name of a class where Trace() method was called.
    /// </summary>
    public string CallerMemberName { get; }

    /// <summary>
    /// Gets the path to the file where Trace() method was called.
    /// </summary>
    public string CallerFilePath { get; }

    /// <summary>
    /// Gets the line number in the file where Trace() method was called.
    /// </summary>
    public int CallerLineNumber { get; }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="callerMemberName">Member name.</param>
    /// <param name="callerFilePath">File path.</param>
    /// <param name="callerLineNumber">Line number.</param>
    public TraceInfo(string callerMemberName, string callerFilePath, int callerLineNumber)
    {
      CallerMemberName = callerMemberName;
      CallerFilePath = callerFilePath;
      CallerLineNumber = callerLineNumber;
    }
  }
}