using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementsClassLibrary
{
    public class RIProperty : Property
    {
        [JsonProperty]
        public int Charge { get; private set; }

        [JsonProperty] 
        public int? CN { get; private set; }
    }
}
