using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eftMarket
{
    public class sellFor
    {
        [JsonInclude]
        public string source;
        [JsonInclude]
        public int price;
    }
}
