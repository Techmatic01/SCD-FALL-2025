using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using EntityFrameWorkExample.Model;
using EntityFrameWorkExample.Model.Entities;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=StudentDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=True"));
    })
    .Build();

// Get the database context
using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

try
{
    // Ensure the database is created
    await dbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("✅ Database created successfully!\n");

    // Seed Data
    if (!dbContext.Students.Any())
    {
        var s1 = new Student { Name = "Ali", Age = 20 };
        var s2 = new Student { Name = "Sara", Age = 22 };
        var s3 = new Student { Name = "Ahmed", Age = 19 };
        
        var c1 = new Course { Title = "Programming", Credits = 3 };
        var c2 = new Course { Title = "Database Systems", Credits = 4 };
        var c3 = new Course { Title = "Web Development", Credits = 3 };

        dbContext.AddRange(s1, s2, s3, c1, c2, c3);
        await dbContext.SaveChangesAsync();

        dbContext.Enrollments.AddRange(
            new Enrollment { StudentId = s1.Id, CourseId = c1.Id, Grade = "A" },
            new Enrollment { StudentId = s1.Id, CourseId = c2.Id, Grade = "B" },
            new Enrollment { StudentId = s2.Id, CourseId = c2.Id, Grade = "A" },
            new Enrollment { StudentId = s2.Id, CourseId = c3.Id, Grade = "B" },
            new Enrollment { StudentId = s3.Id, CourseId = c1.Id, Grade = "C" },
            new Enrollment { StudentId = s3.Id, CourseId = c3.Id, Grade = "A" }
        );
        await dbContext.SaveChangesAsync();
        
        Console.WriteLine("✅ Database seeded with sample data!\n");
    }

    // LINQ Queries
    Console.WriteLine("1️⃣ All Students:");
    var students = dbContext.Students.ToList();
    foreach (var s in students) 
        Console.WriteLine($"   {s.Id}: {s.Name}, Age: {s.Age}");

    Console.WriteLine("\n2️⃣ All Courses:");
    var courses = dbContext.Courses.ToList();
    foreach (var c in courses) 
        Console.WriteLine($"   {c.Id}: {c.Title}, Credits: {c.Credits}");

    Console.WriteLine("\n3️⃣ Students with Courses and Grades:");
    var joinQuery = from e in dbContext.Enrollments
                    join s in dbContext.Students on e.StudentId equals s.Id
                    join c in dbContext.Courses on e.CourseId equals c.Id
                    select new { s.Name, c.Title, e.Grade };
    
    foreach (var item in joinQuery)
        Console.WriteLine($"   {item.Name} → {item.Title} (Grade: {item.Grade})");

    Console.WriteLine("\n4️⃣ Students older than 20:");
    var olderStudents = dbContext.Students.Where(s => s.Age > 20).ToList();
    foreach (var s in olderStudents)
        Console.WriteLine($"   {s.Name}, Age: {s.Age}");

    Console.WriteLine("\n5️⃣ Students in 'Programming' course:");
    var programmingStudents = dbContext.Enrollments
        .Where(e => e.Course.Title == "Programming")
        .Select(e => e.Student)
        .ToList();
    foreach (var s in programmingStudents)
        Console.WriteLine($"   {s.Name}");

    Console.WriteLine("\n6️⃣ Average age of students:");
    var averageAge = dbContext.Students.Average(s => s.Age);
    Console.WriteLine($"   Average Age: {averageAge:F1}");

    Console.WriteLine("\n7️⃣ Highest credit course:");
    var highestCreditCourse = dbContext.Courses.OrderByDescending(c => c.Credits).FirstOrDefault();
    if (highestCreditCourse != null)
        Console.WriteLine($"   {highestCreditCourse.Title} ({highestCreditCourse.Credits} credits)");

    Console.WriteLine("\n8️⃣ Students with Grade 'A':");
    var gradeAStudents = dbContext.Enrollments
        .Where(e => e.Grade == "A")
        .Select(e => e.Student)
        .Distinct()
        .ToList();
    foreach (var s in gradeAStudents)
        Console.WriteLine($"   {s.Name}");

    Console.WriteLine("\n9️⃣ Course enrollment counts:");
    var enrollmentCounts = from c in dbContext.Courses
                           select new
                           {
                               CourseTitle = c.Title,
                               EnrollmentCount = c.Enrollments.Count
                           };
    foreach (var item in enrollmentCounts)
        Console.WriteLine($"   {item.CourseTitle}: {item.EnrollmentCount} students");

    Console.WriteLine("\n🔟 Students with their total credits:");
    var studentCredits = from s in dbContext.Students
                         select new
                         {
                             StudentName = s.Name,
                             TotalCredits = s.Enrollments.Sum(e => e.Course.Credits)
                         };
    foreach (var item in studentCredits)
        Console.WriteLine($"   {item.StudentName}: {item.TotalCredits} credits");

    // ===== ASSIGNMENT QUERIES =====
    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("📚 ASSIGNMENT QUERIES");
    Console.WriteLine(new string('=', 60));

    // Q1. Display all students who enrolled in "Database Systems"
    Console.WriteLine("\nQ1. Students enrolled in 'Database Systems':");
    var studentsInDB = dbContext.Enrollments
        .Where(e => e.Course.Title == "Database Systems")
        .Select(e => e.Student)
        .ToList();
    foreach (var s in studentsInDB)
        Console.WriteLine($"   {s.Name}");

    // Q2. Add a new course "Web Development" (3 credits)
    Console.WriteLine("\nQ2. Adding new course 'Web Development'...");
    var webDevCourse = new Course { Title = "Web Development", Credits = 3 };
    dbContext.Courses.Add(webDevCourse);
    await dbContext.SaveChangesAsync();
    Console.WriteLine("   ✅ Course added successfully!");

    // Q3. Enroll Sara into "Web Development" with grade "A+"
    Console.WriteLine("\nQ3. Enrolling Sara into 'Web Development'...");
    var sara = dbContext.Students.FirstOrDefault(s => s.Name == "Sara");
    var webDev = dbContext.Courses.FirstOrDefault(c => c.Title == "Web Development");
    
    if (sara != null && webDev != null)
    {
        var enrollment = new Enrollment { StudentId = sara.Id, CourseId = webDev.Id, Grade = "A+" };
        dbContext.Enrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();
        Console.WriteLine("   ✅ Sara enrolled successfully!");
    }

    // Q4. Update Ali's age to 21
    Console.WriteLine("\nQ4. Updating Ali's age to 21...");
    var ali = dbContext.Students.FirstOrDefault(s => s.Name == "Ali");
    if (ali != null)
    {
        ali.Age = 21;
        await dbContext.SaveChangesAsync();
        Console.WriteLine("   ✅ Ali's age updated successfully!");
    }

    // Q5. Delete the course "Programming"
    Console.WriteLine("\nQ5. Deleting course 'Programming'...");
    var programmingCourse = dbContext.Courses.FirstOrDefault(c => c.Title == "Programming");
    if (programmingCourse != null)
    {
        dbContext.Courses.Remove(programmingCourse);
        await dbContext.SaveChangesAsync();
        Console.WriteLine("   ✅ Course 'Programming' deleted successfully!");
    }

    // Q6. Show the number of students enrolled in each course
    Console.WriteLine("\nQ6. Number of students enrolled in each course:");
    var courseCounts = dbContext.Courses
        .Select(c => new
        {
            c.Title,
            StudentCount = c.Enrollments.Count()
        })
        .ToList();
    foreach (var item in courseCounts)
        Console.WriteLine($"   {item.Title}: {item.StudentCount} students");

    // Q7. Display average course credits
    Console.WriteLine("\nQ7. Average course credits:");
    var avgCredits = dbContext.Courses.Average(c => c.Credits);
    Console.WriteLine($"   Average Credits: {avgCredits:F1}");

    // Q8. Show all students with grades below "B"
    Console.WriteLine("\nQ8. Students with grades below 'B':");
    var lowGrades = dbContext.Enrollments
        .Where(e => string.Compare(e.Grade, "B") > 0)
        .Select(e => e.Student)
        .Distinct()
        .ToList();
    foreach (var s in lowGrades)
        Console.WriteLine($"   {s.Name}");

    // Q9. Sort students alphabetically by name
    Console.WriteLine("\nQ9. Students sorted alphabetically:");
    var sortedStudents = dbContext.Students.OrderBy(s => s.Name).ToList();
    foreach (var s in sortedStudents)
        Console.WriteLine($"   {s.Name}");

    // Q10. Display total number of students
    Console.WriteLine("\nQ10. Total number of students:");
    var totalStudents = dbContext.Students.Count();
    Console.WriteLine($"   Total Students: {totalStudents}");

    // ===== BONUS PRACTICE TASKS =====
    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("🎯 BONUS PRACTICE TASKS");
    Console.WriteLine(new string('=', 60));

    // Bonus 1: Display the highest and lowest course credit value
    Console.WriteLine("\nBonus 1. Highest and lowest course credits:");
    var maxCredits = dbContext.Courses.Max(c => c.Credits);
    var minCredits = dbContext.Courses.Min(c => c.Credits);
    Console.WriteLine($"   Highest: {maxCredits} credits");
    Console.WriteLine($"   Lowest: {minCredits} credits");

    // Bonus 2: Find all students who are not enrolled in any course
    Console.WriteLine("\nBonus 2. Students not enrolled in any course:");
    var unenrolledStudents = dbContext.Students
        .Where(s => !s.Enrollments.Any())
        .ToList();
    if (unenrolledStudents.Any())
    {
        foreach (var s in unenrolledStudents)
            Console.WriteLine($"   {s.Name}");
    }
    else
    {
        Console.WriteLine("   All students are enrolled in at least one course.");
    }

    // Bonus 3: Update all students with grade "F" to "Repeat"
    Console.WriteLine("\nBonus 3. Updating grade 'F' to 'Repeat'...");
    var fGrades = dbContext.Enrollments.Where(e => e.Grade == "F").ToList();
    foreach (var enrollment in fGrades)
    {
        enrollment.Grade = "Repeat";
    }
    if (fGrades.Any())
    {
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"   ✅ Updated {fGrades.Count} grades from 'F' to 'Repeat'");
    }
    else
    {
        Console.WriteLine("   No 'F' grades found to update.");
    }

    // Bonus 4: Delete all enrollments for a specific course (let's use "Web Development")
    Console.WriteLine("\nBonus 4. Deleting all enrollments for 'Web Development'...");
    var webDevEnrollments = dbContext.Enrollments
        .Where(e => e.Course.Title == "Web Development")
        .ToList();
    if (webDevEnrollments.Any())
    {
        dbContext.Enrollments.RemoveRange(webDevEnrollments);
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"   ✅ Deleted {webDevEnrollments.Count} enrollments for 'Web Development'");
    }
    else
    {
        Console.WriteLine("   No enrollments found for 'Web Development'.");
    }

    // Bonus 5: List all courses with no enrolled students
    Console.WriteLine("\nBonus 5. Courses with no enrolled students:");
    var emptyCourses = dbContext.Courses
        .Where(c => !c.Enrollments.Any())
        .ToList();
    if (emptyCourses.Any())
    {
        foreach (var c in emptyCourses)
            Console.WriteLine($"   {c.Title}");
    }
    else
    {
        Console.WriteLine("   All courses have enrolled students.");
    }

    // Bonus 6: Retrieve all students and the courses they're taking
    Console.WriteLine("\nBonus 6. All students and their courses:");
    var studentsWithCourses = dbContext.Students
        .Select(s => new
        {
            StudentName = s.Name,
            Courses = s.Enrollments.Select(e => e.Course.Title).ToList()
        })
        .ToList();
    foreach (var item in studentsWithCourses)
    {
        Console.WriteLine($"   {item.StudentName}: {string.Join(", ", item.Courses)}");
    }

    // Bonus 7: Count total enrollments in the system
    Console.WriteLine("\nBonus 7. Total enrollments in the system:");
    var totalEnrollments = dbContext.Enrollments.Count();
    Console.WriteLine($"   Total Enrollments: {totalEnrollments}");

    // Bonus 8: Display students grouped by grade
    Console.WriteLine("\nBonus 8. Students grouped by grade:");
    var studentsByGrade = dbContext.Enrollments
        .GroupBy(e => e.Grade)
        .Select(g => new { Grade = g.Key, Students = g.Select(e => e.Student.Name).Distinct().ToList() })
        .OrderBy(g => g.Grade)
        .ToList();
    foreach (var group in studentsByGrade)
    {
        Console.WriteLine($"   Grade {group.Grade}: {string.Join(", ", group.Students)}");
    }

    // Bonus 9: Show total credits earned by each student
    Console.WriteLine("\nBonus 9. Total credits earned by each student:");
    var studentCreditsEarned = dbContext.Students
        .Select(s => new
        {
            StudentName = s.Name,
            TotalCredits = s.Enrollments.Sum(e => e.Course.Credits)
        })
        .OrderByDescending(s => s.TotalCredits)
        .ToList();
    foreach (var item in studentCreditsEarned)
        Console.WriteLine($"   {item.StudentName}: {item.TotalCredits} credits");

    // Bonus 10: Get students enrolled in more than one course
    Console.WriteLine("\nBonus 10. Students enrolled in more than one course:");
    var multiCourseStudents = dbContext.Students
        .Where(s => s.Enrollments.Count() > 1)
        .ToList();
    foreach (var s in multiCourseStudents)
        Console.WriteLine($"   {s.Name} ({s.Enrollments.Count()} courses)");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine("Make sure SQL Server LocalDB is installed and running.");
}

Console.WriteLine("\n✅ Student Management System demonstration completed!");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();