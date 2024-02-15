using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3
{
    public interface IEntity<Key>
    {
        public Key Id { get; set; }
    }
}
