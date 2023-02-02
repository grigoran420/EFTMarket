using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace eftMarket
{
    public class itemsPrice
    {
        [JsonInclude]
        public List<sellFor> sellFor = new List<sellFor>();
    }
}
