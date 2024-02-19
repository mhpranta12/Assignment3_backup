using Assignment3;
using Assignment3Test;
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
//Courses mycourses = new()
//{
//    Id = Guid.NewGuid(),
//    Name = "Asp.Net Core Mvc",
//    Fees = 400000,
//    CourseTeacher = teacher,
//    Topics = topics2
//};
//MyORM<Guid, Courses> myORM = new MyORM<Guid, Courses>();
//myORM.Insert(mycourses);
////myORM.Update(mycourses);
//myORM.Delete(mycourses);
////myORM.Delete(new Guid("EDD0F238-083E-4C2E-8E50-B846768D587F"));
//Courses records = myORM.GetById(new Guid("820DAACF-C06B-456E-B7E0-6004230406B5"));
//Console.WriteLine(records.Name);
////MyORM<Guid, Size> ORM = new MyORM<Guid, Size>();
//var records = myORM.GetAll();
//foreach (var item in records)
//{
//    Console.WriteLine(item);
//    Console.WriteLine();
//}

//Console.WriteLine(records.Name);



var _myOrmWithUser = new MyORM<Guid, User>();
var _myOrmWithFeedback = new MyORM<Guid, Feedback>();
var _myOrmWithColor = new MyORM<Guid, Color>();
var _myOrmWithItem = new MyORM<Guid, Item>();
var _myOrmWithProduct = new MyORM<Guid, Product>();
var _myOrmWithVendor = new MyORM<Guid, Vendor>();

const int totalColorCount = 4;
const int totalFeedbackCount = 3;

var colorWhite = new Color { Id = IdentityGenerator.NewSequentialGuid(), Code = "#FF5733", Name = "White" };
var colorRed = new Color { Id = IdentityGenerator.NewSequentialGuid(), Code = "#FF0000", Name = "Red" };
var colorBlue = new Color { Id = IdentityGenerator.NewSequentialGuid(), Code = "#0000FF", Name = "Blue" };
var colorGreen = new Color { Id = IdentityGenerator.NewSequentialGuid(), Code = "#008000", Name = "Green" };

var user1 = new User { Id = IdentityGenerator.NewSequentialGuid(), Name = "Test User 1", Email = "testuser1@email.com" };
var user2 = new User { Id = IdentityGenerator.NewSequentialGuid(), Name = "Test User 2", Email = "testuser2@email.com" };
var user3 = new User { Id = IdentityGenerator.NewSequentialGuid(), Name = "Test User 3", Email = "testuser3@email.com" };

var feedbackGood = new Feedback { Id = IdentityGenerator.NewSequentialGuid(), FeedbackGiver = user1, Rating = 4.5, Comment = "Good feedback comment" };
var feedbackMedium = new Feedback { Id = IdentityGenerator.NewSequentialGuid(), FeedbackGiver = user2, Rating = 3, Comment = "Medium feedback comment" };
var feedbackBad = new Feedback { Id = IdentityGenerator.NewSequentialGuid(), FeedbackGiver = user3, Rating = 1, Comment = "Bad feedback comment" };

var item = new Item
{
    Id = IdentityGenerator.NewSequentialGuid(),
    Colors = new List<Color> { colorWhite, colorBlue, colorRed, colorGreen },
    Feedbacks = new List<Feedback> { feedbackGood, feedbackMedium, feedbackBad },
    Description = "Test item 1 description...",
    Name = "Test Item 1",
    PhotoUrl = "https://testimages.s3.ap-southeast-1.amazonaws.com/img1"
};

// Act
_myOrmWithItem.Insert(item);
var insertedItem = _myOrmWithItem.GetById(item.Id);
//var it =  _myOrmWithItem.GetById(new Guid("08AA4F18-D8A2-CF29-DFB5-08DC311A8E36"));
Console.WriteLine(insertedItem);

