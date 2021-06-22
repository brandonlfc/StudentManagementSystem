using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authn.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DOB { get; set; }
        public string Ethnicity { get; set; }
        public string Class { get; set; }
        public string Timetable { get; set; }
        public string Assignments { get; set; }
        public string SubmittedAssignments { get; set; }
    }

    public class Teacher
    {
        public int Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email { get; set; }
        public string Class { get; set; }
        //public string Timetable { get; set; }
    }

    public class Assignment
    {
        public int AssignmentID { get; set; }
        public string Start_Date { get; set; }
        public string End_Date { get; set; }
        public string Details { get; set; }
        public string Class { get; set; }
    }
    /*
    public class TeacherTimetable
    {
        public int Id { get; set; }
        public string Image { get; set; }
    }*/

    //List of all student assignments
    public class StudentsAssignments
    {
        public int AssignmentID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Details { get; set; }
    }

    public class SubmittedAssignments
    {
        public int StudentID { get; set; }
        public int AssignmentID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Details { get; set; }
    }

    public class NotSubmitted
    {
        public int AssignmentID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Details { get; set; }
    }

    public class TeacherSubmitted
    {
        public int AssignmentID { get; set; }
        public int StudentID { get; set; }
        public string Details { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
    }

    public class StatsAttendance
    {
        public int Attendance { get; set; }
    }

    public class StatsAttitude
    {
        public float Attitude { get; set; }
    }

    public class StatsAchievement
    {
        public float Achievement { get; set; }
    }
}