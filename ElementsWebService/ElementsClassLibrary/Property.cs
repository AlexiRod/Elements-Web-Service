using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElementsClassLibrary
{
    public class Property : IProperty
    {
        private string name, unit, elementSymbol;


        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public string ElementSymbol { get => elementSymbol; set => elementSymbol = value?.Trim(); }

        [JsonProperty]
        public string Name { get => name; set => name = value?.Replace("[&delta;B]", "&Delta;").Replace("[&gamma;]", "&gamma;").Replace("[&alpha;]", "&alpha;"); }
        [JsonProperty]
        //[DisplayFormat(DataFormatString = "{0:######,##0.00}", NullDisplayText = "-")]
        public decimal Value { get; set; }

        [JsonProperty]
        public string Unit { get => unit; set => unit = value?.Trim().Replace('*', '\0'); }

        [JsonProperty]
        public string? Comments { get; set; }

        [JsonProperty]
        public string? Reference { get; set; }
        public string GetFormattedValue()
        {
            return Value.ToString("0.##############");
        }
    }
}
