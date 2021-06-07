var listStudent = document.getElementById("student_list");

var containerOne = document.getElementById("container_one");
var containerTwo = document.getElementById("container_two");

var createForm = document.getElementById("create_form"); //Form that allows creation of assignments
var listAssignment = document.getElementById("list_assignment"); //Form that lists assignments

var formTitle = document.getElementById("form_title");

var createAssignment = document.getElementById("create_assignments"); //Button to switch to create assignment view
var viewAssignment = document.getElementById("view_assignments"); //Button to switch to assignment view

function regStudents() {
	containerOne.classList.remove("teacher");
	containerOne.classList.add("student");

	registeredTitle.text = "Registered Students";

	listStudent.style.display = "block";
	listTeacher.style.display = "none";

	selectStudent.style.fontWeight = "bold";
	selectTeacher.style.fontWeight = "normal";
}

/*View or Create Assignments*/
function createAssignments() {
	listAssignment.style.display = "none"
	createForm.style.display = "block"

	containerTwo.classList.remove("view_assignments");
	containerTwo.classList.add("create_assignments");

	formTitle.text = "Create Assignment";

	createAssignment.style.fontWeight = "bold";
	viewAssignment.style.fontWeight = "normal";
}

function viewAssignments() {
	createForm.style.display = "none"
	listAssignment.style.display = "block"

	containerTwo.classList.remove("create_assignments");
	containerTwo.classList.add("view_assignments");

	formTitle.text = "View Assignment";

	viewAssignment.style.fontWeight = "bold";
	createAssignment.style.fontWeight = "normal";
}




var timetableDisplay = document.getElementById("timetable_display");

function showTimetable() {
	timetableDisplay.style.display = "block";
}

function hideTimetable() {
	timetableDisplay.style.display = "none";
}

window.onclick = function (event) {
	if (event.target == timetableDisplay) {
		timetableDisplay.style.display = "none";
    }
}


var MainContainer = document.getElementById("main-container");
var AttendanceContainer = document.getElementById("attendance-container");
var AttitudeContainer = document.getElementById("attitude-container");
var AchievementContainer = document.getElementById("achievement-container");

var Teacher = document.getElementById("teacher");
var Attendance = document.getElementById("attendance");
var Attitude = document.getElementById("attitude");
var Achievement = document.getElementById("achievement");

function showTeacher() {
	Attendance.classList.remove("active");
	Attitude.classList.remove("active");
	Achievement.classList.remove("active");

	Teacher.classList.add("active");

	GradeAssignment.style.display = "none";
	AttendanceContainer.style.display = "none";
	AttitudeContainer.style.display = "none";
	AchievementContainer.style.display = "none";
	MainContainer.style.display = "flex";
}

function showAttendance() {
	Teacher.classList.remove("active");
	Attitude.classList.remove("active");
	Achievement.classList.remove("active");

	Attendance.classList.add("active");

	GradeAssignment.style.display = "none";
	AchievementContainer.style.display = "none";
	MainContainer.style.display = "none";
	AttitudeContainer.style.display = "none";
	AttendanceContainer.style.display = "flex";
}

function showAttitude() {
	Teacher.classList.remove("active");
	Attendance.classList.remove("active");
	Achievement.classList.remove("active");

	Attitude.classList.add("active");

	GradeAssignment.style.display = "none";
	AchievementContainer.style.display = "none";
	MainContainer.style.display = "none";
	AttendanceContainer.style.display = "none";
	AttitudeContainer.style.display = "flex";
}

function showAchievement() {
	Teacher.classList.remove("active");
	Attendance.classList.remove("active");
	Attitude.classList.remove("active");

	Achievement.classList.add("active");

	GradeAssignment.style.display = "none";
	MainContainer.style.display = "none";
	AttendanceContainer.style.display = "none";
	AttitudeContainer.style.display = "none";
	AchievementContainer.style.display = "flex";
}



//Gets the current date to put into the attendance date picker
function getDate() {
	var today = new Date();

	var dd = today.getDate();
	var mm = today.getMonth();
	var yyyy = today.getFullYear();

	if (dd < 10) {
		dd = '0' + dd;
	}
	if (mm < 10) {
		mm = '0' + mm;
	}

	today = yyyy + '-' + mm + '-' + dd;

	document.getElementById("attendance_date").value = today;
	document.getElementById("attitude_date").value = today;
}

//Run functions on page load
window.onload = function () {
	getDate(); //Sets the date for attendance
}


var GradeAssignment = document.getElementById("grade-container");
var pdfDocument = document.getElementById("pdf");

function gradeAssignment(StudentID, AssignmentID, AssignmentName, Details, Class) {
	MainContainer.style.display = "none";
	AttendanceContainer.style.display = "none";
	AttitudeContainer.style.display = "none";
	AchievementContainer.style.display = "none";

	GradeAssignment.style.display = "flex";

	document.getElementById("grade-studid").value = StudentID;
	document.getElementById("grade-assid").value = AssignmentID;
	document.getElementById("grade-details").value = Details;

	pdf.href = window.location.origin + "/assignments/" + Class.trim() + "/" + AssignmentName;
}

function Cancel() {
	GradeAssignment.style.display = "none";

	AchievementContainer.style.display = "flex";
}