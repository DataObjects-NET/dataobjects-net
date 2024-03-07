// Copyright (C) 2018-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2018.11.02


namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is throws when number of parameters in <see cref="System.Data.Common.DbCommand"/> exeeded maximum value for the storage.
  /// </summary>
  [Serializable]
  public sealed class ParametersLimitExceededException : StorageException
  {
    /// <summary>
    /// Current number of parameters in <see cref="System.Data.Common.DbCommand"/> which caused exception.
    /// </summary>
    public int CurrentParameterCount { get; private set; }

    /// <summary>
    /// Maximum number of parameters declared for <see cref="System.Data.Common.DbCommand"/> which is allowed.
    /// </summary>
    public int MaxParameterCount { get; private set; }

    public ParametersLimitExceededException(int currentParameterCount, int maxParameterCount)
      : base(string.Format(Strings.ExQueryHasTooManyParametersMaxCountOfParametersIsX, maxParameterCount), null)
    {
      CurrentParameterCount = currentParameterCount;
      MaxParameterCount = maxParameterCount;
    }
  }
}