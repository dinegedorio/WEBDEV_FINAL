using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CrudFinals.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

        public ActionResult Crud()
        {
            using (StudentEntities fe = new StudentEntities())
            {
                var student = fe.Students.ToList();
                return View(student);
            }
        }


        [HttpPost]
        public ActionResult AddUserToDatabase(FormCollection fc)
        {
            string username = fc["Username"];
            string email = fc["Email"];
            string password = fc["Password"];
            string gender = fc["Gender"];

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(gender))
            {
                ViewBag.ErrorMessage = "All fields are required";
                return View("Index");
            }

            // Check if the username is already taken
            using (UserEntities fe = new UserEntities())
            {
                if (fe.Users.Any(u => u.Username == username))
                {
                    ViewBag.ErrorMessage = "Username is already taken";
                    return View("Index");
                }
            }

            User user = new User
            {
                Username = username,
                Email = email,
                Password = password,
                Gender = gender
            };

            using (UserEntities fe = new UserEntities())
            {
                fe.Users.Add(user);
                fe.SaveChanges();
            }

            // Store username in session
            Session["Username"] = username;

            return RedirectToAction("Crud");
        }


        [HttpPost]
        public ActionResult Login(FormCollection fc)
        {
            string username = fc["Username"];
            string password = fc["Password"];

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and password are required");
                return View("Index");
            }

            // Check if it is the admin login
            if (username == "studentsyncadmin" && password == "studentsyncadmin")
            {
                Session["IsAdmin"] = true;
                return RedirectToAction("AdminDashboard");
            }

            // Use UserEntities context to query the Users table
            using (UserEntities userContext = new UserEntities())
            {
                var user = userContext.Users.FirstOrDefault(u => u.Username == username);

                if (user != null)
                {
                    if (user.Password == password)
                    {
                        Session["Username"] = username;
                        return RedirectToAction("Crud");
                    }
                }
            }

            ViewBag.ErrorMessage = "Invalid username or password";
            return View("Index");
        }

        [HttpPost]
        public ActionResult AddStudent(FormCollection fc)
        {
            //from the form collection
            string name = fc["Name"];
            string lastName = fc["LastName"];
            int age = int.Parse(fc["Age"]);
            int grade = int.Parse(fc["Grade"]);

            Student newStudent = new Student
            {
                StudentID = Guid.NewGuid().GetHashCode(),
                Name = name,
                LastName = lastName,
                Age = age,
                Grade = grade
            };

            //context addnew student to database
            using (StudentEntities context = new StudentEntities())
            {
                context.Students.Add(newStudent);
                context.SaveChanges();
            }

            return RedirectToAction("Crud");
        }

        [HttpPost]
        public ActionResult UpdateStudent(int studentId, string name, string lastName, int age, int grade)
        {
            using (StudentEntities context = new StudentEntities())
            {
                var student = context.Students.Find(studentId);

                if (student != null)
                {
                    // Update the non-identity columns
                    student.Name = name;
                    student.LastName = lastName;
                    student.Age = age;

                    student.Grade = grade;

                    // Save changes
                    context.SaveChanges();
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteStudent(int id)
        {
            using (StudentEntities context = new StudentEntities())
            {
                var student = context.Students.Find(id);

                if (student != null)
                {
                    context.Students.Remove(student);
                    context.SaveChanges();
                }
            }

            return RedirectToAction("Crud");
        }


        public ActionResult AdminDashboard()
        {
            // Check if the user is an admin
            if (Session["IsAdmin"] != null && (bool)Session["IsAdmin"])
            {
                using (UserEntities userContext = new UserEntities())
                using (StudentEntities studentContext = new StudentEntities())
                {
                    // Count the number of users
                    int userCount = userContext.Users.Count();

                    // Count the number of students
                    int studentCount = studentContext.Students.Count();
                    int maleCount = userContext.Users.Count(u => u.Gender == "Male");
                    int femaleCount = userContext.Users.Count(u => u.Gender == "Female");
                    int otherCount = userContext.Users.Count(u => u.Gender == "Other");

                    // Age distribution ranges
                    int ageRange0_10 = studentContext.Students.Count(s => s.Age >= 0 && s.Age <= 10);
                    int ageRange11_20 = studentContext.Students.Count(s => s.Age >= 11 && s.Age <= 20);
                    int ageRange21_30 = studentContext.Students.Count(s => s.Age >= 21 && s.Age <= 30);
                    int ageRange31_40 = studentContext.Students.Count(s => s.Age >= 31 && s.Age <= 40);
                    int ageRange41Plus = studentContext.Students.Count(s => s.Age > 40);

                    // Retrieve users for the table
                    var users = userContext.Users.ToList();

                    ViewBag.UserCount = userCount;
                    ViewBag.StudentCount = studentCount;
                    ViewBag.MaleCount = maleCount;
                    ViewBag.FemaleCount = femaleCount;
                    ViewBag.OtherCount = otherCount;
                    ViewBag.AgeRange0_10 = ageRange0_10;
                    ViewBag.AgeRange11_20 = ageRange11_20;
                    ViewBag.AgeRange21_30 = ageRange21_30;
                    ViewBag.AgeRange31_40 = ageRange31_40;
                    ViewBag.AgeRange41Plus = ageRange41Plus;
                    ViewBag.Users = users;

                    return View();
                }
            }

            return RedirectToAction("Index");
        }


    }

}
