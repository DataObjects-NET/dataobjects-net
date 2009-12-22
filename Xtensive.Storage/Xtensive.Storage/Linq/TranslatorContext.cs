// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Rewriters;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq
{
  internal sealed class TranslatorContext
  {
    private readonly AliasGenerator resultAliasGenerator;
    private readonly AliasGenerator columnAliasGenerator;
    private readonly Dictionary<ParameterExpression, Parameter<Tuple>> tupleParameters;
    private readonly Dictionary<CompilableProvider, ApplyParameter> applyParameters;
    private readonly Dictionary<ParameterExpression, ItemProjectorExpression> boundItemProjectors;

    public IEnumerable<IQueryPreProcessor> QueryPreProcessors { get; private set; }

    public Expression Query { get; private set; }

    public DomainModel Model { get; private set; }

    public IMemberCompilerProvider<Expression> CustomCompilerProvider { get; private set; }

    public Translator Translator { get; private set; }

    public ExpressionEvaluator Evaluator { get; private set; }

    public ParameterExtractor ParameterExtractor { get; private set; }

    public LinqBindingCollection Bindings { get; private set; }

    public bool IsRoot(Expression expression)
    {
      return Query == expression;
    }

    public string GetNextAlias()
    {
      return resultAliasGenerator.Next();
    }

    public string GetNextColumnAlias()
    {
      return columnAliasGenerator.Next();
    }

    public ApplyParameter GetApplyParameter(ProjectionExpression projection)
    {
      return GetApplyParameter(projection.ItemProjector.DataSource);
    }

    internal ApplyParameter GetApplyParameter(RecordSet newRecordSet)
    {
      var provider = newRecordSet.Provider;
      ApplyParameter parameter;
      if (!applyParameters.TryGetValue(provider, out parameter)) {
        parameter = new ApplyParameter(provider.GetType().GetShortName());
        // parameter = new ApplyParameter(provider.ToString()); 
        // ENABLE ONLY FOR DEBUGGING! 
        // May lead TO entity.ToString() calls, while ToString can be overriden.
        applyParameters.Add(provider, parameter);
      }
      return parameter;
    }

    public Parameter<Tuple> GetTupleParameter(ParameterExpression expression)
    {
      Parameter<Tuple> parameter;
      if (!tupleParameters.TryGetValue(expression, out parameter)) {
        parameter = new Parameter<Tuple>(expression.ToString());
        tupleParameters.Add(expression, parameter);
      }
      return parameter;
    }

    public ItemProjectorExpression GetBoundItemProjector(ParameterExpression parameter, ItemProjectorExpression itemProjector)
    {
      ItemProjectorExpression result;
      if (!boundItemProjectors.TryGetValue(parameter, out result)) {
        result = itemProjector.BindOuterParameter(parameter);
        boundItemProjectors.Add(parameter, result);
      }
      return result;
    }


    // Constructors

    public TranslatorContext(Expression query, Domain domain)
    {
      // Custom preprocessors
      QueryPreProcessors = domain.Services.GetAllInstances<IQueryPreProcessor>();
      foreach (var queryPreProcessor in QueryPreProcessors)
        query = queryPreProcessor.Apply(query);

      // built-in preprocessors
      query = ClosureAccessRewriter.Rewrite(query);
      query = EqualityRewriter.Rewrite(query);
      query = EntitySetAccessRewriter.Rewrite(query);
      query = PersistentIndexerRewriter.Rewrite(query);
      Query = query;

      resultAliasGenerator = AliasGenerator.Create("#{0}{1}");
      columnAliasGenerator = AliasGenerator.Create(new[] {"column"});
      CustomCompilerProvider = domain.Handler.GetMemberCompilerProvider<Expression>();
      Model = domain.Model;
      Translator = new Translator(this);
      Evaluator = new ExpressionEvaluator(this.Query);
      ParameterExtractor = new ParameterExtractor(Evaluator);
      Bindings = new LinqBindingCollection();
      applyParameters = new Dictionary<CompilableProvider, ApplyParameter>();
      tupleParameters = new Dictionary<ParameterExpression, Parameter<Tuple>>();
      boundItemProjectors = new Dictionary<ParameterExpression, ItemProjectorExpression>();
    }
  }
}
