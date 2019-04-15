// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    20.08.17

using System;

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
