using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3
{
    public class Courses : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Teacher CourseTeacher { get; set; } 
        public List<Topic> Topics { get; set; } 
        public double Fees { get; set; } 
    }
}
