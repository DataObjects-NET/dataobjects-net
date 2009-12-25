// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.11

using System;

namespace Xtensive.Messaging.Tests
{
  /// <summary>
  /// Represents responses.
  /// </summary>
  [Serializable]
  public abstract class ResponseMessage : IResponseMessage
  {
    private long queryId;
    private long responseQueryId;

    /// <summary>
    /// Gets or sets unique query identifier.
    /// </summary>
    public long QueryId
    {
      get { return queryId; }
      set { queryId = value; }
    }

    /// <summary>
    /// Gets or sets response query identifier.
    /// </summary>
    public long ResponseQueryId
    {
      get { return responseQueryId; }
      set { responseQueryId = value; }
    }

  }
}