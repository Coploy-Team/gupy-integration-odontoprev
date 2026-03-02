using System.Text.Json;
using System.Text.Json.Serialization;

namespace GupyIntegration.Models
{
  public class JobResponse
  {
    public List<Result> Results { get; set; } = [];
    public long TotalResults { get; set; }
    public long Page { get; set; }
    public long TotalPages { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string WorkplaceType { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public string ExternalUrl { get; set; } = string.Empty;
    public List<string> Requirements { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
  }

  public class Result
  {
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Responsibilities { get; set; } = string.Empty;
    public string Prerequisites { get; set; } = string.Empty;
    public string AdditionalInformation { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
    public string AddressCountry { get; set; } = string.Empty;
    public string WorkplaceType { get; set; } = string.Empty;
    public List<CustomField> CustomFields { get; set; } = [];
    public string ApplicationDeadline { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
  }

  public class CustomField
  {
    public Guid Id { get; set; }
    public bool CustomFieldRequired { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long Order { get; set; }
    public List<string> Enum { get; set; } = [];
    public Value? Value { get; set; }
  }

  [JsonConverter(typeof(ValueJsonConverter))]
  public struct Value
  {
    public long? Integer;
    public string String;
    public List<string> StringArray;
    public DateTimeOffset? DateTime;
    public bool? Boolean;

    public static implicit operator Value(long Integer) => new Value { Integer = Integer };
    public static implicit operator Value(string String) => new Value { String = String };
    public static implicit operator Value(List<string> StringArray) => new Value { StringArray = StringArray };
    public static implicit operator Value(DateTimeOffset DateTime) => new Value { DateTime = DateTime };
    public static implicit operator Value(bool Boolean) => new Value { Boolean = Boolean };
  }

  public class ValueJsonConverter : JsonConverter<Value>
  {
    public override Value Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      switch (reader.TokenType)
      {
        case JsonTokenType.True:
        case JsonTokenType.False:
          return new Value { Boolean = reader.GetBoolean() };
        case JsonTokenType.Number:
          return new Value { Integer = reader.GetInt64() };
        case JsonTokenType.String:
          var stringValue = reader.GetString();
          if (DateTimeOffset.TryParse(stringValue, out var dateTime))
            return new Value { DateTime = dateTime };
          return new Value { String = stringValue ?? string.Empty };
        case JsonTokenType.StartArray:
          var list = JsonSerializer.Deserialize<List<string>>(ref reader, options);
          return new Value { StringArray = list ?? new() };
        default:
          throw new JsonException($"Unexpected token type: {reader.TokenType}");
      }
    }

    public override void Write(Utf8JsonWriter writer, Value value, JsonSerializerOptions options)
    {
      if (value.Boolean.HasValue)
        writer.WriteBooleanValue(value.Boolean.Value);
      else if (value.Integer.HasValue)
        writer.WriteNumberValue(value.Integer.Value);
      else if (value.DateTime.HasValue)
        writer.WriteStringValue(value.DateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
      else if (value.StringArray != null)
        JsonSerializer.Serialize(writer, value.StringArray, options);
      else if (value.String != null)
        writer.WriteStringValue(value.String);
      else
        writer.WriteNullValue();
    }
  }
}