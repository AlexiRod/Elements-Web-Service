using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementsClassLibrary
{
    public class Element
    {
        [JsonProperty]
        public int Id { get; private set; }
        [JsonProperty]
        public string Symbol { get; set; }
        [JsonProperty]
        public float AtomicWeight { get; private set; }
        public float Density { get; private set; }
        [JsonProperty]
        public float MeltingTemp { get; private set; }
        [JsonProperty]
        public float BoilingTemp { get; private set; }
    }
}
