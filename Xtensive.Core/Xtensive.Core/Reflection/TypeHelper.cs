// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.06.13

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Helpers;
using Xtensive.Core.Resources;
using Xtensive.Core.Threading;
using System.Linq;

namespace Xtensive.Core.Reflection
{
  /// <summary>
  /// <see cref="Type"/> related helper \ extension methods.
  /// </summary>
  public static class TypeHelper
  {
    private const string invokeMethodName = "Invoke";

    private static readonly object _lock = new object();
    private static int createDummyTypeNumber = 0;
    private static AssemblyBuilder assemblyBuilder;
    private static ModuleBuilder moduleBuilder;
    private static ThreadSafeDictionary<Type, Type[]> orderedInterfaces = 
      ThreadSafeDictionary<Type, Type[]>.Create(new object());

    /// <summary>
    /// Searches for associated class for <paramref name="forType"/>, creates its instance, if found.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">Type of result. Can be ether class or interface.</typeparam>
    /// <param name="forType">Type to search the associate for.</param>
    /// <param name="foundForType">Type the associate was found for.</param>
    /// <param name="associateTypeSuffixes">Associate type name suffix.</param>
    /// <param name="constructorParams">Parameters to pass to associate constructor.</param>
    /// <returns>Newly created associate for <paramref name="forType"/>, if found;
    /// otherwise, <see langword="null"/>.</returns>
    public static T CreateAssociate<T>(Type forType, out Type foundForType, string[] associateTypeSuffixes,
      object[] constructorParams)
      where T : class
    {
      return
        CreateAssociate<T>(forType, out foundForType, associateTypeSuffixes, constructorParams,
          ArrayUtils<Pair<Assembly, string>>.EmptyArray);
    }

    /// <summary>
    /// Searches for associated class for <paramref name="forType"/>, creates its instance, if found.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">Type of result. Can be ether class or interface.</typeparam>
    /// <param name="forType">Type to search the associate for.</param>
    /// <param name="foundForType">Type the associate was found for.</param>
    /// <param name="associateTypeSuffixes">Associate type name suffix.</param>
    /// <param name="highPriorityLocations">High-priority search locations (assembly + namespace pairs).</param>
    /// <param name="constructorParams">Parameters to pass to associate constructor.</param>
    /// <returns>Newly created associate for <paramref name="forType"/>, if found;
    /// otherwise, <see langword="null"/>.</returns>
    public static T CreateAssociate<T>(Type forType, out Type foundForType, string[] associateTypeSuffixes,
      object[] constructorParams, IEnumerable<Pair<Assembly, string>> highPriorityLocations)
      where T : class
    {
      return
        CreateAssociate<T>(forType, out foundForType, associateTypeSuffixes, constructorParams, highPriorityLocations,
          false);
    }

    /// <summary>
    /// Searches for associated class for <paramref name="forType"/>, creates its instance, if found.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">Type of result. Can be ether class or interface.</typeparam>
    /// <param name="forType">Type to search the associate for.</param>
    /// <param name="foundForType">Type the associate was found for.</param>
    /// <param name="associateTypeSuffixes">Associate type name suffix.</param>
    /// <param name="highPriorityLocations">High-priority search locations (assembly + namespace pairs).</param>
    /// <param name="constructorParams">Parameters to pass to associate constructor.</param>
    /// <returns>Newly created associate for <paramref name="forType"/>, if found;
    /// otherwise, <see langword="null"/>.</returns>
    /// <param name="exactTypeMatch">If <see langword="false"/> tries to create associates for base class, interfaces, arrays and <see cref="Nullable{T}"/>(if struct) too.</param>
    /// <exception cref="InvalidOperationException"><paramref name="forType"/> is generic type definition.</exception>
    public static T CreateAssociate<T>(Type forType, out Type foundForType, string[] associateTypeSuffixes,
      object[] constructorParams, IEnumerable<Pair<Assembly, string>> highPriorityLocations, bool exactTypeMatch)
      where T : class
    {
      ArgumentValidator.EnsureArgumentNotNull(forType, "forType");
      if (forType.IsGenericTypeDefinition)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantCreateAssociateForGenericTypeDefinitions, GetShortName(forType)));

      List<Pair<Assembly, string>> locations = new List<Pair<Assembly, string>>(1);
      if (highPriorityLocations!=null)
        locations.AddRange(highPriorityLocations);

      return
        CreateAssociateInternal<T>(forType, forType, out foundForType, associateTypeSuffixes, constructorParams,
          locations, exactTypeMatch);
    }

    private static T CreateAssociateInternal<T>(Type originalForType, Type currentForType, out Type foundForType,
      string[] associateTypeSuffixes, object[] constructorParams, List<Pair<Assembly, string>> locations,
      bool exactTypeMatch)
      where T : class
    {
      if (currentForType==null) {
        foundForType = null;
        return null;
      }

      string associateTypePrefix;
      Type[] genericArguments;

      // Possible cases: generic type, array type, regular type
      if (currentForType.IsGenericType) {
        // Generic type
        associateTypePrefix = currentForType.Name;
        genericArguments = currentForType.GetGenericArguments();
      }
      else if (currentForType.IsArray) {
        // Array type
        Type elementType = currentForType.GetElementType();
        int rank = currentForType.GetArrayRank();
        if (rank==1)
          associateTypePrefix = "Array`1";
        else
          associateTypePrefix = string.Format("Array{0}D`1", rank);
        genericArguments = new Type[] {elementType};
      }
      else if (currentForType==typeof (Enum)) {
        // Enum type
        Type underlyingType = Enum.GetUnderlyingType(originalForType);
        associateTypePrefix = "Enum`2";
        genericArguments = new Type[] {originalForType, underlyingType};
      }
      else if (currentForType==typeof (Array)) {
        // Untyped Array type
        foundForType = null;
        return null;
      }
      else {
        // Regular type
        associateTypePrefix = currentForType.Name;
        genericArguments = null;
      }

      // Replacing 'I' at interface types
      if (currentForType.IsInterface && associateTypePrefix.StartsWith("I"))
        associateTypePrefix = AddSuffix(associateTypePrefix.Substring(1), "Interface");

      // Search for exact associate
      T result =
        CreateAssociateInternal<T>(originalForType, currentForType, out foundForType, associateTypePrefix,
          associateTypeSuffixes,
          locations, genericArguments, constructorParams);
      if (result!=null)
        return result;

      if (exactTypeMatch) {
        foundForType = null;
        return null;
      }

      // Nothing is found; tryng to find an associate for base type (except Object)
      Type forTypeBase = currentForType.BaseType;
      if (forTypeBase!=null)
        while (forTypeBase!=typeof (object)) {
          result = CreateAssociateInternal<T>(originalForType, forTypeBase, out foundForType,
            associateTypeSuffixes, constructorParams, locations, true);
          if (result!=null)
            return result;
          forTypeBase = forTypeBase.BaseType;
        }

      // Nothing is found; tryng to find an associate for implemented interface
      Type[] interfaces = currentForType.GetInterfaces();
      int interfaceCount = interfaces.Length;
      BitArray suppressed = new BitArray(interfaceCount);
      while (interfaceCount > 0) {
        // Suppressing all the interfaces inherited from others
        // to allow their associates to not conflict with each other
        for (int i = 0; i < interfaceCount; i++) {
          for (int j = 0; j < interfaceCount; j++) {
            if (i==j)
              continue;
            if (interfaces[i].IsAssignableFrom(interfaces[j])) {
              suppressed[i] = true;
              break;
            }
          }
        }
        Type lastGoodInterfaceType = null;
        // Scanning non-suppressed interfaces
        for (int i = 0; i < interfaceCount; i++) {
          if (suppressed[i])
            continue;
          T resultForInterface = CreateAssociateInternal<T>(originalForType, interfaces[i], out foundForType,
            associateTypeSuffixes, constructorParams, locations, true);
          if (resultForInterface!=null) {
            if (result!=null)
              throw new InvalidOperationException(string.Format(
                Strings.ExMultipleAssociatesMatch,
                GetShortName(currentForType),
                GetShortName(result.GetType()),
                GetShortName(resultForInterface.GetType())));
            result = resultForInterface;
            lastGoodInterfaceType = foundForType;
            foundForType = null;
          }
        }
        if (result!=null) {
          foundForType = lastGoodInterfaceType;
          return result;
        }
        // Moving suppressed interfaces to the beginning
        // to scan them on the next round
        int k = 0;
        for (int i = 0; i < interfaceCount; i++) {
          if (suppressed[i]) {
            interfaces[k] = interfaces[i];
            suppressed[k] = false;
            k++;
          }
        }
        interfaceCount = k;
      }

      // Nothing is found; tryng to find an associate for Object type
      if (currentForType!=typeof (object)) {
        result = CreateAssociateInternal<T>(originalForType, typeof (object), out foundForType,
          "Object", associateTypeSuffixes,
          locations, null, constructorParams);
        if (result!=null)
          return result;
      }

      // Nothing is found at all
      foundForType = null;
      return null;
    }

    private static T CreateAssociateInternal<T>(Type originalForType, 
      Type currentForType, 
      out Type foundForType,
      string associateTypePrefix, 
      string[] associateTypeSuffixes,
      List<Pair<Assembly, string>> locations, 
      Type[] genericArguments, 
      object[] constructorParams)
        where T : class
    {
      int newLocationCount = 0;
      var pair = new Pair<Assembly, string>(typeof (T).Assembly, typeof (T).Namespace);
      if (locations.FindIndex(p => p.First==pair.First && p.Second==pair.Second)<0) {
        locations.Add(pair);
        newLocationCount++;
      }
      pair = new Pair<Assembly, string>(currentForType.Assembly, currentForType.Namespace);
      if (locations.FindIndex(p => p.First==pair.First && p.Second==pair.Second)<0) {
        locations.Add(pair);
        newLocationCount++;
      }

      try {
        for (int i = 0; i < locations.Count; i++) {
          Pair<Assembly, string> location = locations[i];
          for (int currentSuffix = 0; currentSuffix < associateTypeSuffixes.Length; currentSuffix++) {
            string associateTypeSuffix = associateTypeSuffixes[currentSuffix];
            // Trying exact type match (e.g. EnumerableInterfaceHandler`1<...>)
            string associateTypeName = AddSuffix(
              string.Format("{0}.{1}", location.Second, associateTypePrefix), associateTypeSuffix);
            T result = Activate(location.First, CorrectGenericSuffix(associateTypeName, genericArguments==null ? 0 : genericArguments.Length), genericArguments, constructorParams) as T;
            if (result!=null) {
              foundForType = currentForType;
              return result;
            }

            // Trying to paste original type as generic parameter
            result = Activate(location.First, CorrectGenericSuffix(associateTypeName, 1), new Type[] {originalForType}, constructorParams) as T;
            if (result!=null) {
              foundForType = currentForType;
              return result;
            }

            // Trying a generic one (e.g. EnumerableInterfaceHandler`2<T, ...>)
            Type[] newGenericArguments;
            if (genericArguments==null || genericArguments.Length==0) {
              newGenericArguments = new Type[] {originalForType};
              associateTypeName = AddSuffix(
                string.Format("{0}.{1}`1", location.Second, associateTypePrefix), associateTypeSuffix);
            }
            else {
              newGenericArguments = new Type[genericArguments.Length + 1];
              newGenericArguments[0] = originalForType;
              Array.Copy(genericArguments, 0, newGenericArguments, 1, genericArguments.Length);
              associateTypeName = AddSuffix(string.Format("{0}.{1}`{2}",
                location.Second,
                TrimGenericSuffix(associateTypePrefix),
                newGenericArguments.Length),
                associateTypeSuffix);
            }
            result = Activate(location.First, CorrectGenericSuffix(associateTypeName, newGenericArguments.Length), newGenericArguments, constructorParams) as T;
            if (result!=null) {
              foundForType = currentForType;
              return result;
            }
          }
        }
        foundForType = null;
        return null;
      }
      finally {
        for (int i = 0; i < newLocationCount; i++)
          locations.RemoveAt(locations.Count - 1);
      }
    }

    /// <summary>
    /// Creates new dummy type. Such types can be used
    /// as generic arguments (to instantiate unique generic
    /// instances).
    /// </summary>
    /// <param name="namePrefix">Prefix to include into type name.</param>
    /// <param name="inheritFrom">The type to inherit the dummy type from.</param>
    /// <returns><see cref="Type"/> object of newly created type.</returns>
    public static Type CreateDummyType(string namePrefix, Type inheritFrom)
    {
      return CreateDummyType(namePrefix, inheritFrom, false);
    }

    /// <summary>
    /// Creates new dummy type. Such types can be used
    /// as generic arguments (to instantiate unique generic
    /// instances).
    /// </summary>
    /// <param name="namePrefix">Prefix to include into type name.</param>
    /// <param name="inheritFrom">The type to inherit the dummy type from.</param>
    /// <param name="implementProtectedConstructorAccessor">If <see langword="true"/>, static method with name <see cref="DelegateHelper.AspectedProtectedConstructorCallerName"/> will be created for each constructor.</param>
    /// <returns><see cref="Type"/> object of newly created type.</returns>
    public static Type CreateDummyType(string namePrefix, Type inheritFrom, bool implementProtectedConstructorAccessor)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(namePrefix, "namePrefix");
      ArgumentValidator.EnsureArgumentNotNull(inheritFrom, "inheritFrom");


      int n = Interlocked.Increment(ref createDummyTypeNumber);
      string typeName = string.Format("{0}.Internal.{1}{2}", typeof (TypeHelper).Namespace, namePrefix, n);

      return CreateInheritedDummyType(typeName, inheritFrom, implementProtectedConstructorAccessor);
    }

    /// <summary>
    /// Creates new dummy type inherited from another type.
    /// </summary>
    /// <param name="typeName">Type name.</param>
    /// <param name="inheritFrom">The type to inherit the dummy type from.</param>
    /// <returns>New type.</returns>
    public static Type CreateInheritedDummyType(string typeName, Type inheritFrom)
    {
      return CreateInheritedDummyType(typeName, inheritFrom, false);
    }
    /// <summary>
    /// Creates new dummy type inherited from another type.
    /// </summary>
    /// <param name="typeName">Type name.</param>
    /// <param name="inheritFrom">The type to inherit the dummy type from.</param>
    /// <param name="implementProtectedConstructorAccessor">If <see langword="true"/>, static method with name <see cref="DelegateHelper.AspectedProtectedConstructorCallerName"/> will be created for each constructor.</param>
    /// <returns>New type.</returns>
    public static Type CreateInheritedDummyType(string typeName, Type inheritFrom, bool implementProtectedConstructorAccessor)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(typeName, "typeName");
      ArgumentValidator.EnsureArgumentNotNull(inheritFrom, "inheritFrom");
      EnsureEmitInitialized();
      lock (_lock) {
        TypeBuilder typeBuilder = moduleBuilder.DefineType(
          typeName,
          TypeAttributes.Public | TypeAttributes.Sealed,
          inheritFrom,
          ArrayUtils<Type>.EmptyArray
          );
        foreach (ConstructorInfo baseConstructor in inheritFrom.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
          Type[] parameterTypes = baseConstructor.GetParameters().Select(parameterInfo=>parameterInfo.ParameterType).ToArray();
          ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
            baseConstructor.Attributes,
            CallingConventions.Standard,
            parameterTypes
            );
          
          // Create constructor
          ILGenerator methodIL = constructorBuilder.GetILGenerator();
          methodIL.Emit(OpCodes.Ldarg_0);
          int parametersCount = baseConstructor.GetParameters().Count();
          for (short i = 1; i <= parametersCount; i++)
            methodIL.Emit(OpCodes.Ldarg, i);
          methodIL.Emit(OpCodes.Call, baseConstructor);
          methodIL.Emit(OpCodes.Ret);

          // Create ProtectedConstructorAccessor
          if (implementProtectedConstructorAccessor) {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(DelegateHelper.AspectedProtectedConstructorCallerName, 
              MethodAttributes.Private | MethodAttributes.Static, 
              CallingConventions.Standard, typeBuilder.UnderlyingSystemType, parameterTypes);
            ILGenerator accessorIL = methodBuilder.GetILGenerator();
            for (short i = 0; i < parameterTypes.Length; i++)
              accessorIL.Emit(OpCodes.Ldarg, i);
            accessorIL.Emit(OpCodes.Newobj, constructorBuilder);
            accessorIL.Emit(OpCodes.Ret);
          }
        }
        return typeBuilder.CreateType();
      }
    }

    private static void EnsureEmitInitialized()
    {
      if (moduleBuilder!=null)
        return;
      lock (_lock) {
        if (moduleBuilder!=null)
          return;
        var assemblyName = new AssemblyName("Xtensive.TypeHelper.GeneratedTypes");
        assemblyBuilder =
          AppDomain.CurrentDomain.DefineDynamicAssembly(
            assemblyName,
            AssemblyBuilderAccess.RunAndSave);
        var tmp = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName + ".dll");
        Thread.MemoryBarrier();
        moduleBuilder = tmp;
      }
    }

    /// <summary>
    /// Adds suffix to specified generic type name.
    /// </summary>
    /// <param name="typeName">Type name to add suffix for.</param>
    /// <param name="suffix">Suffix to add.</param>
    /// <returns>Specified generic type name with its suffix.</returns>
    public static string AddSuffix(string typeName, string suffix)
    {
      int i = typeName.IndexOf("`");
      if (i >= 0)
        return typeName.Substring(0, i) + suffix + typeName.Substring(i);
      i = typeName.IndexOf("<");
      if (i >= 0)
        return typeName.Substring(0, i) + suffix + typeName.Substring(i);
      return typeName + suffix;
    }

    /// <summary>
    /// Instantiates specified generic type; returns <see langword="null"/>, if either no such a type,
    /// or an error has occurred.
    /// </summary>
    /// <param name="assembly">Assembly where the type is located.</param>
    /// <param name="typeName">Name of the type to instantiate.</param>
    /// <param name="genericArguments">Generic arguments for the type to instantiate 
    /// (<see langword="null"/> means type isn't a generic type definition).</param>
    /// <param name="arguments">Arguments to pass to the type constructor.</param>
    /// <returns>An instance of specified type; <see langword="null"/>, if either no such a type,
    /// or an error has occurred.</returns>
    public static object Activate(Assembly assembly, string typeName, Type[] genericArguments, params object[] arguments)
    {
      ArgumentValidator.EnsureArgumentNotNull(assembly, "assembly");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(typeName, "typeName");
      Type type = assembly.GetType(typeName, false);
      if (type==null)
        return null;
      return Activate(type, genericArguments, arguments);
    }

    /// <summary>
    /// Instantiates specified generic type; returns <see langword="null"/>, if either no such a type,
    /// or an error has occurred.
    /// </summary>
    /// <param name="type">Generic type definition to instantiate.</param>
    /// <param name="genericArguments">Generic arguments for the type to instantiate 
    /// (<see langword="null"/> means <paramref name="type"/> isn't a generic type definition).</param>
    /// <param name="arguments">Arguments to pass to the type constructor.</param>
    /// <returns>An instance of specified type; <see langword="null"/>, if either no such a type,
    /// or an error has occurred.</returns>
    [DebuggerStepThrough]
    public static object Activate(this Type type, Type[] genericArguments, params object[] arguments)
    {
      try {
        if (type.IsGenericTypeDefinition ^ genericArguments!=null)
          return null;
        if (type.IsGenericTypeDefinition)
          type = type.MakeGenericType(genericArguments);
#if DEBUG
        // This code is just for suppression of possible exception during debugging
        if (arguments!=null) {
          bool cancelSearch = false;
          Type[] argumentTypes = new Type[arguments.Length];
          for (int i = 0; i < arguments.Length; i++) {
            object o = arguments[i];
            if (o==null)
              cancelSearch = true; // Actually a case when GetConstructor will fail, 
            // so we should fall back to Activator.CreateInstance
            argumentTypes[i] = o.GetType();
          }
          if (!cancelSearch && type.GetConstructor(argumentTypes)==null)
            return null;
        }
#endif
        return Activator.CreateInstance(type, arguments);
      }
      catch {
        return null;
      }
    }

    /// <summary>
    /// Orders the specified <paramref name="types"/> by their inheritance
    /// (very base go first).
    /// </summary>
    /// <param name="types">The types to sort.</param>
    /// <returns>The list of <paramref name="types"/> ordered by their inheritance.</returns>
    public static List<Type> OrderByInheritance(this IEnumerable<Type> types)
    {
      return TopologicalSorter<Type>.Sort(types, (t1, t2) => t1.IsAssignableFrom(t2));
    }


    /// <summary>
    /// Gets the interfaces of the specified type.
    /// </summary>
    /// <param name="type">The type to get the interfaces of.</param>
    /// <param name="orderByInheritance">Indicates interfaces must be ordered from the very base ones 
    /// to very ancestors.</param>
    /// <returns>The interfaces of the specified type.</returns>
    public static Type[] GetInterfaces(this Type type, bool orderByInheritance)
    {
      if (!orderByInheritance)
        return type.GetInterfaces();
      return orderedInterfaces.GetValue(type,
        _type => _type.GetInterfaces().OrderByInheritance().ToArray());
    }

    /// <summary>
    /// Builds correct full generic type name.
    /// </summary>
    /// <param name="type">A <see cref="Type"/> which name is built.</param>
    /// <returns>Full type name.</returns>
    public static string GetFullName(this Type type)
    {
      string result = type.Namespace + "." + type.Name;
      int arrayBracketPosition = result.IndexOf('[');
      if (arrayBracketPosition > 0)
        result = result.Substring(0, arrayBracketPosition);
      Type[] arguments = type.GetGenericArguments();
      if (arguments==null)
        arguments = ArrayUtils<Type>.EmptyArray;
      if (arguments.Length > 0) {
        StringBuilder sb = new StringBuilder();
        sb.Append(TrimGenericSuffix(result));
        sb.Append("<");
        string comma = "";
        foreach (Type argument in arguments) {
          sb.Append(comma);
          if (!type.IsGenericTypeDefinition)
            sb.Append(GetFullName(argument));
          comma = ",";
        }
        sb.Append(">");
        result = sb.ToString();
      }
      if (type.IsArray) {
        StringBuilder sb = new StringBuilder(result);
        Type elementType = type;
        while (elementType.IsArray) {
          sb.Append("[");
          int commaCount = elementType.GetArrayRank() - 1;
          for (int i = 0; i < commaCount; i++)
            sb.Append(",");
          sb.Append("]");
          elementType = elementType.GetElementType();
        }
        result = sb.ToString();
      }
      return result;
    }

    /// <summary>
    /// Builds correct short generic type name (without namespaces).
    /// </summary>
    /// <param name="type">A <see cref="Type"/> which name is built.</param>
    /// <returns>Short type name.</returns>
    public static string GetShortName(this Type type)
    {
      string result = type.Name;
      int arrayBracketPosition = result.IndexOf('[');
      if (arrayBracketPosition > 0)
        result = result.Substring(0, arrayBracketPosition);
      Type[] arguments = type.GetGenericArguments();
      if (arguments==null)
        arguments = ArrayUtils<Type>.EmptyArray;
      if (arguments.Length > 0) {
        StringBuilder sb = new StringBuilder();
        sb.Append(TrimGenericSuffix(result));
        sb.Append("<");
        string comma = "";
        foreach (Type argument in arguments) {
          sb.Append(comma);
          if (!type.IsGenericTypeDefinition)
            sb.Append(GetShortName(argument));
          comma = ",";
        }
        sb.Append(">");
        result = sb.ToString();
      }
      if (type.IsArray) {
        StringBuilder sb = new StringBuilder(result);
        Type elementType = type;
        while (elementType.IsArray) {
          sb.Append("[");
          int commaCount = elementType.GetArrayRank() - 1;
          for (int i = 0; i < commaCount; i++)
            sb.Append(",");
          sb.Append("]");
          elementType = elementType.GetElementType();
        }
        result = sb.ToString();
      }
      return result;
    }

    /// <summary>
    /// Gets the <see cref="CodeTypeReference"/> to the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">A referenced <see cref="Type"/>.</param>
    /// <returns><see cref="CodeTypeReference"/> to given type.</returns>
    public static CodeTypeReference GetCodeTypeReference(this Type type)
    {
      Dictionary<Type, CodeTypeReference> dcrProcessedTypes = new Dictionary<Type, CodeTypeReference>();
      return InnerGetCodeTypeReference(type, dcrProcessedTypes);
    }

    private static CodeTypeReference InnerGetCodeTypeReference(Type type,
      Dictionary<Type, CodeTypeReference> dcrProcessedTypes)
    {
      CodeTypeReference typeReference;
      if (dcrProcessedTypes.ContainsKey(type))
        return dcrProcessedTypes[type];
      else
        dcrProcessedTypes[type] = typeReference = new CodeTypeReference();

      if (typeof (Array).IsAssignableFrom(type)) {
        Type elementType = type.GetElementType();
        if (elementType==null)
          typeReference.BaseType = typeof (Array).FullName;
        else {
          typeReference.ArrayElementType = InnerGetCodeTypeReference(type.GetElementType(), dcrProcessedTypes);
          typeReference.ArrayRank = type.GetArrayRank();
        }
      }
      else if (type.IsGenericParameter)
        typeReference.BaseType = type.Name;
      else if (type.IsGenericType || type.ContainsGenericParameters) {
        typeReference.BaseType = type.FullName!=null && !type.IsGenericType
          ? type.FullName
          : type.Namespace + "." + type.Name;
        Type[] typeArguments = type.GetGenericArguments();
        for (int index = 0, count = typeArguments.Length; index < count; index++)
          typeReference.TypeArguments.Add(InnerGetCodeTypeReference(typeArguments[index], dcrProcessedTypes));
      }
      else
        typeReference.BaseType = type.FullName ?? type.Namespace + "." + type.Name;
      return typeReference;
    }

    /// <summary>
    /// Indicates whether <paramref name="type"/> is a <see cref="Nullable{T}"/> type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns><see langword="True"/> if type is nullable type;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsNullable(this Type type)
    {
      return type.IsValueType && type.IsGenericType &&
        type.GetGenericTypeDefinition()==typeof (Nullable<>);
    }

    /// <summary>
    /// Indicates whether <typeparamref name="T"/> type is a <see cref="Nullable{T}"/> type.
    /// </summary>
    /// <typeparam name="T">Type to check.</typeparam>
    /// <returns><see langword="True"/> if type is nullable type;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsNullable<T>()
    {
      return ReferenceEquals(default(T), null) && typeof (T).IsValueType;
    }

    /// <summary>
    /// Indicates whether <paramref name="type"/> is a final type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns><see langword="True"/> if type is final type;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsFinal(this Type type)
    {
      return type.IsValueType || type.IsSealed;
    }

    /// <summary>
    /// Indicates whether <typeparamref name="T"/> type is a final type.
    /// </summary>
    /// <typeparam name="T">Type to check.</typeparam>
    /// <returns><see langword="True"/> if type is final type;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsFinal<T>()
    {
      return IsFinal(typeof (T));
    }

    /// <summary>
    /// Gets the delegate "Invoke" method (describing the delegate) for 
    /// the specified <paramref name="delegateType"/>.
    /// </summary>
    /// <param name="delegateType">Type of the delegate to get the "Invoke" method of.</param>
    /// <returns><see cref="MethodInfo"/> object describing the delegate "Invoke" method.</returns>
    public static MethodInfo GetInvokeMethod(this Type delegateType)
    {
      return delegateType.GetMethod(invokeMethodName);
    }

    #region Private \ internal methods

    private static string TrimGenericSuffix(string @string)
    {
      int i = @string.IndexOf('`');
      return i<0 ? @string : @string.Substring(0, i);
    }

    private static string CorrectGenericSuffix(string typeName, int argumentCount)
    {
      int commaPosition = typeName.IndexOf('`');
      if (commaPosition > 0) {
        typeName = typeName.Substring(0, commaPosition);
      }
      if (argumentCount==0)
        return typeName;
      else
        return string.Format("{0}`{1}", typeName, argumentCount);
    }

    #endregion
  }
}