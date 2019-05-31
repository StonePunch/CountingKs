using CountingKs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CountingKs.Converters
{
  public class LinkModelConverter : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      // Specifies that only LinkModel can be handled by this converter
      return objectType.Equals(typeof(LinkModel));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      return reader.Value;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      if (value is LinkModel model)
      {
        writer.WriteStartObject();

        writer.WritePropertyName("href");
        writer.WriteValue(model.Href);

        writer.WritePropertyName("rel");
        writer.WriteValue(model.Rel);

        if (!model.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
          writer.WritePropertyName("method");
          writer.WriteValue(model.Method);
        }

        if (model.IsTemplated)
        {
          writer.WritePropertyName("isTemplated");
          writer.WriteValue(model.IsTemplated);
        }

        writer.WriteEndObject();
      }
    }
  }
}