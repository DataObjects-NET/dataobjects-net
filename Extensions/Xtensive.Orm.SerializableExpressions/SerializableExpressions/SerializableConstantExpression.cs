// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ConstantExpression"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableConstantExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ConstantExpression.Value"/>
    /// </summary>
    [IgnoreDataMember]
    [JsonInclude, JsonConverter(typeof(JsonConverters.ObjectToInferredTypesConverter))]
    public object Value;

    /// <summary>
    /// For DataContractSerializer serialization only.
    /// </summary>
    #pragma warning disable IDE0051
    [JsonIgnore]
    [DataMember(Name = nameof(Value))]
    private object SerializableValue
    {
      get { return DataContractConverters.ObjectToInferredTypesConverter.Default.Value.ToSerializable(Value, Type); }
      set { Value = DataContractConverters.ObjectToInferredTypesConverter.Default.Value.FromSerializable(value, Type); }
    }
    #pragma warning restore IDE0051
  }
}
