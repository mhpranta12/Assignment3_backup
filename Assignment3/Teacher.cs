using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3
{
    public class Teacher:IEntity<Guid>
    {
        public Guid Id { get; set; }    
        private string Name { get; set; }
        public int Age { get; set; }
        public Teacher (Guid id,string name,int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
    }
}
