// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.10

using System;
using Xtensive.Core;

namespace Xtensive.Messaging
{
  [Serializable]
  internal class ErrorResponseMessage : IResponseMessage, IHasError<Exception>
  {
    private readonly Exception error;

    /// <summary>
    /// Gets or sets unique query identifier.
    /// </summary>
    public long QueryId { get; set; }

    public Exception Error
    {
      get { return error; }
    }

    #region IHasError Members

    Exception IHasError.Error
    {
      get { return error; }
    }

    #region IResponseMessage Members

    /// <summary>
    /// Gets or sets response query identifier.
    /// </summary>
    public long ResponseQueryId { get; set; }

    #endregion

    #endregion

    public ErrorResponseMessage(Exception exception)
    {
      error = exception;
    }
  }
}