using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace eftMarket
{
    public class Items
    {
        [JsonInclude]
        public string id;
        [JsonInclude]
        public string normalized_name;
        [JsonInclude]
        public int base_price;
        [JsonInclude]
        public int width;
        [JsonInclude]
        public int height;
        [JsonInclude]
        public string icon_link;
    }
}
