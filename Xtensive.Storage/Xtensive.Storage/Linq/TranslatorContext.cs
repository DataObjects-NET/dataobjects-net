// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Linq
{
  internal sealed class TranslatorContext
  {
    private readonly Expression query;
    private readonly DomainModel model;
    private readonly Translator translator;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly Dictionary<ParameterExpression, ResultExpression> parameterBindings;
    private readonly AliasGenerator resultAliasGenerator;
    private readonly AliasGenerator columnAliasGenerator;

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

    // Extract base: Core.Linq.BindingContext<T>
    
    public ParameterBinding Bind(ParameterExpression pe, ResultExpression re)
    {
      Action disposeAction;
      ResultExpression value;
      if (parameterBindings.TryGetValue(pe, out value)) {
        parameterBindings[pe] = re;
        disposeAction = () => parameterBindings[pe] = value;
      }
      else {
        parameterBindings.Add(pe, re);
        disposeAction = () => parameterBindings.Remove(pe);
      }
      return new ParameterBinding(disposeAction);
    }

    public void ReplaceBound(ParameterExpression pe, ResultExpression re)
    {
      if (!parameterBindings.ContainsKey(pe))
        throw new InvalidOperationException();
      parameterBindings[pe] = re;
    }

    public ResultExpression GetBound(ParameterExpression pe)
    {
      return parameterBindings[pe];
    }


    // Constructor

    public TranslatorContext(Expression query)
    {
      resultAliasGenerator = AliasGenerator.Create();
      columnAliasGenerator = AliasGenerator.Create(new[] {"column"});
      this.query = ExpressionPreprocessor.Preprocess(query);
      var domain = Domain.Current;
      if (domain == null)
        throw new InvalidOperationException(Strings.ExNoCurrentSession);
      model = domain.Model;
      parameterBindings = new Dictionary<ParameterExpression, ResultExpression>();
      translator = new Translator(this);
      evaluator = new ExpressionEvaluator(this.query);
      parameterExtractor = new ParameterExtractor(evaluator);
    }
  }
}