// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.17

using System;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Describes the <see cref="Agent"/>,
  /// e.g. its URL, processor's load, available memory, etc.
  /// </summary>
  [Serializable]
  public class AgentInfo
  {
    #region Private fields

    private readonly string url;
    private readonly float processorLoad;
    private readonly float availableMemory;

    #endregion

    #region Properties

    /// <summary>
    /// Gets load factor of agent. Calculates from <see cref="processorLoad"/> and <see cref="AvailableMemory"/>. 
    /// Greater value matches more busy agent.
    /// </summary>
    public float Load
    {
      get
      {
        if (100 - processorLoad < 0.001 || availableMemory < 50) {
          return float.MaxValue;
        }
        return 1/(100 - processorLoad) + 10/availableMemory;
      }
    }

    /// <summary>
    /// Gets available memory on agent (in megabytes)
    /// </summary>
    public float AvailableMemory
    {
      get { return availableMemory; }
    }

    /// <summary>
    /// Gets agent's url.
    /// </summary>
    public string Url
    {
      get { return url; }
    }

    /// <summary>
    /// Gets agent's average processor load for a last several seconds.
    /// </summary>
    public float ProcessorLoad
    {
      get { return processorLoad; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="AgentInfo"/>.
    /// </summary>
    /// <param name="url">Agent's url.</param>
    /// <param name="processorLoad">Agent's average processor load for last several seconds.</param>
    /// <param name="availableMemory">Agent's available memory in megabytes.</param>
    public AgentInfo(string url, float processorLoad, float availableMemory)
    {
      this.url = url;
      this.availableMemory = availableMemory;
      this.processorLoad = processorLoad;
    }

    #endregion
  }
}