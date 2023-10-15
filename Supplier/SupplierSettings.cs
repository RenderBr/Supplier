using Auxiliary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Supplier
{
    public class SupplierSettings : ISettings
    {
        [JsonPropertyName("DatabaseType")]
        public string DatabaseType { get; set; } = "sqlite";
    }
}
