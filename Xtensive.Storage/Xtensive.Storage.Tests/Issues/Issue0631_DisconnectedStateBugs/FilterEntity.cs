// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;

namespace Xtensive.Storage.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  /// <summary>
  /// </summary>
  public class FilterEntity : SampleEntity
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterEntity"/> class.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    public FilterEntity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Gets or sets NullableGuid.
    /// </summary>
    [Field]
    public Guid? NullableGuid { get; set; }

    /// <summary>
    /// Gets or sets Sample.
    /// </summary>
    [Field]
    public SampleEntity Sample { get; set; }
  }
}