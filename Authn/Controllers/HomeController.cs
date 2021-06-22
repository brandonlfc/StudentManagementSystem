using Authn.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Aspose.Words;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web;

namespace Authn.Controllers
{
    public class HomeController : Controller
    {
        SqlConnection conn = new SqlConnection(Program.connString);
        SqlDataReader dr;

        List<Student> students = new List<Student>();
        List<Teacher> teachers = new List<Teacher>();
        List<Assignment> assignments = new List<Assignment>();

        List<TeacherSubmitted> teachersubmitted = new List<TeacherSubmitted>();

        List<StudentsAssignments> studentsAssignments = new List<StudentsAssignments>();
        List<SubmittedAssignments> submittedAssignments = new List<SubmittedAssignments>();
        List<NotSubmitted> notSubmittedAssignments = new List<NotSubmitted>();

        //List of students attendance, attitude and acheivement to be displayed on the student page
        List<StatsAttendance> attendance = new List<StatsAttendance>();
        List<StatsAttitude> attitude = new List<StatsAttitude>();
        List<StatsAchievement> achievement = new List<StatsAchievement>();

        List<Student> totalAssignments = new List<Student>();
        List<Student> studAssignments = new List<Student>();

        private readonly ILogger<HomeController> _logger;
        private IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }


        //Student submitting their assignment
        [Authorize(Roles = "Student")]
        public IActionResult SubmitAssignment(IFormFile fileUpload, int edit_ass_id)
        {
            GetStudentsClass(); //Get the students class

            int? Id = null;
            string FirstName = String.Empty;
            string LastName = String.Empty;
            string Class = String.Empty;

            //Set student details to previously initiliazed variables
            foreach (Student stud in students)
            {
                Id = Convert.ToInt32(stud.Id);
                FirstName = stud.First_Name;
                LastName = stud.Last_Name;
                Class = stud.Class;
            }


            string submitted = String.Empty;
            foreach (Student stud in students) //Runs this foreach loop once as only one student is in the last (the logged in student)
            {
                if (students.Count > 1)
                {
                    stud.SubmittedAssignments += ", " + edit_ass_id;
                }
                else
                {
                    stud.SubmittedAssignments += edit_ass_id;
                }
                string[] sub = stud.SubmittedAssignments.Split(", ");
                Array.Sort(sub);

                for (int i = 0; i < sub.Length; i++) //Run for the length of the students submitted assignments
                {
                    submitted += sub[i] + ", "; //Adds a comma after each assignment
                }
                if(submitted.Length > 0) //If theres submitted assignments
                {
                    submitted = submitted.Remove(submitted.Length - 2); //Removes the whitespace and the last comma
                }
            }

            string Assignments = String.Empty;

            foreach (Student stud in students)
            {
                string[] assignments = stud.Assignments.Split(", ");

                for (int i = 0; i < assignments.Length; i++) //Run for the length of the students assignments they need to complete
                {
                    if(assignments[i].Contains(edit_ass_id.ToString()))
                    {
                        submittedAssignments.Add(new SubmittedAssignments()
                        {
                            AssignmentID = edit_ass_id, //Adds the assignment the student has completed to a list
                        });
                    }
                    else
                    {
                        Assignments += assignments[i] + ", "; //Adds the assignment the student hasn't completed to a string
                    }
                }
                if (Assignments.Length > 0) //If there are assignments the student hasn't completed
                {
                    Assignments = Assignments.Remove(Assignments.Length - 2); //Remove the whitespace and the last comma
                }
                else
                {
                    Assignments = DBNull.Value.ToString(); //Set the string to null for the database if there are no assignments needed to complete
                }
            }

            SqlCommand cmd = new SqlCommand("assignAssignment", conn);
            //SqlCommand cmd = new SqlCommand("UPDATE Students SET Assignments = @assDue, SubmittedAssignments = @SubmittedAssignments WHERE Id=@StudentID", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Assignments", Assignments);
            cmd.Parameters.AddWithValue("@SubmittedAssignments", submitted);
            cmd.Parameters.AddWithValue("@Id", Id);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            

            string fileName = String.Empty; //Sets the default name of the image to null
            string pdfFileName = String.Empty;
            var guid = Guid.NewGuid();
            //Checks if a file was uploaded
            try
            {
                if (fileUpload != null)
                {
                    string ext = System.IO.Path.GetExtension(fileUpload.FileName); //Gets the extension of the image uploaded
                                                                                   //Saves the image name as a combination of the teachers first name, last name and adds a guid in case teachers have the same names
                    fileName = edit_ass_id + "_" + FirstName + LastName + "_" + guid + ext;

                    string saveLocation = ("Assignments/" + Class + "/").Trim(); //Where the image will be stored
                    saveLocation += fileName; //Adds the image name to where the image will be saved
                    var dir = Path.Combine(_env.WebRootPath, saveLocation); //Sets the save location of the image

                    //Saves the image to the folder specified with the name generated
                    using (var fileStream = new FileStream(dir, FileMode.Create, FileAccess.Write))
                    {
                        fileUpload.CopyTo(fileStream);
                    }
                    Document doc = new Document(dir);
                    pdfFileName = edit_ass_id + "_" + FirstName + LastName + "_" + guid + ".pdf";
                    doc.Save(Path.Combine(_env.WebRootPath, ("Assignments/" + Class + "/").Trim()) + pdfFileName);
                }

                //Runs the SQL Procedure Command
                SqlCommand cmd1 = new SqlCommand("submitAssignment", conn); //Estabalishes connection to database and use the stored procedure
                cmd1.CommandType = CommandType.StoredProcedure;

                cmd1.Parameters.AddWithValue("@AssignmentID", edit_ass_id);
                cmd1.Parameters.AddWithValue("@Name", pdfFileName);
                cmd1.Parameters.AddWithValue("@StudentID", Id);

                conn.Open(); //Open connection to database
                cmd1.ExecuteNonQuery();
                conn.Close(); //Close connection to database
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }

            return RedirectToAction("Student");
        }

        //Gets all the assignments the student hasn't submitted then stores them into the NotSubmitted List
        public List<NotSubmitted> GetNotSubmittedAssignments()
        {
            foreach (Student stud in students)
            {
                string[] studSubmitted = stud.SubmittedAssignments.Split(',');
                int i = 0; //Used to itterate through list items
                foreach (StudentsAssignments ass in studentsAssignments) //Goes through assignments in the list
                {
                    //If length is in range and student submitted assignments string contains assignment id from assignments list
                    if (studSubmitted.Length > i && studSubmitted[i].Contains(ass.AssignmentID.ToString())) //Submitted
                    {
                        i++;
                    }
                    else //Not Submitted
                    {
                        notSubmittedAssignments.Add(new NotSubmitted()
                        {
                            AssignmentID = ass.AssignmentID,
                            StartDate = ass.StartDate,
                            EndDate = ass.EndDate,
                            Details = ass.Details,
                        });
                    }
                }
            }

            return notSubmittedAssignments;
        }

        //Gets all the assignments the student has submitted then stores them into the SubmittedAssignments List
        public List<SubmittedAssignments> GetSubmittedAssignments()
        {
            foreach (Student stud in students)
            {
                string[] studSubmitted = stud.SubmittedAssignments.Split(',');
                int i = 0; //Used to itterate through list items
                foreach (StudentsAssignments ass in studentsAssignments) //Goes through assignments in the list
                {
                    //If length is in range and student submitted assignments string contains assignment id from assignments list
                    if (studSubmitted.Length > i && studSubmitted[i].Contains(ass.AssignmentID.ToString())) //Submitted
                    {
                        submittedAssignments.Add(new SubmittedAssignments()
                        {
                            StudentID = Convert.ToInt32(stud.Id),
                            AssignmentID = ass.AssignmentID,
                            StartDate = ass.StartDate,
                            EndDate = ass.EndDate,
                            Details = ass.Details,
                        });
                        i++;
                    }
                }
            }

            return submittedAssignments;
        }

        //Gets a list of all assignments depending on the logged in students class
        public List<StudentsAssignments> GetStudentsAssignments()
        {
            string studClass = String.Empty;
            foreach(Student stud in students)
            {
                studClass = stud.Class;
            }

            SqlCommand cmd = new SqlCommand("listAssignments", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Class", studClass);

            if (studentsAssignments.Count > 0)
            {
                studentsAssignments.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    studentsAssignments.Add(new StudentsAssignments() //
                    {
                        AssignmentID = Convert.ToInt32(dr["AssignmentID"]),
                        StartDate = Convert.ToDateTime(dr["Start_Date"]),
                        EndDate = Convert.ToDateTime(dr["End_Date"]),
                        Details = dr["Details"].ToString().Trim(),
                    });
                }
                conn.Close();

                return studentsAssignments;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
        }

        public List<Student> GetStudentsClass()
        {
            SqlCommand cmd = new SqlCommand("getClassStudent", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Email", this.User.Identity.Name); //Sends the loggined in teachers email into the procedure

            if (teachers.Count > 0)
            {
                teachers.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    students.Add(new Student() //Adds new item to teachers list
                    {
                        Id = (int)dr["Id"],
                        First_Name = dr["First_Name"].ToString().Trim(),
                        Last_Name = dr["Last_Name"].ToString().Trim(),
                        Email = dr["Email"].ToString().Trim(),
                        Class = dr["Class"].ToString().Trim(),
                        Timetable = dr["Timetable"].ToString().Trim(),
                        Assignments = dr["Assignments"].ToString().Trim(),
                        SubmittedAssignments = dr["SubmittedAssignments"].ToString().Trim(),
                    });
                }
                conn.Close();

                return students;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
        }

        public List<TeacherSubmitted> GetListOfSubmitted()
        {
            foreach (Student item in students)
            {
                SqlCommand cmd = new SqlCommand("getSubmitted", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@StudentID", item.Id);

                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    teachersubmitted.Add(new TeacherSubmitted() //
                    {
                        AssignmentID = Convert.ToInt32(dr["AssignmentID"]),
                        StudentID = Convert.ToInt32(dr["StudentID"]),
                        Details = dr["Details"].ToString(),
                        Name = dr["Name"].ToString(),
                        Class= dr["Class"].ToString(),
                    });
                }
                conn.Close();
            }

            return teachersubmitted;
        }

        public List<Teacher> GetTeachersClass()
        {
            SqlCommand cmd = new SqlCommand("getClassTeacher", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Email", this.User.Identity.Name); //Sends the loggined in teachers email into the procedure

            if (teachers.Count > 0)
            {
                teachers.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    teachers.Add(new Teacher() //Adds new item to teachers list
                    {
                        Id = (int)dr["Id"],
                        First_Name = dr["First_Name"].ToString().Trim(),
                        Last_Name = dr["Last_Name"].ToString().Trim(),
                        Email = dr["Email"].ToString().Trim(),
                        Class = dr["Class"].ToString().Trim(),
                        //Timetable = dr["Timetable"].ToString().Trim(),
                    });
                }
                conn.Close();

                return teachers;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
        }

        public List<Student> GetStudentsForTeacher()
        {
            var TeachersClass = String.Empty; //Creates an empty string
            foreach (Teacher item in teachers)
            {
                TeachersClass = item.Class;
            }

            SqlCommand cmd = new SqlCommand("listStudentsForTeacher", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Class", TeachersClass); //Checks User Input Email With Stored Email

            if (students.Count > 0)
            {
                students.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    students.Add(new Student() //Adds new item to students list
                    {
                        Id = (int)dr["Id"],
                        First_Name = dr["First_Name"].ToString().Trim(),
                        Last_Name = dr["Last_Name"].ToString().Trim(),
                        Email = dr["Email"].ToString().Trim(),
                        Assignments = dr["Assignments"].ToString().Trim(),
                        SubmittedAssignments = dr["SubmittedAssignments"].ToString().Trim(),
                    });
                }
                conn.Close();

                return students;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
        }
        public List<Student> GetStudents()
        {
            SqlCommand cmd = new SqlCommand("listStudents", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;

            if (students.Count > 0)
            {
                students.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    students.Add(new Student()
                    {
                        Id = (int)dr["Id"],
                        First_Name = dr["First_Name"].ToString().Trim(),
                        Last_Name = dr["Last_Name"].ToString().Trim(),
                        Email = dr["Email"].ToString().Trim(),
                        Class = dr["Class"].ToString().Trim(),
                        Phone = dr["Phone"].ToString().Trim(),
                        DOB = (DateTime)dr["D.O.B"],
                        Ethnicity = dr["Ethnicity"].ToString().Trim(),
                        Timetable = dr["Timetable"].ToString().Trim(),
                        Assignments = dr["Assignments"].ToString().Trim(),
                        SubmittedAssignments = dr["SubmittedAssignments"].ToString().Trim(),
                    });
                }
                conn.Close();
                return students;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
        }

        public List<Teacher> GetTeachers()
        {
            SqlCommand cmd = new SqlCommand("listTeachers", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;

            if (teachers.Count > 0)
            {
                teachers.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    teachers.Add(new Teacher() //Adds new item to teachers list
                    {
                        Id = (int)dr["Id"],
                        First_Name = dr["First_Name"].ToString().Trim(),
                        Last_Name = dr["Last_Name"].ToString().Trim(),
                        Email = dr["Email"].ToString().Trim(),
                        Class = dr["Class"].ToString().Trim(),
                        //Timetable = dr["Timetable"].ToString().Trim(),
                    });
                }
                return teachers;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
        }

        public List<Assignment> GetAssignments()
        {
            var TeachersClass = String.Empty;
            foreach (Teacher item in teachers) //Gets each item in the teachers list
            {
                TeachersClass = item.Class; //Saves the class from the list to the TeacherClass variable
            }

            SqlCommand cmd = new SqlCommand("listAssignments", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Class", TeachersClass);

            if (assignments.Count > 0)
            {
                assignments.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    assignments.Add(new Assignment() //Adds new item to assignments list
                    {
                        AssignmentID = (int)dr["AssignmentID"],
                        Start_Date = dr["Start_Date"].ToString().Trim(),
                        End_Date = dr["End_Date"].ToString().Trim(),
                        Details = dr["Details"].ToString().Trim(),
                        Class = dr["Class"].ToString().Trim(),
                    });
                }
                conn.Close();
                return assignments;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
        }

        public void AssignAssignment()
        {
            //Adds all the assignments created to a list
            foreach (Assignment item in assignments)
            {
                totalAssignments.Add(new Student()
                {
                    Assignments = item.AssignmentID.ToString(),
                });
            }

            foreach (Student stud in students) //All students in students assignments table
            {
                if (stud.SubmittedAssignments != String.Empty) //Check if the student has submitted no assignments
                {
                    //Splits the list of submitted assignmets at the comma to store into the string array
                    string[] studSubmitted = stud.SubmittedAssignments.Split(',');
                    string notSubmitted = String.Empty; //Sets assignments that hasn't been complete back to nothing for each student in the list
                    int i = 0; //Used to itterate through list items
                    foreach (Assignment ass in assignments) //Goes through assignments in the list
                    {
                        //If length is in range and student submitted assignments string contains assignment id from assignments list
                        if (studSubmitted.Length > i && studSubmitted[i].Contains(ass.AssignmentID.ToString()))
                        {
                            //Edits database to set complete assessments
                            i++;
                        }
                        else
                        {
                            notSubmitted += ass.AssignmentID + ", "; //Adds comma after each assignment id not submitted
                            //Edits database to set incompleted assessments
                        }
                    }
                    //Sets the student that has some assignments completed and sets the assignments that they haven't completed yet
                    SqlCommand cmd1 = new SqlCommand("assignAssignment", conn);
                    cmd1.Connection = conn;
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.AddWithValue("@Id", stud.Id);

                    if (notSubmitted.Length > 0) //If there are any assignments not submitted
                    {
                        notSubmitted = notSubmitted.Remove(notSubmitted.Length - 2); //Removes the leading comma and white space
                        cmd1.Parameters.AddWithValue("@Assignments", notSubmitted);
                    }
                    else //If student has completed all assignments, set assignments to complete to null
                    {
                        cmd1.Parameters.AddWithValue("@Assignments", DBNull.Value);
                    }

                    cmd1.Parameters.AddWithValue("@SubmittedAssignments", stud.SubmittedAssignments.Trim());

                    conn.Open();
                    cmd1.ExecuteNonQuery();
                    conn.Close();

                }
                else if (stud.SubmittedAssignments == String.Empty) //If student hasn't completed any assignments and there are assignments to complete
                {
                    string notSubmitted = String.Empty;
                    foreach(Assignment ass in assignments) //Each item in the assignments list
                    {
                        notSubmitted += ass.AssignmentID + ", "; //Adds comma after each assignment id not submitted
                    }
                    if (notSubmitted.Length > 0) //If there are assignments the student hasn't completed
                    {
                        notSubmitted = notSubmitted.Remove(notSubmitted.Length - 2); //Removes the leading comma and white space
                    }
                    else
                    {
                        notSubmitted = DBNull.Value.ToString(); //Set the string to null for the database if there are no assignments needed to complete
                    }
                    
                    SqlCommand cmd2 = new SqlCommand("assignAssignment", conn);
                    cmd2.Connection = conn;

                    cmd2.CommandType = CommandType.StoredProcedure;

                    cmd2.Parameters.AddWithValue("@Id", stud.Id);
                    cmd2.Parameters.AddWithValue("@Assignments", notSubmitted);
                    cmd2.Parameters.AddWithValue("@SubmittedAssignments", DBNull.Value);

                    conn.Open();
                    cmd2.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }


        public List<StatsAttendance> GetAttendance()
        {
            SqlCommand cmd = new SqlCommand("getStudentAttendance", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            int? StudentID = null;
            foreach(Student stud in students)
            {
                StudentID = stud.Id;
            }

            cmd.Parameters.AddWithValue("@StudentID", StudentID);


            if (attendance.Count > 0)
            {
                attendance.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    attendance.Add(new StatsAttendance() //
                    {
                        Attendance = (int)dr["Attendance"],
                    });
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }

            return attendance;
        }

        public List<StatsAttitude> GetAttitude()
        {
            SqlCommand cmd = new SqlCommand("getStudentAttitude", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            int? StudentID = null;
            foreach (Student stud in students)
            {
                StudentID = stud.Id;
            }

            cmd.Parameters.AddWithValue("@StudentID", StudentID);


            if (attitude.Count > 0)
            {
                attitude.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    attitude.Add(new StatsAttitude() //
                    {
                        Attitude = (float)dr["Attitude"],
                    });
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }

            return attitude;
        }

        public List<StatsAchievement> GetAchievement()
        {
            SqlCommand cmd = new SqlCommand("getStudentAchievement", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            int? StudentID = null;
            foreach (Student stud in students)
            {
                StudentID = stud.Id;
            }

            cmd.Parameters.AddWithValue("@StudentID", StudentID);


            if (achievement.Count > 0)
            {
                achievement.Clear();
            }
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    achievement.Add(new StatsAchievement() //
                    {
                        Achievement = (float)dr["Score"],
                    });
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }

            return achievement;
        }




        [Authorize(Roles = "Student")]
        public IActionResult Student()
        {
            dynamic mymodel = new ExpandoObject();
            mymodel.Students = GetStudentsClass();
            mymodel.Assignments = GetStudentsAssignments();
            mymodel.SubmittedAssignments = GetSubmittedAssignments();
            mymodel.NotSubmittedAssignments = GetNotSubmittedAssignments();

            mymodel.Attendance = GetAttendance();
            mymodel.Attitude = GetAttitude();
            mymodel.Achievement = GetAchievement();

            return View(mymodel);
        }


        [Authorize(Roles = "Teacher")]
        public IActionResult SubmitAttitude(int[] StudentID, int[] Attitude, DateTime Date)
        {
            for (int i = 0; i < StudentID.Length; i++) //Runs for how ever many students are in the class
            {
                SqlCommand cmd = new SqlCommand("submitAttitude", conn);
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Date", Date);
                cmd.Parameters.AddWithValue("@Attitude", Attitude[i]);
                cmd.Parameters.AddWithValue("@StudentID", StudentID[i]);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return RedirectToAction("Teacher");
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult SubmitAttendance(int[] StudentID, string[] Attendance, DateTime Date)
        {
            for (int i = 0; i < StudentID.Length; i++) //Runs for how ever many students are in the class
            {
                int AttendancePoints = 0;
                int LateBy = 0;
                switch (Attendance[i])
                {
                    case "Present":
                        AttendancePoints = 240;
                        break;
                    case "Late":
                        AttendancePoints = 240 - LateBy;
                        break;
                    case "Absent":
                        break;
                }

                SqlCommand cmd = new SqlCommand("submitAttendance", conn);
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Date", Date);
                cmd.Parameters.AddWithValue("@Attendance", AttendancePoints);
                cmd.Parameters.AddWithValue("@StudentID", StudentID[i]);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return RedirectToAction("Teacher");
        }

        //Teacher views students submited assignments then grades them
        [Authorize(Roles ="Teacher")]
        public IActionResult GradeAssignment(int AssignmentID, float Grade, int StudentID)
        {
            SqlCommand cmd = new SqlCommand("assignAssignmentGrade", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@AssignmentID", AssignmentID);
            cmd.Parameters.AddWithValue("@Grade", Grade);
            cmd.Parameters.AddWithValue("@StudentID", StudentID);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return RedirectToAction("Teacher");
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult CreateAssignment(DateTime createStartDate, DateTime createEndDate, string createDetails)
        {
            GetAssignments();
            GetTeachersClass();
            var TeachersClass = String.Empty;
            foreach (Teacher item in teachers)
            {
                TeachersClass = item.Class;
            }

            //Runs the SQL Procedure Command
            SqlCommand cmd = new SqlCommand("createAssignment", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Start_Date", createStartDate);
            cmd.Parameters.AddWithValue("@End_Date", createEndDate);
            cmd.Parameters.AddWithValue("@Details", createDetails.Trim());
            cmd.Parameters.AddWithValue("@Class", TeachersClass.Trim());

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            //AssignAssignment();

            return RedirectToAction("Teacher");
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Teacher()
        {
            dynamic mymodel = new ExpandoObject();
            mymodel.Teachers = GetTeachersClass();
            mymodel.Students = GetStudentsForTeacher();
            mymodel.Assignments = GetAssignments();
            mymodel.Submitted = GetListOfSubmitted();
            AssignAssignment();

            return View(mymodel);
        }
        
        public void StudentTimetable(IFormFile updatedTimetable, string oldTimetable, string FirstName, string LastName, SqlCommand cmd)
        {
            string imgName = DBNull.Value.ToString();
            if (updatedTimetable != null)
            {
                //Checks if there is already an existing gile
                if (oldTimetable != null)
                {
                    imgName = oldTimetable; //Sets the updated files name to the existing file name
                }
                else
                {
                    string ext = System.IO.Path.GetExtension(updatedTimetable.FileName); //Gets the extension of the image uploaded
                    //Saves the image name as a combination of the teachers first name, last name and adds a guid in case teachers have the same names
                    imgName = FirstName + LastName + "_" + Guid.NewGuid() + ext;
                }
                string saveLocation = "Timetables/Students/"; //Where the image will be stored
                saveLocation += imgName; //Adds the image name to where the image will be saved
                var dir = Path.Combine(_env.WebRootPath, saveLocation); //Sets the save location of the image

                //Saves the image to the folder specified with the name generated
                using (var fileStream = new FileStream(dir, FileMode.Create, FileAccess.Write))
                {
                    updatedTimetable.CopyTo(fileStream);
                }
            }

            cmd.Parameters.AddWithValue("@Timetable", imgName.Trim());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UpdateStudentDetails(string edit_stud_id, string update_stud_first_name, string update_stud_last_name, string update_stud_email, string update_stud_class, string update_stud_phone, string update_stud_dob, string update_stud_ethnicity, IFormFile update_stud_timetable, string uploaded_stud_timetable)
        {
          
            SqlCommand cmd = new SqlCommand("updateStudent", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Id", edit_stud_id.Trim());
            cmd.Parameters.AddWithValue("@FirstName", update_stud_first_name.Trim());
            cmd.Parameters.AddWithValue("@LastName", update_stud_last_name.Trim());
            cmd.Parameters.AddWithValue("@Email", update_stud_email.Trim());
            cmd.Parameters.AddWithValue("@Class", update_stud_class.Trim());
            cmd.Parameters.AddWithValue("@Phone", update_stud_phone.Trim());
            cmd.Parameters.AddWithValue("@DOB", update_stud_dob.Trim());
            cmd.Parameters.AddWithValue("@Ethnicity", update_stud_ethnicity.Trim());

            conn.Open();
            try
            {
                StudentTimetable(update_stud_timetable, uploaded_stud_timetable, update_stud_first_name, update_stud_last_name, cmd);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error: ", ex);
            }
            conn.Close();

            return RedirectToAction("Secured");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UpdateTeacherDetails(string edit_teach_id, string update_teach_first_name, string update_teach_last_name, string update_teach_email, string update_teach_class)
        {
            /*
            string imgName = uploaded_teach_timetable;
            if (update_teach_timetable != null)
            {
                //Checks if there is already an existing gile
                if (uploaded_teach_timetable != null)
                {
                    imgName = uploaded_teach_timetable; //Sets the updated files name to the existing file name
                }
                else
                {
                    string ext = System.IO.Path.GetExtension(update_teach_timetable.FileName); //Gets the extension of the image uploaded
                    //Saves the image name as a combination of the teachers first name, last name and adds a guid in case teachers have the same names
                    imgName = update_teach_first_name + update_teach_last_name + "_" + Guid.NewGuid() + ext;
                }
                string saveLocation = "timetables/teachers/"; //Where the image will be stored
                saveLocation += imgName; //Adds the image name to where the image will be saved
                var dir = Path.Combine(_env.WebRootPath, saveLocation); //Sets the save location of the image

                //Saves the image to the folder specified with the name generated
                using (var fileStream = new FileStream(dir, FileMode.Create, FileAccess.Write))
                {
                    update_teach_timetable.CopyTo(fileStream);
                }
            }*/

            SqlCommand cmd = new SqlCommand("updateTeacher", conn);
            cmd.Connection = conn;

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Id", edit_teach_id.Trim());
            cmd.Parameters.AddWithValue("@First_Name", update_teach_first_name.Trim());
            cmd.Parameters.AddWithValue("@Last_Name", update_teach_last_name.Trim());
            cmd.Parameters.AddWithValue("@Email", update_teach_email.Trim());
            cmd.Parameters.AddWithValue("@Class", update_teach_class.Trim());
            //cmd.Parameters.AddWithValue("@Timetable", imgName.Trim());

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            
            return RedirectToAction("Secured");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteStudent(string edit_stud_id)
        {
            SqlCommand cmd = new SqlCommand("deleteStudent", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Id", edit_stud_id);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return RedirectToAction("Secured");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteTeacher(string edit_teach_id)
        {
            SqlCommand cmd = new SqlCommand("deleteTeacher", conn);
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Id", edit_teach_id);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return RedirectToAction("Secured");
        }

        //Registers teacher into the database from the Admin Portal
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterTeacher(string teach_first_name, string teach_last_name, string teach_email, string teach_class)
        {
            /*
            string imgName = DBNull.Value.ToString(); //Sets the default name of the image to null
            //Checks if a file was uploaded
            if (teach_timetable != null)
            {
                string ext = System.IO.Path.GetExtension(teach_timetable.FileName); //Gets the extension of the image uploaded
                //Saves the image name as a combination of the teachers first name, last name and adds a guid in case teachers have the same names
                imgName = teach_first_name + teach_last_name + "_" + Guid.NewGuid() + ext;

                string saveLocation = "timetables/teachers/"; //Where the image will be stored
                saveLocation += imgName; //Adds the image name to where the image will be saved
                var dir = Path.Combine(_env.WebRootPath, saveLocation); //Sets the save location of the image

                //Saves the image to the folder specified with the name generated
                using (var fileStream = new FileStream(dir, FileMode.Create, FileAccess.Write))
                {
                    teach_timetable.CopyTo(fileStream);
                }
            }*/

            //Runs the SQL Procedure Command
            SqlCommand cmd = new SqlCommand("registerTeacher", conn); //Estabalishes connection to database and use the stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@First_Name", teach_first_name.Trim());
            cmd.Parameters.AddWithValue("@Last_Name", teach_last_name.Trim());
            cmd.Parameters.AddWithValue("@Email", teach_email.Trim());
            cmd.Parameters.AddWithValue("@Class", teach_class.Trim());
            //cmd.Parameters.AddWithValue("@Timetable", imgName.Trim()); //Saves the timetable name to the database

            conn.Open(); //Open connection to database
            cmd.ExecuteNonQuery();
            conn.Close(); //Close connection to database

            return RedirectToAction("Secured"); //Redirect to Admin Page
        }

        //Registers student into the database from the Admin Portal
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterStudent(string stud_first_name, string stud_last_name, string stud_email, string stud_class, string stud_phone, string stud_dob, string stud_ethnicity, IFormFile stud_timetable)
        {
            string imgName = DBNull.Value.ToString(); //Sets the default name of the image to null
            //Checks if a file was uploaded
            if (stud_timetable != null)
            {
                string ext = System.IO.Path.GetExtension(stud_timetable.FileName); //Gets the extension of the image uploaded
                //Saves the image name as a combination of the teachers first name, last name and adds a guid in case teachers have the same names
                imgName = stud_first_name + stud_last_name + "_" + Guid.NewGuid() + ext;

                string saveLocation = "Timetables/Students/"; //Where the image will be stored
                saveLocation += imgName; //Adds the image name to where the image will be saved
                var dir = Path.Combine(_env.WebRootPath, saveLocation); //Sets the save location of the image

                //Saves the image to the folder specified with the name generated
                using (var fileStream = new FileStream(dir, FileMode.Create, FileAccess.Write))
                {
                    stud_timetable.CopyTo(fileStream);
                }
            }

            //Runs the SQL Procedure Command
            SqlCommand cmd = new SqlCommand("registerStudent", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@First_Name", stud_first_name.Trim());
            cmd.Parameters.AddWithValue("@Last_Name", stud_last_name.Trim());
            cmd.Parameters.AddWithValue("@Email", stud_email.Trim());
            cmd.Parameters.AddWithValue("@Class", stud_class.Trim());
            cmd.Parameters.AddWithValue("@Phone", stud_phone.Trim());
            cmd.Parameters.AddWithValue("@DOB", stud_dob.Trim());
            cmd.Parameters.AddWithValue("@Ethnicity", stud_ethnicity.Trim());
            cmd.Parameters.AddWithValue("@Timetable", imgName.Trim());

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return RedirectToAction("Secured");
        }

        //Redirects to Admin portal. Gets Student and Teacher list and stores into a model to be display on the Admin Porta
        [Authorize(Roles = "Admin")]
        public IActionResult Secured()
        {
            dynamic mymodel = new ExpandoObject();
            mymodel.Students = GetStudents();
            mymodel.Teachers = GetTeachers();

            return View(mymodel);
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("denied")]
        public IActionResult Denied()
        {
            return View();
        }

        public IActionResult PasswordReset()
        {
            ViewBag.ResetCode = Request.Query["guidcode"];
            ViewBag.User = Request.Query["user"];

            return View();
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult ResetUserPassword(string ConfirmNewPassword, string user, string ResetCode)
        {
            SqlCommand cmd = new SqlCommand(null, conn);

            if (user == "Student")
            {
                cmd.CommandText = "updatePasswordStudent";
            }
            else if (user == "Teacher")
            {
                cmd.CommandText = "updatePasswordTeacher";
            }
            else if (user == "Admin")
            {
                cmd.CommandText = "updatePasswordAdmin";
            }
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Password", ConfirmNewPassword);
            cmd.Parameters.AddWithValue("@ResetCode", ResetCode);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            
            TempData["PassChange"] = "Password Successfully Changed";
            return Redirect("~/Login?ReturnUrl=%2FHome%2FSecured");
        }

        public IActionResult ResetPassword(string Email, string usertype)
        {
            SqlCommand cmd = new SqlCommand(null, conn);
            if (usertype == "Student")
            {
                cmd.CommandText = "resetGetEmailStudent";
            }
            else if (usertype == "Teacher")
            {
                cmd.CommandText =  "resetGetEmailTeacher";
            }
            else if (usertype == "Admin")
            {
                cmd.CommandText =  "resetGetEmailAdmin";
            }
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", Email);

            conn.Open();
            int loginResult = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            if(loginResult == 1)
            {
                string GuidID = loginResult.ToString();
                TempData["GuidID"] = GuidID;

                string resetCode = Guid.NewGuid().ToString();
                SqlCommand cmd1 = new SqlCommand("assignResetCode"+usertype, conn);
                cmd1.CommandType = CommandType.StoredProcedure;

                cmd1.Parameters.AddWithValue("@Email", Email);
                cmd1.Parameters.AddWithValue("@ResetCode", resetCode);

                conn.Open();
                cmd1.ExecuteNonQuery();
                conn.Close();

                try
                {
                    SendEmail(Email, resetCode, usertype);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error: ", ex);
                }

                TempData["Success"] = "Success. An email has been sent.";
                return RedirectToAction("ForgotPassword");
            }
            else
            {
                TempData["Error"] = "Error. Incorrect Email";
                return RedirectToAction("ForgotPassword");
            }
        }

        //Email sending for forgotten password
        public void SendEmail(string Email, string resetCode, string userType)
        {
            using (MailMessage mm = new MailMessage("ProjectileProtectionIV@gmail.com", Email))
            {
                //string link = "https://localhost:5001/Home/PasswordReset?user="+userType+"&guidcode=" + resetCode;
                string link = "https://studentmanagement-system.azurewebsites.net/Home/PasswordReset?user=" + userType+"&guidcode=" + resetCode;
                mm.Subject = "Password Reset For Student Management Systems";
                mm.Body = "You have requested a password reset for your Student Management Systems account\n"
                        + "Please click the link below to change your password:\n" +  link;
                mm.BodyEncoding = Encoding.UTF8;
                mm.IsBodyHtml = true;

                /*
                SmtpClient smtp = new SmtpClient();
                smtp.UseDefaultCredentials = false;
                NetworkCredential NetworkCred = new NetworkCredential("ProjectileProtectionIV@gmail.com", "H@rryP0tt3r");
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);*/

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("ProjectileProtectionIV@gmail.com", "H@rryP0tt3r");
                    smtp.EnableSsl = true;
                    smtp.Send(mm);
                }
            }
        }

        //Set up with database to validate username and password. Look up credentials in database
        [HttpPost("login")]
        public async Task<IActionResult> Validate(string username, string password, string returnUrl, string usertype)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (usertype == "student")
            {
                //Runs the SQL Procedure Command
                SqlCommand cmd = new SqlCommand("loginStudent", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", username); //Checks User Input Email With Stored Email
                cmd.Parameters.AddWithValue("@Password", password); //Checks User Input Password With Stored Password

                //Opens Connection to SQL Server, Runs Command And Then Stores It As An INT To Be Evaulated
                conn.Open();

                int loginResult = Convert.ToInt32(cmd.ExecuteScalar());

                //Closes Connection To SQL
                conn.Close();

                if (loginResult == 1)
                {
                    //Claims to allow the logged in user to view certain web pages
                    var claims = new List<Claim>();
                    claims.Add(new Claim("username", username));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, "Student")); //Sets the NameIdentifier of the claim to Student
                    claims.Add(new Claim(ClaimTypes.Name, username)); //Sets the name in the claim to the logged in users email address
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    return RedirectToAction("Student");
                }
            }
            else if (usertype == "teacher")
            {
                //Runs the SQL Procedure Command
                SqlCommand cmd = new SqlCommand("loginTeacher", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", username); //Checks User Input Email With Stored Email
                cmd.Parameters.AddWithValue("@Password", password); //Checks User Input Password With Stored Password


                //Opens Connection to SQL Server, Runs Command And Then Stores It As An INT To Be Evaulated
                conn.Open();
                int loginResult = Convert.ToInt32(cmd.ExecuteScalar());
                //Closes Connection To SQL
                conn.Close();

                if (loginResult == 1)
                {
                    //Claims to allow the logged in user to view certain web pages
                    var claims = new List<Claim>();
                    claims.Add(new Claim("username", username));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, "Teacher")); //Sets the NameIdentifier of the claim to Teacher
                    claims.Add(new Claim(ClaimTypes.Name, username)); //Sets the name in the claim to the logged in users email address
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    return RedirectToAction("Teacher");
                }
            }
            else if (usertype == "admin") 
            {
                //Runs the SQL Procedure Command
                SqlCommand cmd = new SqlCommand("loginAdmin", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", username); //Checks User Input Email With Stored Email
                cmd.Parameters.AddWithValue("@Password", password); //Checks User Input Password With Stored Password

                //Opens Connection to SQL Server, Runs Command And Then Stores It As An INT To Be Evaulated
                conn.Open();

                int loginResult = Convert.ToInt32(cmd.ExecuteScalar());

                //Closes Connection To SQL
                conn.Close();

                if (loginResult == 1)
                {
                    var claims = new List<Claim>();
                    claims.Add(new Claim("username", username));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, "Admin")); //Sets the NameIdentifier of the claim to Admin
                    claims.Add(new Claim(ClaimTypes.Name, username)); //Sets the name in the claim to the logged in users email address
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    return Redirect(returnUrl);
                }
            }

            TempData["Error"] = "Error. User or Password is invalid";
            return View("login");
        }
        
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
