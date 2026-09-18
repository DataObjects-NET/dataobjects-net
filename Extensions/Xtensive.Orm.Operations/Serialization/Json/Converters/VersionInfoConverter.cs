// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Tuples;


namespace Xtensive.Orm.Operations.Serialization.Json
{
  /// <summary>
  /// 
  /// </summary>
  public class VersionInfoConverter : JsonConverter<VersionInfo>
  {
    private const string FieldTypesProperty = "FieldTypes";
    private const string FieldValuesProperty = "FieldValues";

    private readonly PropertyInfo tupleAccessor = typeof(VersionInfo).GetProperty("Value", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <inheritdoc/>
    public override VersionInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      reader.EnsureStartObject();

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      var propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(nameof(VersionInfo.IsVoid)))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(VersionInfo.IsVoid)), propertyName);
      _ = reader.Read();
      var isVoid = reader.GetBoolean();
      if (isVoid) {
        reader.EnsureEndObject();
        return VersionInfo.Void;
      }

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(FieldTypesProperty))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(FieldTypesProperty), propertyName);
      _ = reader.Read();
      var tupleTypes = JsonSerializer.Deserialize<string[]>(ref reader, options);

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(FieldValuesProperty))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(FieldValuesProperty), propertyName);
      _ = reader.Read();
      var tupleValues =  JsonSerializer.Deserialize<string[]>(ref reader, options);
      _ = reader.Read();
      reader.EnsureEndObject();

      var descriptor = ToDescriptorFromSerializableForm(tupleTypes);
      var tuple = ToTupleFromSerializableForm(descriptor, tupleValues);

      return new VersionInfo(tuple);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, VersionInfo value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();

      writer.WriteBoolean(options.ApplyNamingPolicy(nameof(VersionInfo.IsVoid)), value.IsVoid);

      if (!value.IsVoid) {
        var tuple = ExtractTuple(value);

        var fieldTypes = ToSerializableFormat(tuple.Descriptor);
        var formattedTuple = ToSerializableFormat(tuple);

        writer.WritePropertyName(options.ApplyNamingPolicy(FieldTypesProperty));
        JsonSerializer.Serialize<string[]>(writer, fieldTypes, options);

        writer.WritePropertyName(options.ApplyNamingPolicy(FieldValuesProperty));
        JsonSerializer.Serialize<string[]>(writer, formattedTuple, options);
      }

      writer.WriteEndObject();
    }


    private Tuples.Tuple ExtractTuple(VersionInfo value)
    {
      var tuple = (Tuples.Tuple) tupleAccessor.GetValue(value);
      return tuple.Clone();
    }

    private static string[] ToSerializableFormat(Xtensive.Tuples.Tuple tuple)
    {
      var rawFormat = tuple.Format();
      return rawFormat.Split(',');
    }

    private static string[] ToSerializableFormat(TupleDescriptor descriptor)
    {
      var types = new string[descriptor.Count];
      for (int i = 0, count = descriptor.Count; i < count; i++) {

        var fieldType = descriptor[i];

        // use simplyfied format for system types, which are always there and well-known
        types[i] = (fieldType.IsArray && IsCoreDotNetType(fieldType.GetElementType())) || IsCoreDotNetType(fieldType)
          ? fieldType.FullName
          : descriptor[i].AssemblyQualifiedName;
      }
      return types;


      static bool IsCoreDotNetType(Type type)
      {
        if (type.IsArray && IsCoreDotNetType(type.GetElementType())
            || (type.IsGenericType && type.GenericTypeArguments.All(static gt => IsCoreDotNetType(gt)))) {
          return true;
        }

        var assemblyName = type.Assembly.GetName().Name;

        return assemblyName.Equals("System.Private.CoreLib", StringComparison.Ordinal) 
          || assemblyName.Equals("mscorlib", StringComparison.Ordinal)
          || assemblyName.Equals("System.Runtime", StringComparison.Ordinal);
      }
    }

    private static TupleDescriptor ToDescriptorFromSerializableForm(string[] fieldTypes)
    {
      var types = fieldTypes.Select(static st => Type.GetType(st)).ToArray();
      return TupleDescriptor.Create(types);
    }

    private static Tuples.Tuple ToTupleFromSerializableForm(TupleDescriptor descriptor, string[] values)
    {
      var valuesString = string.Join(',', values);
      return descriptor.Parse(valuesString);
    }
  }


  internal static class TuplesExtesions
  {
    


    

    
  }
}
