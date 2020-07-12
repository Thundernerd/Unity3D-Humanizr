using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TNRD.Humanizr
{
    public class UnityResourceManager : ResourceManager
    {
        private Dictionary<string, Dictionary<string, string>> cultureToLocalizationData = new Dictionary<string, Dictionary<string, string>>();
        
        public UnityResourceManager(string baseName, Assembly assembly)
            : base(baseName, assembly)
        {
        }

        public override string GetString(string name, CultureInfo culture)
        {
            var data = GetCultureData(culture);
            if (data == null)
                return string.Empty;

            if (data.TryGetValue(name, out var value))
                return value;

            return string.Empty;
        }

        private Dictionary<string, string> GetCultureData(CultureInfo culture)
        {
            var path = $"Humanizr/{culture.Name}/resources";
            var asset = Resources.Load<TextAsset>(path);
            if (asset != null)
            {
                return ParseAndCacheJson(culture.Name, asset.text);
            }
            
            path = $"Humanizr/{culture.Parent.Name}/resources";
            asset = Resources.Load<TextAsset>(path);
            if (asset != null)
            {
                return ParseAndCacheJson(culture.Name, asset.text);
            }

            return null;
        }

        private Dictionary<string, string> ParseAndCacheJson(string name, string json)
        {
            if (cultureToLocalizationData.TryGetValue(name, out var data))
                return data;
            
            var dict = new Dictionary<string,string>();
            var token = JToken.Parse(json);
            foreach (var child in token.Children())
            {
                var property = (JProperty) child;
                dict.Add(property.Name, property.Value.Value<string>());
            }

            cultureToLocalizationData[name] = dict;
            return dict;
        }
    }
}
