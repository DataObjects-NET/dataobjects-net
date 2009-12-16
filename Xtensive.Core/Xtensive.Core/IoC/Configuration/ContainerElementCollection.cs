// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System;
using System.Linq;
using Xtensive.Core.Configuration;

namespace Xtensive.Core.IoC.Configuration
{
  /// <summary>
  /// Configuration collection for <see cref="ContainerElement"/> type.
  /// </summary>
  [Serializable]
  public class ContainerElementCollection : ConfigurationCollection<ContainerElement>
  {
    /// <summary>
    /// Gets the default container.
    /// </summary>
    public ContainerElement Default
    {
      get
      {
        var result = this.Where(c => c.Name=="default").SingleOrDefault();
        if (result!=null) {
          return result;
        }
        return this.Where(c => string.IsNullOrEmpty(c.Name)).SingleOrDefault();
      }
    }
  }
}