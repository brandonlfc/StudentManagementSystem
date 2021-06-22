var listStudent = document.getElementById("student_list"); //Gets the registered students list
var listTeacher = document.getElementById("teacher_list"); //Gets the registered teachers list

var containerOne = document.getElementById("container_one"); //Container to display the registered users
var selectStudent = document.getElementById("created_students"); //Button to swap to the registered students list
var selectTeacher = document.getElementById("created_teachers"); //Button to swap to the registered teachers list
var registeredTitle = document.getElementById("registered_title"); //Title of the registered users container

var formStudent = document.getElementById("student_form"); //Form to create a student
var formTeacher = document.getElementById("teacher_form"); //Form to create a teacher

var containerTwo = document.getElementById("container_two"); //Container to display the create user forms
var registerStudent = document.getElementById("reg_student"); //Button to swap to the create student form
var registerTeacher = document.getElementById("reg_teacher"); //Button to swap to the create teacher form
var createTitle = document.getElementById("form_title");

//Displays the registed students
function regStudents() {
	containerOne.classList.remove("teacher"); //Removes the box-shadow for teachers
	containerOne.classList.add("student"); //Adds the box-shadow for students

	registeredTitle.text = "Registered Students"; //Sets the registered users title

	listStudent.style.display = "block"; //Displays the list of registered students
	listTeacher.style.display = "none"; //Hides the list of registered teachers

	selectStudent.style.fontWeight = "bold"; //Sets the student button text to bold
	selectTeacher.style.fontWeight = "normal"; //Sets the teacher button text to normal
}

//Displays the registered teachers
function regTeachers() {
	containerOne.classList.add("teacher"); //Adds the box-shadow for teachers
	containerOne.classList.remove("student"); //Removes the box-shadow for students

	registeredTitle.text = "Registered Teachers";

	listStudent.style.display = "none";
	listTeacher.style.display = "block";

	selectStudent.style.fontWeight = "normal";
	selectTeacher.style.fontWeight = "bold";
}

function createStudent() {
	containerTwo.classList.remove("teacher");
	containerTwo.classList.add("student");

	createTitle.text = "Create Student";

	formStudent.style.display = "block";
	formTeacher.style.display = "none";

	registerStudent.style.fontWeight = "bold";
	registerTeacher.style.fontWeight = "normal";
}

function createTeacher() {
	containerTwo.classList.add("teacher");
	containerTwo.classList.remove("student");

	createTitle.text = "Create Teacher";

	formStudent.style.display = "none";
	formTeacher.style.display = "block";

	registerStudent.style.fontWeight = "normal";
	registerTeacher.style.fontWeight = "bold";
}




//RETRIEVE STUDENT INFORMATION
var editStudentForm = document.getElementById("student_form_edit");

var editStudID = document.getElementById("edit_stud_id");
var editStudFirstName = document.getElementById("edit_stud_first_name");
var editStudLastName = document.getElementById("edit_stud_last_name");
var editStudEmail = document.getElementById("edit_stud_email");
var editStudClass = document.getElementById("edit_stud_class");
var editStudPhone = document.getElementById("edit_stud_phone");
var editStudDOB = document.getElementById("edit_stud_dob");
var editStudEthnicity = document.getElementById("edit_stud_ethnicity");
var editStudTimetable = document.getElementById("uploaded_stud_timetable");

//RETRIEVE TEACHER INFORMATION
var editTeacherForm = document.getElementById("teacher_form_edit");

var editTeachID = document.getElementById("edit_teach_id");
var editTeachFirstName = document.getElementById("edit_teach_first_name");
var editTeachLastName = document.getElementById("edit_teach_last_name");
var editTeachEmail = document.getElementById("edit_teach_email");
var editTeachClass = document.getElementById("edit_teach_class");
var editTeachTimetable = document.getElementById("uploaded_teach_timetable");



/*EDIT STUDENT DETAILS*/
function editStudent(ID, FirstName, LastName, Email, Class, Phone, DOB, Ethnicity, Timetable) {
	containerTwo.classList.remove("teacher");
	containerTwo.classList.remove("student");
	containerTwo.classList.add("editUser");

	formStudent.style.display = "none";
	formTeacher.style.display = "none";

	registerStudent.style.display = "none";
	registerTeacher.style.display = "none";
	editTeacherForm.style.display = "none";

	createTitle.text = "Edit Student, ID: " + ID;

	editStudentForm.style.display = "block";

	var splitDOB = DOB.split(' ')[0]; /*Removes time after the date*/
	var subDOB = splitDOB.substring(0, 2); /*gets first two characters from the date*/

	//Checks the date. Reverses the date to display in the date box
	if (subDOB.includes('/')) //If one of the first two characters contains a /
	{
		var a = subDOB.substring(0, 1) //Gets first character from string
		a = "0" + a; //adds 0 before string
		var aDOB = a + splitDOB.substring(1, 9); //adds the fixed string to the rest of the date
		var fixedDOB = aDOB.split('/').reverse().join('-'); //reverses date to yyyy/mm/ddd
	} else {
		var fixedDOB = splitDOB.split('/').reverse().join('-');
	}

	//INPUT VALUES INTO FORM TO BE EDITED
	editStudID.value = ID;
	editStudFirstName.value = FirstName;
	editStudLastName.value = LastName;
	editStudEmail.value = Email;
	editStudClass.value = Class;
	editStudPhone.value = Phone;
	editStudDOB.value = fixedDOB;
	editStudEthnicity.value = Ethnicity;
	editStudTimetable.value = Timetable;
}

/*EDIT TEACHER DETAILS*/
function editTeacher(ID, FirstName, LastName, Email, Class, Timetable) {
	//Removes the register teacher and student box shadow. Adds the edit box shadow
	containerTwo.classList.remove("teacher");
	containerTwo.classList.remove("student");
	containerTwo.classList.add("editUser");

	formStudent.style.display = "none"; //Hides the register student form
	formTeacher.style.display = "none"; //Hides the register teacher form
	editStudentForm.style.display = "none"; //Hides the edit student form

	//Hides the student and techer register forms
	registerStudent.style.display = "none";
	registerTeacher.style.display = "none";

	createTitle.text = "Edit Teacher, ID: " + ID; //Sets the text of the edit form

	editTeacherForm.style.display = "block"; //Displays the edit form

	//INPUT VALUES INPUT FORM TO BE EDITED
	editTeachID.value = ID;
	editTeachFirstName.value = FirstName;
	editTeachLastName.value = LastName;
	editTeachEmail.value = Email;
	editTeachClass.value = Class;
	editTeachTimetable.value = Timetable;
}

function cancelEdit() {
	containerTwo.classList.remove("editUser");
	containerTwo.classList.add("student");

	//Hide edit form of teacher and student
	editStudentForm.style.display = "none";
	editTeacherForm.style.display = "none";

	//Display register student form
	formStudent.style.display = "block";

	/*Tabs at the top to swap between registering student or teacher*/
	registerStudent.style.display = "block";
	registerTeacher.style.display = "block";

	registerStudent.style.fontWeight = "bold"; //Sets font of create student to bold
	registerTeacher.style.fontWeight = "normal"; //Sets font of create teacher to normal
	createTitle.text = "Create Student"; //Sets the text of the register form
}