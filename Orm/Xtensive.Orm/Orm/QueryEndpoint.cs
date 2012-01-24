using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Resources;
using Xtensive.Parameters;
using Xtensive.Reflection;
using Activator = System.Activator;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// Provides methods allowing to run LINQ queries,
  /// create future (delayed) and compiled queries,
  /// and finally, resolve <see cref="Key"/>s to <see cref="Entity">entities</see>.
  /// </summary>
  public class QueryEndpoint
  {
    protected Session Session { get; private set; }
    public QueryProvider Provider { get; private set; }

    /// <summary>
    /// The "starting point" for any LINQ query -
    /// a <see cref="IQueryable{T}"/> enumerating all the instances
    /// of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the sequence element.</typeparam>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> enumerating all the instances
    /// of type <typeparamref name="T"/>.
    /// </returns>
    public virtual IQueryable<T> All<T>()
      where T : class, IEntity
    {
      var allMethod = WellKnownMembers.Query.All.MakeGenericMethod(typeof(T));
      var expression = Expression.Call(null, allMethod);
      return Provider.CreateQuery<T>(expression);
    }

    /// <summary>
    /// The "starting point" for dynamic LINQ query -
    /// a <see cref="IQueryable"/> enumerating all the instances
    /// of type <paramref name="elementType"/>.
    /// </summary>
    /// <param name="elementType">Type of the sequence element.</param>
    /// <returns>
    /// An <see cref="IQueryable"/> enumerating all the instances
    /// of type <paramref name="elementType"/>.
    /// </returns>
    public virtual IQueryable All(Type elementType)
    {
      var allMethod = WellKnownMembers.Query.All.MakeGenericMethod(elementType);
      var expression = Expression.Call(null, allMethod);
      var provider = (IQueryProvider) Provider;
      return provider.CreateQuery(expression);
    }

    /// <summary>
    /// Performs full-text query for the text specified in free text form.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public virtual IQueryable<FullTextMatch<T>> FreeText<T>(string searchCriteria)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      var method = WellKnownMembers.Query.FreeTextString.MakeGenericMethod(typeof(T));
      var expression = Expression.Call(method, Expression.Constant(searchCriteria));
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the text specified in free text form.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public virtual IQueryable<FullTextMatch<T>> FreeText<T>(Expression<Func<string>> searchCriteria)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      var method = WellKnownMembers.Query.FreeTextExpression.MakeGenericMethod(typeof(T));
      var expression = Expression.Call(null, method, new[] { searchCriteria });
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }


    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Entity with the specified key is not found.</exception>
    public virtual Entity Single(Key key)
    {
      if (key == null)
        return null;
      var result = SingleOrDefault(key);
      if (result == null)
        throw new KeyNotFoundException(String.Format(
          Strings.EntityWithKeyXDoesNotExist, key));
      return result;
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    public virtual Entity SingleOrDefault(Key key)
    {
      if (key == null)
        return null;
      Entity result;
      using (var tx = Session.OpenAutoTransaction()) {
        var cache = Session.EntityStateCache;
        var state = cache[key, true];

        if (state == null) {
          if (Session.IsDebugEventLoggingEnabled)
            LogTemplate<Log>.Debug(Strings.LogSessionXResolvingKeyYExactTypeIsZ, Session, key,
                                   key.HasExactType ? Strings.Known : Strings.Unknown);
          state = Session.Handler.FetchEntityState(key);
        }

        if (state == null || state.IsNotAvailableOrMarkedAsRemoved || !key.TypeReference.Type.UnderlyingType.IsAssignableFrom(state.Type.UnderlyingType))
          // No state or Tuple = null or incorrect query type => no data in storage
          result = null;
        else
          result = state.Entity;

        tx.Complete();
      }
      return result;
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    public virtual T Single<T>(Key key)
      where T : class, IEntity
    {
      return (T)(object)Single(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    public virtual T Single<T>(params object[] keyValues)
      where T : class, IEntity
    {
      return (T)(object)Single(GetKeyByValues<T>(keyValues));
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    public virtual T SingleOrDefault<T>(Key key)
      where T : class, IEntity
    {
      return (T)(object)SingleOrDefault(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// </returns>
    public virtual T SingleOrDefault<T>(params object[] keyValues)
      where T : class, IEntity
    {
      return (T)(object)SingleOrDefault(GetKeyByValues<T>(keyValues));
    }

    /// <summary>
    /// Fetches multiple instances of specified type  by provided <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The source sequence.</param>
    /// <returns>The sequence of entities of type <typeparam name="T"/> matching provided <paramref name="keys"/>.</returns>
    public virtual IEnumerable<T> Many<T>(IEnumerable<Key> keys)
      where T : class, IEntity
    {
      return new PrefetchFacade<T>(Session, keys);
    }

    /// <summary>
    /// Fetches multiple instances of specified type  by provided <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The source sequence.</param>
    /// <returns>The sequence of entities of type <typeparam name="T"/> matching provided <paramref name="keys"/>.</returns>
    public virtual IEnumerable<T> Many<T,TElement>(IEnumerable<TElement> keys)
      where T : class, IEntity
    {
      var elementType = typeof (TElement);
      Func<TElement, Key> selector;
      if (elementType == typeof(object[]))
        selector = e => Key.Create<T>(Session.Domain, (object[]) (object) e);
      else if (typeof (Tuple).IsAssignableFrom(elementType))
        selector = e => Key.Create<T>(Session.Domain, (Tuple) (object) e);
      else
        selector = e => Key.Create<T>(Session.Domain, new object[] {e} );

      return new PrefetchFacade<T>(Session, keys.Select(selector));
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public virtual IEnumerable<TElement> Execute<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      return Execute(query.Method, query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public virtual IEnumerable<TElement> Execute<TElement>(object key, Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      return ExecuteInternal(GetParameterizedQuery(key, query), query.Target);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public virtual TResult Execute<TResult>(Func<QueryEndpoint,TResult> query)
    {
      return Execute(query.Method, query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public virtual TResult Execute<TResult>(object key, Func<QueryEndpoint,TResult> query)
    {
      var domain = Session.Domain;
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<object, TranslatedQuery> item;
      ParameterizedQuery<TResult> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(key, true, out item))
          parameterizedQuery = (ParameterizedQuery<TResult>)item.Second;
      if (parameterizedQuery == null) {
        ExtendedExpressionReplacer replacer;
        var queryParameter = BuildQueryParameter(target, out replacer);
        using (var queryCachingScope = new QueryCachingScope(queryParameter, replacer)) {
          TResult result;
          using (new ParameterContext().Activate()) {
            if (queryParameter != null)
              queryParameter.Value = target;
            result = query.Invoke(this);
          }
          parameterizedQuery = (ParameterizedQuery<TResult>)queryCachingScope.ParameterizedQuery;
          lock (cache)
            if (!cache.TryGetItem(key, false, out item))
              cache.Add(new Pair<object, TranslatedQuery>(key, parameterizedQuery));
          return result;
        }
      }
      return ExecuteInternal(parameterizedQuery, target);
    }

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query associated with the future scalar will be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public virtual Delayed<TResult> ExecuteDelayed<TResult>(object key, Func<QueryEndpoint,TResult> query)
    {
      var domain = Session.Domain;
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<object, TranslatedQuery> item;
      ParameterizedQuery<TResult> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(key, true, out item))
          parameterizedQuery = (ParameterizedQuery<TResult>)item.Second;
      if (parameterizedQuery == null) {
        ExtendedExpressionReplacer replacer;
        var queryParameter = BuildQueryParameter(target, out replacer);
        using (var queryCachingScope = new QueryCachingScope(queryParameter, replacer, false)) {
          using (new ParameterContext().Activate()) {
            if (queryParameter != null)
              queryParameter.Value = target;
            query.Invoke(this);
          }
          parameterizedQuery = (ParameterizedQuery<TResult>)queryCachingScope.ParameterizedQuery;
          lock (cache)
            if (!cache.TryGetItem(key, false, out item))
              cache.Add(new Pair<object, TranslatedQuery>(key, parameterizedQuery));
        }
      }
      var parameterContext = CreateParameterContext(target, parameterizedQuery);
      var result = new Delayed<TResult>(Session, parameterizedQuery, parameterContext);
      Session.RegisterDelayedQuery(result.Task);
      return result;
    }

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query associated with the future scalar will not be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public virtual Delayed<TResult> ExecuteDelayed<TResult>(Func<QueryEndpoint,TResult> query)
    {
      return ExecuteDelayed(query.Method, query);
    }

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public virtual IEnumerable<TElement> ExecuteDelayed<TElement>(object key, Func<QueryEndpoint,IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetParameterizedQuery(key, query);
      var parameterContext = CreateParameterContext(query.Target, parameterizedQuery);
      var result = new DelayedSequence<TElement>(Session, parameterizedQuery, parameterContext);
      Session.RegisterDelayedQuery(result.Task);
      return result;
    }

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public virtual IEnumerable<TElement> ExecuteDelayed<TElement>(Func<QueryEndpoint,IQueryable<TElement>> query)
    {
      return ExecuteDelayed(query.Method, query);
    }

    public virtual IQueryable<TElement> Store<TElement>(IEnumerable<TElement> source)
    {
      var method = WellKnownMembers.Queryable.AsQueryable.MakeGenericMethod(typeof(TElement));
      var expression = Expression.Call(method, Expression.Constant(source));
      return Provider.CreateQuery<TElement>(expression);
    }

    #region Private / internal methods

    /// <exception cref="ArgumentException"><paramref name="keyValues"/> array is empty.</exception>
    private Key GetKeyByValues<T>(object[] keyValues)
      where T : class, IEntity
    {
      ArgumentValidator.EnsureArgumentNotNull(keyValues, "keyValues");
      if (keyValues.Length == 0)
        throw new ArgumentException(Strings.ExKeyValuesArrayIsEmpty, "keyValues");
      if (keyValues.Length == 1) {
        var keyValue = keyValues[0];
        if (keyValue is Key)
          return keyValue as Key;
        if (keyValue is Entity)
          return (keyValue as Entity).Key;
      }
      return Key.Create(Session.Domain, typeof(T), keyValues);
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    private static Parameter BuildQueryParameter(object target, out ExtendedExpressionReplacer replacer)
    {
      if (target == null) {
        replacer = new ExtendedExpressionReplacer(e => e);
        return null;
      }
      var closureType = target.GetType();
      var parameterType = typeof(Parameter<>).MakeGenericType(closureType);
      var valueMemberInfo = parameterType.GetProperty("Value", closureType);
      var queryParameter = (Parameter) Activator.CreateInstance(parameterType, "pClosure");
      replacer = new ExtendedExpressionReplacer(e => {
                                                       if (e.NodeType == ExpressionType.Constant && e.Type.IsClosure()) {
                                                         if (e.Type == closureType)
                                                           return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
                                                         throw new NotSupportedException(String.Format(Strings.ExExpressionDefinedOutsideOfCachingQueryClosure, e));
                                                       }
                                                       return null;
      });
      return queryParameter;
    }

    private TResult ExecuteInternal<TResult>(ParameterizedQuery<TResult> query, object target)
    {
      var context = CreateParameterContext(target, query);
      return query.Execute(Session, context);
    }

    private ParameterizedQuery<IEnumerable<TElement>> GetParameterizedQuery<TElement>(object key, Func<QueryEndpoint,IQueryable<TElement>> query) 
    {
      var domain = Session.Domain;
      var cache = domain.QueryCache;
      Pair<object, TranslatedQuery> item;
      ParameterizedQuery<IEnumerable<TElement>> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(key, true, out item))
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>)item.Second;
      if (parameterizedQuery == null) {
        ExtendedExpressionReplacer replacer;
        var queryParameter = BuildQueryParameter(query.Target, out replacer);
        using (new QueryCachingScope(queryParameter, replacer)) {
          var result = query.Invoke(this);
          var translationResult = Provider.Translate<IEnumerable<TElement>>(result.Expression);
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>)translationResult.Query;
          lock (cache)
            if (!cache.TryGetItem(key, false, out item))
              cache.Add(new Pair<object, TranslatedQuery>(key, parameterizedQuery));
        }
      }
      return parameterizedQuery;
    }

    private Expression ReplaceClosure(Expression source, out object target, out Parameter parameter)
    {
      Type currentClosureType = null;
      Parameter queryParameter = null;
      Type parameterType = null;
      PropertyInfo valueMemberInfo = null;
      object currentTarget = null;
      var replacer = new ExtendedExpressionReplacer(e => {
                                                           if (e.NodeType == ExpressionType.Constant && e.Type.IsClosure()) {
                                                             if (currentClosureType == null) {
                                                               currentClosureType = e.Type;
                                                               currentTarget = ((ConstantExpression)e).Value;
                                                               parameterType = typeof(Parameter<>).MakeGenericType(currentClosureType);
                                                               valueMemberInfo = parameterType.GetProperty("Value", currentClosureType);
                                                               queryParameter = (Parameter)Activator.CreateInstance(parameterType, "pClosure", currentTarget);
                                                             }
                                                             if (e.Type == currentClosureType)
                                                               return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType),
                                                                                                  valueMemberInfo);
                                                             throw new InvalidOperationException(Strings.ExQueryContainsClosuresOfDifferentTypes);
                                                           }
                                                           return null;
      });
      var result = replacer.Replace(source);
      target = currentTarget;
      parameter = queryParameter;
      return result;
    }

    private static ParameterContext CreateParameterContext<TResult>(object target, ParameterizedQuery<TResult> query)
    {
      var parameterContext = new ParameterContext();
      if (query.QueryParameter != null)
        using (parameterContext.Activate())
          query.QueryParameter.Value = target;
      return parameterContext;
    }

    #endregion


    // Constructors

    public QueryEndpoint(Session session)
    {
      Session = session;
      Provider = new QueryProvider(session);
    }
  }
}