var listStudent = document.getElementById("student_list");

var containerOne = document.getElementById("container_one");
var containerTwo = document.getElementById("container_two");

var createForm = document.getElementById("create_form"); //Form that allows creation of assignments
var listAssignment = document.getElementById("list_assignment"); //Form that lists assignments

var formTitle = document.getElementById("form_title");

var createAssignment = document.getElementById("create_assignments"); //Button to switch to create assignment view
var viewAssignment = document.getElementById("view_assignments"); //Button to switch to assignment view

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

	pdf.href = window.location.origin + "/Assignments/" + Class.trim() + "/" + AssignmentName;
}

function Cancel() {
	GradeAssignment.style.display = "none";

	AchievementContainer.style.display = "flex";
}


/*Don't allow creation of assignment if date set to past date*/
var StartValidation = document.getElementById("startValidation");
var EndValidation = document.getElementById("endValidation");

function checkDate() {

	var StartDate = document.getElementById("startDate").value;
	var EndDate = document.getElementById("endDate").value;
	var SubmitButton = document.getElementById("submitButton");

	var SelectedStartDate = new Date(StartDate);
	var SelectedEndDate = new Date(EndDate);

	var TodayDate = new Date().setHours(0, 0, 0); /*Sets the time portion to 0 to allow assignments to be created on the same day*/

	/*Disable button if start or end date is in the past, or if end date is before the start date*/
	if (SelectedStartDate < TodayDate || SelectedEndDate < TodayDate || SelectedEndDate < SelectedStartDate) {
		SubmitButton.disabled = true;
	}
	/*Enable button if start and end date is a future date, and if end date is after the start date*/
	else if (SelectedStartDate >= TodayDate && SelectedEndDate > TodayDate && SelectedEndDate >= SelectedStartDate) {
		SubmitButton.disabled = false;
	}

	/*If Start Date is after present date, hide validation text. Else if Start End is before present Date, show validation text*/
	if (SelectedStartDate > TodayDate) {
		StartValidation.classList.add("validation");
	}
	else if (SelectedStartDate < TodayDate) {
		StartValidation.classList.remove("validation");
	}

	/*If End Date is after Start Date, hide validation text. Else if End Date is before Start Date, show validation text*/
	if (SelectedEndDate >= SelectedStartDate) {
		EndValidation.classList.add("validation");
	}
	else if (SelectedEndDate < SelectedStartDate) {
		EndValidation.classList.remove("validation");
    }
}

function resetValidation() {
	StartValidation.classList.add("validation");
	EndValidation.classList.add("validation");
}