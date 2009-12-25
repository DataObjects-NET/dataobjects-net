// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.10

using System;
using Xtensive.Core;

namespace Xtensive.Messaging.Tests
{
  [Serializable]
  public class ErrorMessage : IResponseMessage, IHasError<InvalidProgramException>
  {
    private long queryId;
    private long responseQueryId;
    private readonly InvalidProgramException error;

    /// <summary>
    /// Gets or sets unique query identifier.
    /// </summary>
    public long QueryId
    {
      get { return queryId; }
      set { queryId = value; }
    }


    public InvalidProgramException Error
    {
      get { return error; }
    }

    Exception IHasError.Error
    {
      get { return error; }
    }

    /// <summary>
    /// Gets or sets response query identifier.
    /// </summary>
    public long ResponseQueryId
    {
      get { return responseQueryId; }
      set { responseQueryId = value; }
    }

    public ErrorMessage(InvalidProgramException exception)
    {
      error = exception;
    }
  }
}