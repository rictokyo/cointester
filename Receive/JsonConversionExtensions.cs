﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

public static class JsonConversionExtensions
{
    public static IDictionary ToDictionary(this JObject json)
    {
        var propertyValuePairs = json.ToObject<Dictionary<string, object>>();
        ProcessJObjectProperties(propertyValuePairs);
        ProcessJArrayProperties(propertyValuePairs);
        return propertyValuePairs;
    }

    public static IDictionary Deserialize(byte[] body)
    {
        IDictionary dic;

        using (var ms = new MemoryStream(body))
        using (var reader = new BsonDataReader(ms))
        {
            var serializer = new JsonSerializer();

            JObject obj = serializer.Deserialize(reader) as JObject;
            dic = obj.ToDictionary();
        }

        return dic["Value"] as IDictionary;
    }


    private static void ProcessJObjectProperties(IDictionary<string, object> propertyValuePairs)
    {
        var objectPropertyNames = (from property in propertyValuePairs
                                   let propertyName = property.Key
                                   let value = property.Value
                                   where value is JObject
                                   select propertyName).ToList();

        objectPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToDictionary((JObject)propertyValuePairs[propertyName]));
    }

    private static void ProcessJArrayProperties(IDictionary<string, object> propertyValuePairs)
    {
        var arrayPropertyNames = (from property in propertyValuePairs
                                  let propertyName = property.Key
                                  let value = property.Value
                                  where value is JArray
                                  select propertyName).ToList();

        arrayPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToArray((JArray)propertyValuePairs[propertyName]));
    }

    public static object[] ToArray(this JArray array)
    {
        return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
    }

    private static object ProcessArrayEntry(object value)
    {
        if (value is JObject)
        {
            return ToDictionary((JObject)value);
        }
        if (value is JArray)
        {
            return ToArray((JArray)value);
        }
        return value;
    }
}