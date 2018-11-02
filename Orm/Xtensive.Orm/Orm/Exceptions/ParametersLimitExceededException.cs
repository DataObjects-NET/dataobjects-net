using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Xtensive.Orm
{
  public sealed class ParametersLimitExceededException : StorageException
  {
    public int CurrentParameterCount { get; private set; }

    public int MaxParameterCount { get; private set; }

    public ParametersLimitExceededException(int currentParameterCount, int maxParameterCount)
      : base(string.Format(Strings.ExQueryHasTooManyParametersMaxCountOfParametersIsX, maxParameterCount), null)
    {
      CurrentParameterCount = currentParameterCount;
      MaxParameterCount = maxParameterCount;
    }
  }
}
