using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchanges
{
    public class Valute
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double Rate { get; set; }

        public Valute() { }
    }
}
