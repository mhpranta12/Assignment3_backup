using Assignment3;
using System.Reflection;
Size smallSize = new Size() { Id = new Guid("5E9E118C-2D81-43D6-A11E-B2FB046D587E"), Name = "Large", Length = 26 };
List<Size> sizes = new()
    {
    new Size(){Id = new Guid("6E73D9EB-1804-40AB-AF72-31C686F4FE33"), Name="Low",Length=18},
    new Size(){Id = new Guid("992EA3ED-A99D-4295-AF59-B7254DEABDD3"), Name="Extra Low",Length=26}
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
    Id = new Guid("292DE346-8C70-4458-A134-48AE35E6A6AA"),
    Name = "AutoFac DI",
    Sizes = sizes
};
List<Topic> topics2 = new List<Topic>() { topic3 };
Teacher teacher = new Teacher(new Guid("ECB4955C-E1E7-48C6-BE1A-66820A86372D"),
    "Jalal Uddin", 38);
Courses mycourses = new Courses()
{
    Id = new Guid("EDD0F238-083E-4C2E-8E50-B846768D587F"),
    Name = "Asp.Net Core Mvc",
    Fees = 400000,
    CourseTeacher = teacher,
    Topics = topics2
};
MyORM<Guid, Courses> myORM = new MyORM<Guid, Courses>();
//myORM.Insert(mycourses);
//myORM.Update(mycourses);
myORM.Delete(mycourses);
//myORM.Delete(new Guid("EDD0F238-083E-4C2E-8E50-B846768D587F"));
//var records = myORM.GetById(new Guid("EDD0F238-083E-4C2E-8E50-B846768D587F"));
//MyORM<Guid, Size> ORM = new MyORM<Guid, Size>();
//var records = ORM.GetAll();
//foreach (var item in records)
//{
//    Console.WriteLine(item);
//    Console.WriteLine();
//}

