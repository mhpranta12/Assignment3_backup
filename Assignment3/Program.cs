using Assignment3;
using System.Reflection;
Size smallSize = new Size() { Id = Guid.NewGuid(), Name = "Large", Length = 26 };
List<Size> sizes = new()
    {
    new Size(){Id = Guid.NewGuid(), Name="Low",Length=18},
    new Size(){Id = Guid.NewGuid(), Name="Extra Low",Length=26}
    };
List<Size> sizes2 = new()
    {
    new Size(){Id = Guid.NewGuid(), Name="Large",Length=30},
    new Size(){Id = Guid.NewGuid(), Name="Extra Extra Large",Length=34}
    };
List<Topic> topics = new List<Topic>();
Topic topic = new Topic() { Id = Guid.NewGuid(), Name = "Worker Service ", Sizes = sizes };
Topic topic2 = new Topic() { Id = Guid.NewGuid(), Name = "Clean Architecture", Sizes = sizes2 };
topics.Add(topic);
topics.Add(topic2);

Topic topic3 = new Topic()
{
    Id = Guid.NewGuid(),
    Name = "AutoFac DI",
    Sizes = sizes
};
List<Topic> topics2 = new List<Topic>() { topic3 };
Teacher teacher = new Teacher(Guid.NewGuid(),
    "Jalal Uddin", 38);
Courses mycourses = new()
{
    Id = Guid.NewGuid(),
    Name = "Asp.Net Core Mvc",
    Fees = 400000,
    CourseTeacher = teacher,
    Topics = topics2
};
MyORM<Guid, Courses> myORM = new MyORM<Guid, Courses>();
//myORM.Insert(mycourses);
////myORM.Update(mycourses);
//myORM.Delete(mycourses);
////myORM.Delete(new Guid("EDD0F238-083E-4C2E-8E50-B846768D587F"));
Courses records = myORM.GetById(new Guid("820DAACF-C06B-456E-B7E0-6004230406B5"));
Console.WriteLine(records.Name);
////MyORM<Guid, Size> ORM = new MyORM<Guid, Size>();
//var records = myORM.GetAll();
//foreach (var item in records)
//{
//    Console.WriteLine(item);
//    Console.WriteLine();
//}

//Console.WriteLine(records.Name);