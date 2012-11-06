
function UpdateTotal(input, col, tbl, taskID, showAlert) {
    if (input.value.length > 0 && (isNaN(parseFloat(input.value)) || parseFloat(input.value) < 0 || parseFloat(input.value) > 24)) {

        if (showAlert) {
            alert("Please enter a valid number of hours");
            setTimeout("document.getElementById('" + input.id + "').focus();document.getElementById('" + input.id + "').select();", 0);
        }

        return;
    }

    var fix = TruncateNum(input.value);

    if (alert) {
        input.value = fix;
    }
    else if (input.value != fix) {
        return;
    }

    var prefix = GetPrefix();
    var sum = 0;
    var diff = 0;
    var totalRow = $('#totalRow_' + taskID);

    if (tbl == 'P' || tbl == 'U') {
        for (i = 0; i < 7; i++) {
            sum += GetFloat(document.getElementById(prefix + "txtHours" + tbl + "_" + taskID + "_" + i).value)
        }

        sum = RoundFloat(sum);
        diff = RoundFloat(sum - GetFloat(totalRow.html()));

        if (diff != 0) {
            var totalCol = $('#totalP_' + col);
            var totalTbl = $('#totalP_7');
            totalRow.html(sum);
            totalCol.html(RoundFloat(GetFloat(totalCol.html()) + diff));
            totalTbl.html(RoundFloat(GetFloat(totalTbl.html()) + diff));

            if (document.getElementById(prefix + 'myMode').value == 'log' && taskID != 'leave') {
                var spent = $('#spent_' + taskID);
                spent.val(RoundFloat(GetFloat(spent.val()) + diff));
                $('#spentDiv_' + taskID).html(spent.val());
            }

            var total = $('#totalA_' + col);

            if (total.size() == 1) {
                $('#total' + tbl + '2_' + col).html(totalCol.html());
                $('#total' + tbl + '2_7').html(totalTbl.html());
                total.html(RoundFloat(GetFloat(total.html()) + diff));
                $('#totalA_7').html(RoundFloat(GetFloat($('#totalA_7').html()) + diff));
            }
        }
    }
    else {
        for (i = 0; i < 7; i++) {
            sum += GetFloat(document.getElementById(prefix + "txtBarHours" + tbl + "_" + taskID + "_" + i).value)
        }

        sum = RoundFloat(sum);
        diff = RoundFloat(sum - GetFloat(totalRow.html()));

        if (diff != 0) {
            var totalCol = $('#total' + tbl + '_' + col);
            var totalTbl = $('#total' + tbl + '_7');

            totalRow.html(sum);
            totalCol.html(RoundFloat(GetFloat(totalCol.html()) + diff));
            totalTbl.html(RoundFloat(GetFloat(totalTbl.html()) + diff));
        }
    }

    if (diff != 0) {
        SetRowUpdated(taskID, prefix);
    }
}

function GetFloat(val) {
    if (val.length == 0) {
        return 0;
    }

    return parseFloat(val);
}

function RoundFloat(val) {
    return Math.round(val * Math.pow(10, 1)) / Math.pow(10, 1);
}

function TruncateNum(num) {
    num = String(num);
    var idx = num.indexOf(".")

    if (idx != -1 && num.length - idx > 2) {
        return num.substr(0, idx + 2);
    }

    return num;
}

function CompleteClick(chk, id, tbl) {
    var prefix = GetPrefix();
    var row = $('#' + prefix + 'row_' + id);
    var status = $('#' + prefix + 'cell_' + id + '_3');

    if (chk.checked) {
        var i = 6;
        var found = 0;

        while (i >= 0 && found == 0) {
            var input = $('#' + prefix + "txtHours" + tbl + "_" + id + "_" + i);

            if (input.val() != '0' && input.val() != '') {
                found = 1;
            }

            i--;
        }

        if (found == 0) {
            chk.checked = false;
            alert('No hours logged against task');
        }
        else {
            row.attr('class', 'completed');
            status.html('Completed');
        }
    }
    else {
        row.attr('class', 'open');
        status.html('Open');

        for (i = 0; i < 7; i++) {
            var input = $('#' + prefix + "txtHours" + tbl + "_" + id + "_" + i);
            input.show();

            if (i < 5) {
                input.parent().attr('class', 'inputHour');
            }
            else {
                input.parent().attr('class', 'inputHourWE');
            }
        }
    }

    SetRowUpdated(id, prefix);
}

function SetRowUpdated(id, prefix) {

    if (!prefix) {
        prefix = GetPrefix();
    }

    $('#' + prefix + 'hdnUpdate_' + id).val('y');
    $('#' + prefix + 'updateMade').val('y');
    $('#' + prefix + 'btnUndo').show();
    $('#' + prefix + 'btnSave').show();
}

function UpdateCheck(input) {
    if ($('input[id$="updateMade"]').val() == 'y') {
        if (!confirm("Changes will be lost, are you sure?")) {
            if (input) {
                if (input.id.indexOf('ddWeek') != -1) {
                    input.selectedIndex = parseInt($('input[id$="weekIndex"]').val());
                }
                else if (input.id.indexOf('ddDirectorate') != -1) {
                    input.selectedIndex = parseInt($('input[id$="dirIndex"]').val());
                }
                else if (input.id.indexOf('ddTeam') != -1) {
                    input.selectedIndex = parseInt($('input[id$="teamIndex"]').val());
                }
                else if (input.id.indexOf('ddProfile') != -1) {
                    input.selectedIndex = parseInt($('input[id$="profileIndex"]').val());
                }
            }

            return false;
        }
    }

    return true;
}

function CheckHours() {
    var hours = parseFloat($('#totalP_7').html());

    if (hours < 40) {
        var mode = $('input[id$="_myMode"]').val();
        if (!confirm('There are less than 40 total hours on the ' + mode + ',\nare you sure you want to set it ready for approval?')) {
            return false;
        }
    }

    return true;
}

function GetPrefix(replace) {
    var prefix = $('input[id$="_myMode"]').attr('id');
    prefix = prefix.substr(0, prefix.length - 6);

    if (replace) {
        while (prefix.indexOf('_') != -1) {
            prefix = prefix.replace('_', '$');
        }
    }

    return prefix;
}

function PrepareRedirect(url) {
    document.getElementById('redirectURL').value = url;
    __doPostBack('', '');
    return false;
}

function CancelEnter(e) {
    if (e.keyCode == 13) {
        return false;
    }
}

function CheckAlert() {
    var id;
    var valid = false;

    if (!CheckSingle()) return false;

    $('.rowCheck').each(function () {
        var chk = this.firstChild;

        if (chk.checked) {
            var id = chk.id.substr(chk.id.indexOf('chk_') + 4);
            var type = id.substr(0, 1);

            if (type == 'n') {
                alert("Please save the plan/log before creating an alert on a new task");
                return false;
            }
            else if (type == 'a') {
                alert("Please save the plan/log before creating an alert on a new barrier");
                return false;
            }
            else if (type == 'l') {
                alert("Alerts can not be created on Leave/Holiday");
                return false;
            }

            $('#alertLinkID').val(id);
            valid = true;
        }
    });

    if (valid) {
        DoPostBack('lnkAlert');
    }
}

function ToggleWE(input) {
    var input2;

    if (input.innerHTML == "+") {
        input.innerHTML = "-";
        $('#showWE').val('y');
        $('.planDayWE').show();
        $('.planWE').show();
        $('.actualWE').show();
        $('.hourWE').show();
        $('.barrierHourWE').show();
        $('.planHourWE').show();
        $('.compHourWE').show();
        $('.inputHourWE').show();
        $('.inputHourCompWE').show();
        $('.inputBarrierHourWE').show();
        $('.totalFooterWE').show();
        $('.totalFooterPlanWE').show();
    }
    else {
        input.innerHTML = "+";
        $('#showWE').val('n');
        $('.planDayWE').hide();
        $('.planWE').hide();
        $('.actualWE').hide();
        $('.hourWE').hide();
        $('.planHourWE').hide();
        $('.compHourWE').hide();
        $('.barrierHourWE').hide();
        $('.inputHourWE').hide();
        $('.inputHourCompWE').hide();
        $('.inputBarrierHourWE').hide();
        $('.totalFooterWE').hide();
        $('.totalFooterPlanWE').hide();
    }

    if (input.id.indexOf("lnkToggleWE2") != -1) {
        document.getElementById(input.id.substr(0, input.id.length - 1)).innerHTML = input.innerHTML;
        input2 = input;
    }
    else {
        input2 = document.getElementById(input.id + "2");

        if (input2) {
            input2.innerHTML = input.innerHTML;
            prefix = input2
        }
    }

    if (input2) {
        var prefix = input2.id.substr(0, input2.id.length - 12);
        var unplanDiv = document.getElementById(prefix + "cellUnplanDiv");

        if (input.innerHTML == '+') {
            unplanDiv.colSpan -= 4;
        }
        else {
            unplanDiv.colSpan += 4;
        }
    }

    return false;
}

function ToggleBarriers(input) {
    var link = $('#' + input.id);
    var prefix = input.id.substr(0, input.id.length - 18);
    var we1 = document.getElementById(prefix + 'hdrToggleWE');
    var we2 = document.getElementById(prefix + 'hdrToggleWE2');

    if (link.html() == "Show Barriers") {
        link.html('Hide Barriers');
        $('#showBarriers').val('y');

        if (navigator.appVersion.indexOf("MSIE 7.") == -1) {
            if (we1) {
                we1.rowSpan += $('.pBarrierRow').size();
            }

            if (we2) {
                we2.rowSpan += $('.uBarrierRow').size();
            }
        }

        $('.pBarrierRow').show();
        $('.uBarrierRow').show();

    }
    else {
        link.html('Show Barriers');
        $('#showBarriers').val('n');

        if (navigator.appVersion.indexOf("MSIE 7.") == -1) {
            if (we1) {
                we1.rowSpan -= $('.pBarrierRow').size();
            }

            if (we2) {
                we2.rowSpan -= $('.uBarrierRow').size();
            }
        }

        $('.pBarrierRow').hide();
        $('.uBarrierRow').hide();

        $('.rowCheck').each(function () {
            var chk = this.firstChild;

            if (chk.checked) {
                var type = chk.id.substr(chk.id.indexOf('chk_') + 4, 1);

                if (type == 'b' || type == 'a') {
                    chk.checked = false;
                    RowChecked(chk, 'b');
                }
            }
        });

        if (navigator.appVersion.indexOf("MSIE ") == -1) {
            var input2 = document.getElementById(prefix + 'lnkToggleWE');
            ToggleWE(input2);
            ToggleWE(input2);
        }
    }

    return false;
}

function RowChecked(input, type) {

    var change = 1;
    if (!input.checked) change = -1;

    if (type == 'p') {
        document.getElementById('selTaskPlan').value =
                    parseInt(document.getElementById('selTaskPlan').value) + change;
    }
    else if (type == 'u') {
        document.getElementById('selTaskUnplan').value =
                    parseInt(document.getElementById('selTaskUnplan').value) + change;
    }
    else if (type == 'b') {
        document.getElementById('selBarrier').value =
                    parseInt(document.getElementById('selBarrier').value) + change;
    }
}

function GetRowsChecked() {
    return parseInt(document.getElementById('selTaskPlan').value) +
                parseInt(document.getElementById('selTaskUnplan').value) +
                parseInt(document.getElementById('selBarrier').value);
}

function CheckSingle() {
    var checked = GetRowsChecked();

    if (checked == 0) {
        alert('No rows selected');
        return false;
    }
    else if (checked > 1) {
        alert('Multiple rows selected, please select a single row');
        return false;
    }

    return true;
}

function CheckTasksSelected(ctrl) {
    var valid = true;
    var count = parseInt(document.getElementById('selTaskPlan').value) +
                parseInt(document.getElementById('selTaskUnplan').value);

    if (count == 0) {
        alert('No tasks selected');
        return false;
    }
    else if (count == 1 || ctrl == 'lnkCarry') {
        $('.rowCheck').each(function () {
            var chk = this.firstChild;

            if (chk.checked) {
                var idx = chk.id.indexOf('chk_');
                var type = chk.id.substr(idx + 4, 1);

                if (type == 'l') {
                    if (count == 1) {
                        alert('No tasks selected');
                        valid = false;
                        return false;
                    }
                }
                else if (type == 'n') {
                    if (ctrl == 'lnkCarry') {
                        alert('Please save the plan/log before carrying the selected task(s) forward');
                        valid = false;
                        return false;
                    }
                    else if (chk.id.substr(idx + 6, 1) == '-') {
                        alert('Please save the plan/log before adding the selected task to favorites');
                        valid = false;
                        return false;
                    }
                }
                else if (ctrl == 'lnkCarry' && type != 'a' && type != 'b') {
                    var compID = chk.id.substr(0, idx) + 'cell' + chk.id.substr(idx + 3) + '_3';
                    var state = $('#' + compID).html()

                    if (state == 'Completed') {
                        alert('Can not carry forward completed tasks');
                        valid = false;
                        return false;
                    }
                    else if (state == 'OBE') {
                        alert('Can not carry forware OBE tasks');
                        valid = false;
                        return false;
                    }
                }
            }
        });
    }

    if (valid) {
        DoPostBack(ctrl);
    }
}

function EditClicked() {
    
    if (!CheckSingle()) return false;

    var valid = true;

    $('.rowCheck').each(function () {
        var chk = this.firstChild;

        if (chk.checked) {
            if (chk.id.substr(chk.id.indexOf('chk_') + 4, 1) == 'l') {
                alert("Leave/Holiday can not be edited");
                valid = false;
                return false;
            }
        }
    });

    if (valid) {
        DoPostBack('lnkEdit');
    }
}

function RemoveClicked() {
    if (GetRowsChecked() == 0) {
        alert('No rows selected');
        return false;
    }
    else if ($('input[id$="_myMode"]').val() == 'log' &&
                document.getElementById('selTaskPlan').value != '0') {
        alert('Planned tasks can not be removed from the log');
        return false;
    }

    DoPostBack('lnkRemove');
}

function AddBarrierClicked() {
    var checked = parseInt(document.getElementById('selTaskPlan').value) +
                parseInt(document.getElementById('selTaskUnplan').value);

    if (checked == 0) {
        alert('No task selected');
        return false;
    }
    else if (checked > 1) {
        alert('Multiple tasks selected, please select a single task');
        return false;
    }

    var valid = true;

    $('.rowCheck').each(function () {
        var chk = this.firstChild;

        if (chk.checked) {
            if (chk.id.substr(chk.id.indexOf('chk_') + 4, 1) == 'l') {
                alert("Can not add a barrier to Leave/Holiday");
                valid = false;
                return false;
            }
        }
    });

    if (valid) {
        DoPostBack('lnkAddBarrier');
    }
}

function LoadAddTask(id) {
    var prefix = GetPrefix();
    var re = $('#' + prefix + 'addTaskRE');
    var exit = $('#' + prefix + 'addTaskExit');
    var compTree = document.getElementById('complexityTree');

    $('#addTaskProgram').html(eval('taskProgram_' + id));
    $('#addTaskWBS').html(eval('taskWBS_' + id));
    $('#addTaskAllocated').html(eval('taskAllocated_' + id));
    $('#addTaskStart').html(eval('taskStart_' + id));
    $('#addTaskDue').html(eval('taskDue_' + id));
    $('#addTaskStatus').html(eval('taskStatus_' + id));
    $('#addTaskOComments').html(eval('taskOComments_' + id));
    $('#' + prefix + 'addTaskAComments').val(eval('taskAComments_' + id));

    if (eval('taskInst_' + id) == "False") {
        $('#divAddTaskType').hide();
        $('#divAddTaskExit').hide();
        $('#divAddTaskRE').hide();
        $('#trAddTaskComp').hide();

        exit.show();
        $('#' + prefix + 'divAddTaskTypeTree').show();

        var ec = eval('taskExit_' + id);
        
        while (ec.indexOf("<br>") != -1) {
            ec = ec.replace("<br>", "\r\n");
        }

        exit.val(ec);

        var comp = null;

        if (compTree && compTree.value != '') {
            $('#trAddTaskRE').hide();
            comp = eval('taskComp_' + id)
        }
        else {
            re.show();
            re.val(eval('taskRE_' + id));
        }

        SelectNode("addTaskType", eval('taskType_' + id), false, true, comp);
    }
    else {
        $('#divAddTaskType').show();
        $('#divAddTaskExit').show();
        $('#trAddTaskRE').show();
        $('#divAddTaskRE').show();

        re.hide();
        exit.hide();
        $('#' + prefix + 'divAddTaskTypeTree').hide();

        $('#divAddTaskType').html(eval('taskTypeTitle_' + id));
        $('#divAddTaskRE').html(eval('taskRE_' + id));
        $('#divAddTaskExit').html(eval('taskExit_' + id));

        if (compTree && compTree.value != '') {
            $('#trAddTaskComp').show();
            $('#divAddTaskComp').html(eval('taskComp_' + id));
        }
        else {
            $('#trAddTaskComp').hide();
        }
    }
}

function ValidateAddTask() {
    var prefix = GetPrefix();
    var task = document.getElementById(prefix + "addTaskSelector");

    if (task.selectedIndex == -1) {
        alert("Please select a task");
        task.focus();
        return false;
    }

    if (eval('taskInst_' + task[task.selectedIndex].value) == "False") {

        var compTree = document.getElementById('complexityTree');

        if (!IsTreeSelected("addTaskType")) {
            alert("Please select a task type");
            return false;
        }
        else if (!compTree || compTree.value == '') {
            var re = document.getElementById(prefix + "addTaskRE");

            if (re.value.length == 0 || isNaN(parseFloat(re.value)) || parseFloat(re.value) <= 0) {
                alert("Please enter in a valid RE");
                re.focus();
                return false;
            }
        }
        else if (!TreeValidateComplexity()) {
            return false;
        }
    }

    if (document.getElementById(prefix + "myMode").value == "log" &&
                       !IsTreeSelected("addUnplanCode")) {
        alert("Please select an unplanned code");
        return false;
    }
}

function CheckPrevTasks() {
    var prevTasks = document.getElementById(GetPrefix() + 'lbPrevTasks');

    if (prevTasks.selectedIndex == -1) {
        alert('No tasks selected');
        return false;
    }
}

function LoadFavorite(id) {
    var prefix = GetPrefix();
    var re = $('#' + prefix + 'addTaskRE');
    var compTree = document.getElementById('complexityTree');

    $('#favAllocated').html(eval('favAllocated_' + id));
    $('#' + prefix + 'ddProgram').val(eval('favProgram_' + id));

    var comp = null;

    if (compTree && compTree.value != '') {
        $('#trAddTaskRE').hide();
        comp = eval('favComp_' + id)
    }
    else {
        re.val(eval('favRE_' + id));
    }

    SelectNode("addTaskType", eval('favType_' + id), false, true, comp);
}

function ValidateFavorite() {
    var prefix = GetPrefix();
    var fav = document.getElementById(prefix + "lbFavs");

    if (fav.selectedIndex == -1) {
        alert("Please select a favorite");
        fav.focus();
        return false;
    }

    var program = document.getElementById(prefix + "ddProgram");

    if (program.selectedIndex < 1) {
        alert("Please select a program");
        program.focus();
        return false;
    }

    var compTree = document.getElementById('complexityTree');

    if (!IsTreeSelected("addTaskType")) {
        alert("Please select a task type");
        return false;
    }
    else if (!compTree || compTree.value == '') {
        var re = document.getElementById(prefix + "addTaskRE");

        if (re.value.length == 0 || isNaN(parseFloat(re.value)) || parseFloat(re.value) <= 0) {
            alert("Please enter in a valid RE");
            re.focus();
            return false;
        }
    }
    else if (!TreeValidateComplexity()) {
        return false;
    }

    if (document.getElementById(prefix + "myMode").value == "log" &&
                       !IsTreeSelected("addUnplanCode")) {
        alert("Please select an unplanned code");
        return false;
    }
}

function ValidateEditTask() {

    var program = document.getElementById(GetPrefix() + "ddProgram");

    if (program && program.selectedIndex < 1) {
        alert("Please select a program");
        program.focus();
        return false;
    }

    var compTree = document.getElementById('complexityTree');

    if (!compTree || compTree.value == '') {
        var re = document.getElementById(GetPrefix() + "addTaskRE");

        if (re && re.value.length == 0 || isNaN(parseFloat(re.value)) || parseFloat(re.value) <= 0) {
            alert("Please enter in a valid RE");
            re.focus();
            return false;
        }
    }
    else if (!TreeValidateComplexity()) {
        return false;
    }
}

function ValidateBarrier() {
    if (!IsTreeSelected("barCode")) {
        alert("Please select a barrier code");
        return false;
    }

    var comment = document.getElementById(GetPrefix() + "barComment");

    if (comment.value.length == 0) {
        alert('Please enter a comment');
        comment.focus();
        return false;
    }
}

function ValidateAlert() {
    var prefix = GetPrefix();
    var sub = document.getElementById(prefix + 'alertSubject');

    if (sub.value == '') {
        alert("Please enter a subject");
        sub.focus();
        return false;
    }

    var alm = document.getElementById(prefix + 'alertALM');

    if (alm && !alm.checked) {

        var mgr = document.getElementById(prefix + 'alertMgr');
        var owner = document.getElementById(prefix + 'alertOwner');

        if ((mgr || owner) && (!mgr || !mgr.checked) && (!owner || !owner.checked)) {
            alert("Please select someone to notify");
            return false;
        }
    }
}

function InitMenus() {
    var menu = $('.addMenu');

    for (var i = 0; i < 2; i++) {
        if (menu.size() > 0) {
            for (var j = 1; j < menu.size(); j++) {
                $(menu[j]).remove();
            }

            menu.first().dialog({
                autoOpen: false,
                resizable: false,
                width: 150,
                minHeight: 16,
                create: function (event, ui) {
                    $('.ui-widget-header').hide();
                    $('.ui-dialog').css('padding', '0px');
                    $('.ui-dialog').css('border-color', 'Black');
                    $('.ui-dialog-titlebar').removeClass('ui-corner-all');
                    $('.ui-dialog').removeClass('ui-corner-all');
                },
                open: function (event, ui) {
                    var cell = $('.' + this.id + 'Cell').position();
                    $(event.target).parent().css('position', 'absolute');
                    if (cell.left > 30 || navigator.appVersion.indexOf("MSIE") != -1) {
                        $(event.target).parent().css('left', cell.left + 'px');
                    }
                    else {
                        $(event.target).parent().css('left', (cell.left - 1) + 'px');
                    }
                    $(event.target).parent().css('top', (cell.top + 24) + 'px');
                }
            });
        }

        menu = $('.actionMenu');
    }
}

function CloseAddMenu(force) {
    var cell = $('.addMenuCell');
    var menu = $('#addMenu');

    if (force) {
        menu.dialog('close');
        cell.attr('bgcolor', '#666666');
    }
    else if (document.all) {
        var x = event.clientX + document.documentElement.scrollLeft;
        var y = event.clientY + document.documentElement.scrollTop;
        setTimeout(function () { CloseMenu(cell, menu, x, y); }, 200);
    }
    else {
        setTimeout(function () { CloseMenu(cell, menu); }, 300);
    }
}

function CloseActionMenu(force) {
    var cell = $('.actionMenuCell');
    var menu = $('#actionMenu');

    if (force) {
        menu.dialog('close');
        cell.attr('bgcolor', '#666666');
    }
    else if (document.all) {
        var x = event.clientX + document.documentElement.scrollLeft;
        var y = event.clientY + document.documentElement.scrollTop;
        setTimeout(function () { CloseMenu(cell, menu, x, y); }, 200);
    }
    else {
        setTimeout(function () { CloseMenu(cell, menu); }, 300);
    }
}

function CloseMenu(cell, menu, x, y) {
    var cellLeft = cell.position().left;
    var cellTop = cell.position().top;
    var menuRight = cellLeft + menu.width();

    if (!x) {
        x = parseInt(document.getElementById('xpos').value);
        y = parseInt(document.getElementById('ypos').value);
        cellLeft += 2;
        cellTop += 2;
        menuRight--;
    }
    else if (navigator.appVersion.indexOf("MSIE 7.") != -1) {
        cellLeft += 2;
        cellTop += 2;
        menuRight--;
    }

    var cellBottom = cellTop + cell.height() + 3;

    if (!(x >= cellLeft && x < cellLeft + cell.width() && y >= cellTop && y < cellBottom) &&
        !(x >= cellLeft + 1 && x < menuRight &&
          y >= cellBottom && y < cellBottom + menu.height())) {

        menu.dialog('close');
        cell.attr('bgcolor', '#666666');
    }
}

function MenuItemOn(cell) {
    cell.bgColor = '#DDDDDD';
    cell.style.cursor = 'pointer';
}

function MenuItemOff(cell) {
    cell.bgColor = '#FFFFFF';
}

function DoPostBack(ctrl) {
    __doPostBack(GetPrefix(1) + ctrl, '')
}

$(function () {
    $('.datepicker').datepicker({
        buttonImage: '/images/calendar_icon.jpg',
        buttonImageOnly: true,
        showOn: "both",
        beforeShow: function (input, inst) {
            if (document.getElementById('minDate') != null) {
                $('#' + this.id).datepicker('option', 'minDate', -14);
            }
        }
    });

    InitMenus();

    var planned = 0;
    var unplanned = 0;
    var barrier = 0;

    $('.rowCheck').each(function () {
        var chk = this.firstChild;

        if (chk.checked) {
            var type = chk.id.substr(chk.id.indexOf('chk_') + 4, 1);

            if (type == 't' || type == 'n' || type == 'l') {
                if (chk.onclick.toString().indexOf(", 'p'") != -1) {
                    planned++;
                }
                else {
                    unplanned++;
                }
            }
            else if (type == 'b' || type == 'a') {
                barrier++;
            }
        }
    });

    document.getElementById('selTaskPlan').value = planned;
    document.getElementById('selTaskUnplan').value = unplanned;
    document.getElementById('selBarrier').value = barrier;

    if (!document.all) {
        document.onmousemove = setMousePosition;
    }
});

function setMousePosition(e) {
    document.getElementById('xpos').value = e.pageX;
    document.getElementById('ypos').value = e.pageY;
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(PopupForm);

function PopupForm() {
    $(function () {
        InitMenus();
        var popup = $('#popupForm');

        if (popup.size() > 0) {
            var dialog;

            if (popup.val() == 'popupTask') {
                dialog = $('.popupTask');

                dialog.first().dialog({
                    width: 600,
                    modal: true,
                    open: function (type, data) { $(this).parent().appendTo('form'); }
                });

                var loadCheck = document.getElementById('editTaskType');

                if (loadCheck) {
                    var compCheck = document.getElementById('editTaskComp');

                    if (compCheck) {
                        compCheck = compCheck.value;
                        $('#trAddTaskRE').hide();
                    }

                    SelectNode('addTaskType', loadCheck.value, false, true, compCheck);
                }

                loadCheck = document.getElementById('editUnplanCode');

                if (loadCheck) {
                    SelectNode('addUnplanCode', loadCheck.value, false, true, null);
                }
            }
            else if (popup.val() == 'popupPrevTasks') {
                dialog = $('.popupPrevTasks');

                dialog.first().dialog({
                    width: 530,
                    modal: true,
                    open: function (type, data) { $(this).parent().appendTo('form'); }
                });
            }
            else if (popup.val() == 'popupBarrier') {
                dialog = $('.popupBarrier');

                dialog.first().dialog({
                    width: 600,
                    modal: true,
                    open: function (type, data) { $(this).parent().appendTo('form'); }
                });

                var barID = document.getElementById('editBarrier');

                if (barID) {
                    SelectNode('barCode', barID.value, false, true, null);
                }
            }
            else if (popup.val() == 'popupAlert') {
                dialog = $('.popupAlert');

                dialog.first().dialog({
                    width: 550,
                    resizable: false,
                    modal: true,
                    open: function (type, data) { $(this).parent().appendTo('form'); }
                });
            }
            else if (popup.val() == 'popupCarry') {
                dialog = $('.popupCarry');

                dialog.first().dialog({
                    width: 250,
                    minHeight: 135,
                    resizable: false,
                    modal: true,
                    open: function (type, data) { $(this).parent().appendTo('form'); }
                });
            }

            for (var i = 1; i < dialog.size(); i++) {
                $(dialog[i]).remove();
            }
        }
    });
}
