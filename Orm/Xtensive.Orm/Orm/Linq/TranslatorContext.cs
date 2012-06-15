// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Linq;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Parameters;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq
{
  internal sealed class TranslatorContext
  {
    private readonly AliasGenerator resultAliasGenerator;
    private readonly AliasGenerator columnAliasGenerator;
    private readonly Dictionary<ParameterExpression, Parameter<Tuple>> tupleParameters;
    private readonly Dictionary<CompilableProvider, ApplyParameter> applyParameters;
    private readonly Dictionary<ParameterExpression, ItemProjectorExpression> boundItemProjectors;

    public CompilerConfiguration RseCompilerConfiguration { get; private set; }

    public ProviderInfo ProviderInfo { get; private set; }

    public Expression Query { get; private set; }

    public Domain Domain { get; private set; }

    public DomainModel Model { get; private set; }

    public IMemberCompilerProvider<Expression> CustomCompilerProvider { get; private set; }

    public Translator Translator { get; private set; }

    public ExpressionEvaluator Evaluator { get; private set; }

    public ParameterExtractor ParameterExtractor { get; private set; }

    public LinqBindingCollection Bindings { get; private set; }

    public bool IsRoot(Expression expression)
    {
      return Query==expression;
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

    public ApplyParameter GetApplyParameter(CompilableProvider provider)
    {
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

    public void RebindApplyParameter(CompilableProvider old, CompilableProvider @new)
    {
      ApplyParameter parameter;
      if (applyParameters.TryGetValue(old, out parameter)) {
        applyParameters[@new] = parameter;
      }
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

    public TranslatorContext(Domain domain, CompilerConfiguration rseCompilerConfiguration, Expression query)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(rseCompilerConfiguration, "rseCompilerConfiguration");
      ArgumentValidator.EnsureArgumentNotNull(query, "query");

      Domain = domain;
      RseCompilerConfiguration = rseCompilerConfiguration;

      // Applying query preprocessors
      query = domain.Handler.QueryPreprocessors
        .Aggregate(query, (current, preprocessor) => preprocessor.Apply(current));

      // Built-in preprocessors
      query = ClosureAccessRewriter.Rewrite(query);
      query = EqualityRewriter.Rewrite(query);
      query = EntitySetAccessRewriter.Rewrite(query);
      Evaluator = new ExpressionEvaluator(query);
      query = PersistentIndexerRewriter.Rewrite(query, this);
      Query = query;

      resultAliasGenerator = AliasGenerator.Create("#{0}{1}");
      columnAliasGenerator = AliasGenerator.Create(new[] {"c01umn"});
      CustomCompilerProvider = domain.Handler.GetMemberCompilerProvider<Expression>();
      Model = domain.Model;
      ProviderInfo = domain.Handlers.ProviderInfo;
      Translator = new Translator(this);
      ParameterExtractor = new ParameterExtractor(Evaluator);
      Bindings = new LinqBindingCollection();
      applyParameters = new Dictionary<CompilableProvider, ApplyParameter>();
      tupleParameters = new Dictionary<ParameterExpression, Parameter<Tuple>>();
      boundItemProjectors = new Dictionary<ParameterExpression, ItemProjectorExpression>();
    }
  }
}
