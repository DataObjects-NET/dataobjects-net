// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.MemberCompilation;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
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

    public CompilerConfiguration RseCompilerConfiguration { get; }

    public ProviderInfo ProviderInfo { get; }

    public Expression Query { get; }

    public Domain Domain { get; }

    public DomainModel Model { get; }

    public TypeIdRegistry TypeIdRegistry { get; }

    public IMemberCompilerProvider<Expression> CustomCompilerProvider { get; }

    public Translator Translator { get; }

    public ExpressionEvaluator Evaluator { get; }

    public ParameterExtractor ParameterExtractor { get; }

    public LinqBindingCollection Bindings { get; }

    public IReadOnlyList<string> SessionTags { get; private set; }

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
        var providerType = provider.GetType();
        parameter = new ApplyParameter(providerType.IsGenericType ? providerType.GetShortName() : providerType.Name);
        // parameter = new ApplyParameter(provider.ToString()); 
        // ENABLE ONLY FOR DEBUGGING! 
        // May lead TO entity.ToString() calls, while ToString can be overriden.
        applyParameters.Add(provider, parameter);
      }
      return parameter;
    }

    public IReadOnlyList<string> GetMainQueryTags() =>
      Domain.TagsEnabled
        ? applyParameters.Keys.OfType<TagProvider>().Select(p => p.Tag).ToList()
        : Array.Empty<string>();

    public IDisposable DisableSessionTags()
    {
      var originalTags = SessionTags;
      SessionTags = null;
      return new Disposable((b) => SessionTags = originalTags);
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

    private Expression ApplyPreprocessor(IQueryPreprocessor preprocessor, Session session, Expression query)
    {
      var preprocessor2 = preprocessor as IQueryPreprocessor2;
      return preprocessor2!=null
        ? preprocessor2.Apply(session, query)
        : preprocessor.Apply(query);
    }

    // Constructors

    public TranslatorContext(Session session, CompilerConfiguration rseCompilerConfiguration, Expression query,
      CompiledQueryProcessingScope compiledQueryScope)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      ArgumentValidator.EnsureArgumentNotNull(rseCompilerConfiguration, nameof(rseCompilerConfiguration));
      ArgumentValidator.EnsureArgumentNotNull(query, nameof(query));

      Domain = session.Domain;
      RseCompilerConfiguration = rseCompilerConfiguration;
      SessionTags = (Domain.TagsEnabled) ? session.Tags : null;

      // Applying query preprocessors
      query = Domain.Handler.QueryPreprocessors
        .Aggregate(query, (current, preprocessor) => ApplyPreprocessor(preprocessor, session, current));

      // Built-in preprocessors
      query = AggregateOptimizer.Rewrite(query);
      query = ClosureAccessRewriter.Rewrite(query, compiledQueryScope);
      query = EqualityRewriter.Rewrite(query);
      query = EntitySetAccessRewriter.Rewrite(query);
      query = SubqueryDefaultResultRewriter.Rewrite(query);
      Evaluator = new ExpressionEvaluator(query);
      query = PersistentIndexerRewriter.Rewrite(query, this);
      Query = query;

      resultAliasGenerator = AliasGenerator.Create("#{0}{1}");
      columnAliasGenerator = AliasGenerator.Create(new[] {"c01umn"});
      CustomCompilerProvider = Domain.Handler.GetMemberCompilerProvider<Expression>();
      Model = Domain.Model;
      TypeIdRegistry = session.StorageNode.TypeIdRegistry;
      ProviderInfo = Domain.Handlers.ProviderInfo;
      Translator = new Translator(this, compiledQueryScope);
      ParameterExtractor = new ParameterExtractor(Evaluator);
      Bindings = new LinqBindingCollection();
      applyParameters = new Dictionary<CompilableProvider, ApplyParameter>();
      tupleParameters = new Dictionary<ParameterExpression, Parameter<Tuple>>();
      boundItemProjectors = new Dictionary<ParameterExpression, ItemProjectorExpression>();
    }
  }
}
