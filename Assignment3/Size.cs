using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3
{
    public class Size:IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public Guid TeacherId { get; set; }
    }
}
