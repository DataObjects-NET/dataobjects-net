using System;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Serializable]
  public class ErrorDocument : Document
  {
    [Field]
    public string LinkedMessage { get; set; }
    
    [Field]
    public Document LinkedDocument { get; set; }

    [Field]
    public Processor ExecutableHavingCausedTheError { get; set; }

    [Field]
    public Container OriginalContainer { get; set; }

    [Field]
    public DateTime? RetryAfter { get; set; }

    public DateTime? GetDateOfFirstSimilarError()
    {
      return null;
    }
  }
}
