// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Rewriters;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Linq
{
  internal sealed class TranslatorContext
  {
    private readonly Expression query;
    private readonly DomainModel model;
    private readonly Translator translator;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly AliasGenerator resultAliasGenerator;
    private readonly AliasGenerator columnAliasGenerator;
    private readonly BindingCollection<ParameterExpression, ResultExpression> bindings;
    private readonly Dictionary<ParameterExpression, Parameter<Tuple>> groupingParameters;
    private readonly Dictionary<CompilableProvider, ApplyParameter> applyParameters;

    public Dictionary<ParameterExpression, Parameter<Tuple>> GroupingParameters
    {
      get { return groupingParameters; }
    }

    public Expression Query
    {
      get { return query; }
    }

    public DomainModel Model
    {
      get { return model; }
    }

    public Translator Translator
    {
      get { return translator; }
    }

    public ExpressionEvaluator Evaluator
    {
      get { return evaluator; }
    }

    public ParameterExtractor ParameterExtractor
    {
      get { return parameterExtractor; }
    }

    public BindingCollection<ParameterExpression, ResultExpression> Bindings
    {
      get { return bindings; }
    }

    public bool IsRoot(Expression expression)
    {
      return query == expression;
    }

    public string GetNextAlias()
    {
      return resultAliasGenerator.Next();
    }

    public string GetNextColumnAlias()
    {
      return columnAliasGenerator.Next();
    }

    public ApplyParameter GetApplyParameter(ResultExpression result)
    {
      var provider = result.RecordSet.Provider;
      ApplyParameter parameter;
      if (!applyParameters.TryGetValue(provider, out parameter)) {
        parameter = new ApplyParameter(provider.ToString());
        applyParameters.Add(provider, parameter);
      }
      return parameter;
    }

    // Constructor

    public TranslatorContext(Expression query, DomainModel model)
    {
      resultAliasGenerator = AliasGenerator.Create("#{0}{1}");
      columnAliasGenerator = AliasGenerator.Create(new[] {"column"});
      this.query = EntitySetAccessRewriter.Rewrite(EqualityRewriter.Rewrite(query));
      this.model = model;
      translator = new Translator(this);
      evaluator = new ExpressionEvaluator(this.query);
      parameterExtractor = new ParameterExtractor(evaluator);
      bindings = new BindingCollection<ParameterExpression, ResultExpression>();
      groupingParameters = new Dictionary<ParameterExpression, Parameter<Tuple>>();
      applyParameters = new Dictionary<CompilableProvider, ApplyParameter>();
    }
  }
}
