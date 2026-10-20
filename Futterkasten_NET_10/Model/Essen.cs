using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futterkasten.Model
{
    public struct Essen : IEnumerable<Essen>
    {
        public string Name { get; set; }
        public bool MagJessy { get; set; }
        public bool MagRene { get; set; }
        public bool IsIgnored { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool IsAufwändig { get; set; }
        public bool JessysInsuFood { get; set; }

        public IEnumerator<Essen> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
