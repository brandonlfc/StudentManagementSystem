/*-----------------------------------------------------------------------------------
 * TIME TABLE
 ------------------------------------------------------------------------------------*/
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
/*====================================================================================*/


/*-----------------------------------------------------------------------------------
 * EDIT ASSIGNMENT
 ------------------------------------------------------------------------------------*/
var editContainer = document.getElementById("edit_container");

var editFormTitle = document.getElementById("form_title"); //Text of the edit assignment form

var due = document.getElementById("container_one");
var submitted = document.getElementById("container_two");

var assID = document.getElementById("edit_ass_id");
var assStart = document.getElementById("edit_ass_start");
var assEnd = document.getElementById("edit_ass_end");
var assDetails = document.getElementById("edit_ass_details");

function editAssignment(AssignmentID, StartDate, EndDate, Details) {
	editFormTitle.text = "Edit Assignment: " + AssignmentID
	editFormTitle.style.flex = "auto";
	editFormTitle.style.textAlign = "center";
	editFormTitle.style.marginLeft = "0";

	due.style.display = "none";
	submitted.style.display = "none";

	editContainer.style.display = "block";

	assID.value = AssignmentID;
	assStart.value = StartDate;
	assEnd.value = EndDate;
	assDetails.value = Details;
}
/*====================================================================================*/

/*-----------------------------------------------------------------------------------
 * CANCEL EDIT ASSIGNMENT
 ------------------------------------------------------------------------------------*/
function cancelEdit() {
	due.style.display = "block";
	submitted.style.display = "block";

	editContainer.style.display = "none";
}

/*====================================================================================*/