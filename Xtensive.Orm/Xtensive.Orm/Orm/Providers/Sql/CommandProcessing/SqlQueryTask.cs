// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A query task (i.e. SELECT) for <see cref="CommandProcessor"/>.
  /// </summary>
  public sealed class SqlQueryTask : SqlTask
  {
    /// <summary>
    /// A request.
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
    public override void ProcessWith(CommandProcessor processor)
    {
      processor.ProcessTask(this);
    }

 
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="request">A value for <see cref="Request"/>.</param>
    public SqlQueryTask(QueryRequest request)
    {
      Request = request;
      ParameterContext = null;
      Output = null;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="request">A value for <see cref="Request"/>.</param>
    /// <param name="parameterContext">A value for <see cref="ParameterContext"/>.</param>
    /// <param name="output">A value for <see cref="Output"/>.</param>
    public SqlQueryTask(QueryRequest request, ParameterContext parameterContext, List<Tuple> output)
    {
      Request = request;
      ParameterContext = parameterContext;
      Output = output;
    }
  }
}