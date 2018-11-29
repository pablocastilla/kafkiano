using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Models
{
    public class IndexViewModel
    {
        public string ProductToAdd { get; set; }
        public int QuantityToAdd { get; set; }

        public string ProductToOrder { get; set; }
        public int QuantityToOrder { get; set; }
    }
}
