// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using System.Collections.Generic;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A query task (i.e. SELECT) for <see cref="CommandProcessor"/>.
  /// </summary>
  public sealed class SqlLoadTask : SqlTask
  {
    /// <summary>
    /// A request to execute.
    /// </summary>
    public readonly QueryRequest Request;

    /// <summary>
    /// A parameter context to activate during parameters binding.
    /// </summary>
    public readonly ParameterContext ParameterContext;

    /// <summary>
    /// A list of tuples to store result in.
    /// </summary>
    public readonly List<Tuple> Output;

    /// <inheritdoc/>
    public override bool ProcessWith(ISqlTaskProcessor processor)
    {
      return processor.ProcessTask(this);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="request">A value for <see cref="Request"/>.</param>
    /// <param name="output">A value for <see cref="Output"/>.</param>
    /// <param name="parameterContext">A value for <see cref="ParameterContext"/>.</param>
    public SqlLoadTask(QueryRequest request, List<Tuple> output, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(request, "request");
      ArgumentValidator.EnsureArgumentNotNull(output, "output");

      Request = request;
      ParameterContext = parameterContext;
      Output = output;
    }
  }
}