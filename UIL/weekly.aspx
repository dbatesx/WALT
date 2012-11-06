<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="weekly.aspx.cs" Inherits="WALT.UIL.Weekly" MasterPageFile="Site1.Master" EnableEventValidation="false" %>
<%@ Register TagPrefix="cc" TagName="TreeSelector" Src="~/Controls/TreeSelector.ascx" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <script src="/js/weekly.js" type="text/javascript"></script>
    <asp:HiddenField ID="weeklyPlanID" runat="server" />
    <asp:HiddenField ID="weekIndex" runat="server" />
    <asp:HiddenField ID="dirIndex" runat="server" />
    <asp:HiddenField ID="teamIndex" runat="server" />
    <asp:HiddenField ID="profileIndex" runat="server" />
    <asp:HiddenField ID="updateMade" runat="server" />
    <asp:HiddenField ID="sessionChanges" runat="server" />
    <input type="hidden" id="selTaskPlan" name="selTaskPlan" />
    <input type="hidden" id="selTaskUnplan" name="selTaskUnplan" />
    <input type="hidden" id="selBarrier" name="selBarrier" />
    <input type="hidden" id="redirectURL" name="redirectURL" />
    <input type="hidden" id="clearSession" name="clearSession" />
    <input type="hidden" id="alertLinkID" name="alertLinkID" />
    <input type="hidden" id="showBarriers" name="showBarriers" />
    <input type="hidden" id="showWE" name="showWE" />
    <input type="hidden" id="xpos" />
    <input type="hidden" id="ypos" />

    <asp:PlaceHolder ID="phHeader" runat="server" />

    <asp:Table class="weeklyOuterTbl" CellSpacing="0" runat="server">
     <asp:TableRow>
      <asp:TableCell ColumnSpan="2" style="border-bottom: 1px solid Black; padding: 0px 0px 3px 0px">
       <table>
        <tr>
         <td width="250" style="font-size: 16pt; white-space: nowrap" rowspan="2"><b><asp:Label ID="lblPlanTable" runat="server" /></b></td>
         <td style="padding: 0px 0px 0px 6px"><b>Directorate:</b></td>
         <td style="padding: 0px 0px 0px 6px" colspan="2"><b>Team:</b></td>
         <td style="padding: 0px 0px 0px 6px" colspan="3"><b>Member:</b></td>
         <td style="padding: 0px 0px 0px 6px" colspan="3"><b>Week:</b></td>
        </tr>
        <tr>
         <td style="padding: 1px 6px 1px 6px">
          <asp:DropDownList ID="ddDirectorate" onchange="if(!UpdateCheck(this))return false"
            OnSelectedIndexChanged="ddDirectorate_SelectedIndexChanged" AutoPostBack="true" runat="server"></asp:DropDownList>
         </td>
         <td style="padding: 1px 6px 1px 6px">
          <asp:DropDownList ID="ddTeam" onchange="if(!UpdateCheck(this))return false"
            OnSelectedIndexChanged="ddTeam_SelectedIndexChanged" AutoPostBack="true" runat="server"></asp:DropDownList>
         </td>
         <td style="padding: 1px 1px 1px 6px">
          <asp:ImageButton ID="lnkPrevProfile" CssClass="lnkPrevProfile" ImageUrl="/images/pointer-left.png"
            OnClientClick="return UpdateCheck()" OnClick="lnkChangeProfile_Click" runat="server" />
         </td>
         <td style="padding: 1px 0px 1px 1px">
          <asp:DropDownList ID="ddProfile" onchange="if(!UpdateCheck(this))return false"
            OnSelectedIndexChanged="ddProfile_SelectedIndexChanged" AutoPostBack="true" runat="server"></asp:DropDownList>
         </td>
         <td style="padding: 1px 6px 1px 0px">
          <asp:ImageButton ID="lnkNextProfile" ImageUrl="/images/pointer-right.png"
            OnClientClick="return UpdateCheck()" OnClick="lnkChangeProfile_Click" runat="server" />
         </td>
         <td style="padding: 1px 1px 1px 6px">
          <asp:ImageButton ID="lnkPrevWeek" ImageUrl="/images/pointer-left.png"
            OnClientClick="return UpdateCheck()" OnClick="lnkPrevWeek_Click" runat="server" />
         </td>
         <td style="padding: 1px 3px 1px 1px">
          <asp:DropDownList ID="ddWeek" onchange="if(!UpdateCheck(this))return false"
            OnSelectedIndexChanged="ddWeek_SelectedIndexChanged" AutoPostBack="true" runat="server" />
         </td>
         <td>
          <asp:TextBox ID="txtWeek" class="datepicker" onchange="if(!UpdateCheck())return false"
            OnTextChanged="txtWeek_TextChanged" AutoPostBack="true" style="display: none" runat="server"></asp:TextBox>
         </td>
         <td>
          <asp:ImageButton ID="lnkNextWeek" ImageUrl="/images/pointer-right.png"
            OnClientClick="return UpdateCheck()" OnClick="lnkNextWeek_Click" runat="server" />
         </td>
        </tr>
       </table>
      </asp:TableCell>
     </asp:TableRow>
     <asp:TableRow ID="rowPlanBtns" BackColor="#666666" ForeColor="White" BorderWidth="0">
      <asp:TableCell style="padding: 0px">

       <asp:UpdatePanel ID="btnPanel" runat="server">
        <ContentTemplate>
           <asp:PlaceHolder ID="phUpdatePanel" runat="server"></asp:PlaceHolder>
           <asp:HiddenField ID="myMode" runat="server" />
           <asp:Table CellSpacing="0" CellPadding="5" runat="server">
            <asp:TableRow>

             <asp:TableCell ID="cellAddMenu" CssClass="addMenuCell" Width="40" HorizontalAlign="Center" style="cursor: default; border-left: 1px solid Black"
                onmouseover="this.bgColor='#000000';$('#addMenu').dialog('open')" onmouseout="CloseAddMenu()">
               <b>Add</b>
               <div id="addMenu" class="addMenu" style="display: none; padding: 0px" onmouseout="CloseAddMenu()">
                <asp:Table ID="tblAddMenu" runat="server" Width="150" CellSpacing="0" CellPadding="3">
                 <asp:TableRow>
                  <asp:TableCell ID="cellNewTask" Height="16" onclick="DoPostBack('lnkNewTask')" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                   New Task<asp:LinkButton id="lnkNewTask" OnClick="lnkNewTask_Click" style="display: none" runat="server" />
                  </asp:TableCell>
                 </asp:TableRow>
                 <asp:TableRow>
                  <asp:TableCell ID="cellAddTask" Height="16" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                   Existing Task<asp:LinkButton id="lnkAddTask" OnClick="lnkAddTask_Click" style="display: none" runat="server" />
                  </asp:TableCell>
                 </asp:TableRow>
                 <asp:TableRow>
                  <asp:TableCell ID="cellAddPrev" Height="16" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                   Tasks From Last Week<asp:LinkButton id="lnkAddPrev" OnClick="lnkAddPrev_Click" style="display: none" runat="server" />
                  </asp:TableCell>
                 </asp:TableRow>
                 <asp:TableRow>
                   <asp:TableCell ID="cellAddFav" Height="16" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                    From Favorites<asp:LinkButton id="lnkAddFav" OnClick="lnkAddFav_Click" style="display: none" runat="server" />
                  </asp:TableCell>
                 </asp:TableRow>
                 <asp:TableRow>
                  <asp:TableCell ID="cellAddBarrier" Height="16" onclick="AddBarrierClicked()" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                   Barrier<asp:LinkButton ID="lnkAddBarrier" OnClick="lnkAddBarrier_Click" style="display: none" runat="server" />
                  </asp:TableCell> 
                 </asp:TableRow>
                 <asp:TableRow>
                  <asp:TableCell ID="cellLeave" Height="16" onclick="DoPostBack('lnkLeave')" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                   Leave/Holiday<asp:LinkButton ID="lnkLeave" OnClick="lnkLeave_Click" style="display: none" runat="server" />
                  </asp:TableCell>
                 </asp:TableRow>
                </asp:Table>
               </div>
             </asp:TableCell>
             
             <asp:TableCell ID="cellEdit" Width="40" HorizontalAlign="Center" style="cursor: pointer"
                onclick="EditClicked()" onmouseover="this.bgColor='#000000'" onmouseout="this.bgColor='#666666'">
              <b>Edit</b>
              <asp:LinkButton ID="lnkEdit" OnClick="lnkEdit_Click" style="display: none" runat="server" />
             </asp:TableCell>
             <asp:TableCell ID="cellRemove" Width="60" HorizontalAlign="Center" style="cursor: pointer"
                onclick="RemoveClicked()" onmouseover="this.bgColor='#000000'" onmouseout="this.bgColor='#666666'">
              <b>Remove</b><asp:LinkButton ID="lnkRemove" OnClick="lnkRemove_Click" style="display: none" runat="server" />
             </asp:TableCell>
             
             <asp:TableCell ID="cellActionMenu" CssClass="actionMenuCell" Width="55" HorizontalAlign="Center" style="cursor: default"
                onmouseover="this.bgColor='#000000';$('#actionMenu').dialog('open')" onmouseout="CloseActionMenu()">
              <b>Actions</b>
              <div id="actionMenu" class="actionMenu" style="display: none; padding: 0px" onmouseout="CloseActionMenu()">
               <asp:Table ID="tblActionMenu" runat="server" Width="150" CellSpacing="0" CellPadding="3">
                <asp:TableRow>
                 <asp:TableCell ID="cellCarry" Height="16" onclick="CheckTasksSelected('lnkCarry')" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                  Carry Tasks Forward<asp:LinkButton ID="lnkCarry" OnClick="lnkCarry_Click" style="display: none" runat="server" />
                 </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                 <asp:TableCell ID="cellAddToFavs" Height="16" onclick="CheckTasksSelected('lnkAddToFavs')" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                  Add Tasks to Favorites<asp:LinkButton id="lnkAddToFavs" OnClick="lnkAddToFavs_Click" style="display: none" runat="server" />
                 </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                 <asp:TableCell ID="cellAlert" Height="16" onclick="CheckAlert()" onmouseover="MenuItemOn(this)" onmouseout="MenuItemOff(this)">
                  Create Alert<asp:LinkButton ID="lnkAlert" OnClick="lnkAlert_Click" style="display: none" runat="server"/>
                 </asp:TableCell>
                </asp:TableRow>
               </asp:Table>
              </div>
             </asp:TableCell>
             <asp:TableCell ID="cellToggleBarriers" Width="92" HorizontalAlign="Center" style="cursor: pointer"
                onclick="ToggleBarriers(this)" onmouseover="this.bgColor='#000000'" onmouseout="this.bgColor='#666666'" Font-Bold="true">
                Hide Barriers
             </asp:TableCell>
            </asp:TableRow>
           </asp:Table>

           <asp:Panel ID="popupTask" title="Add Task" CssClass="popupTask" Visible="false" style="display: none" runat="server">
             <table cellpadding="3" cellspacing="0" class="dialogTbl" width="100%">

              <asp:PlaceHolder ID="phAddTaskSelector" runat="server">
               <tr>
                <td colspan="2">
                 <table cellpadding="3" cellspacing="0" width="100%">
                  <tr>
                   <td valign="top" width="50%">
                    <asp:ListBox ID="addTaskSelector" runat="server" Width="100%" Rows="7"
                        onchange="LoadAddTask(this[this.selectedIndex].value)"></asp:ListBox>
                   </td>
                   <td valign="top" width="50%">
                    <table cellpadding="3" cellspacing="0">
                     <tr>
                      <td><b>Program:</b></td>
                      <td><div id="addTaskProgram" /></td>
                     </tr>
                     <tr>
                      <td><b>WBS:</b></td>
                      <td><div id="addTaskWBS" /></td>
                     </tr>
                     <tr>
                      <td><b>Hours Allocated:</b></td>
                      <td><div id="addTaskAllocated" /></td>
                     </tr>
                     <tr>
                      <td><b>Requested Start Date:</b></td>
                      <td><div id="addTaskStart" /></td>
                     </tr>
                     <tr>
                      <td><b>Due Date:</b></td>
                      <td><div id="addTaskDue" /></td>
                     </tr>
                     <tr>
                      <td><b>Status:</b></td>
                      <td><div id="addTaskStatus" /></td>
                     </tr>
                    </table>
                   </td>
                  </tr>
                 </table>
                </td>
               </tr>
              </asp:PlaceHolder>

              <asp:PlaceHolder ID="phEditTaskInfo" runat="server">
               <tr>
                <td><b>Title:</b></td>
                <td>
                 <asp:Label ID="lblEditTaskTitle" runat="server" />
                 <asp:HiddenField ID="hdnEditTaskInst" runat="server" />
                 <asp:HiddenField ID="hdnEditTaskID" runat="server" />
                </td>
               </tr>
               <tr id="trProgramInfo" runat="server">
                <td><b>Program:</b></td>
                <td><asp:Label ID="lblEditTaskProgram" runat="server" /></td>
               </tr>
               <tr>
                <td><b>WBS:</b></td>
                <td><asp:Label ID="lblEditTaskWBS" runat="server" /></td>
               </tr>
               <tr>
                <td><b>Hours Allocated:</b></td>
                <td><asp:Label ID="lblEditTaskAllocated" runat="server" /></td>
               </tr>
               <tr>
                <td><b>Requested Start Date:</b></td>
                <td><asp:Label ID="lblEditTaskStart" runat="server" /></td>
               </tr>
               <tr>
                <td><b>Due Date:</b></td>
                <td><asp:Label ID="lblEditTaskDue" runat="server" /></td>
               </tr>
               <tr>
                <td><b>Status:</b></td>
                <td><asp:Label ID="lblEditTaskStatus" runat="server" /></td>
               </tr>
              </asp:PlaceHolder>

              <asp:PlaceHolder ID="phAddFav" runat="server">
               <tr>
                <td valign="top"><b>Favorite:</b></td>
                <td>
                 <asp:ListBox ID="lbFavs" runat="server" Width="370" Rows="7"
                    onchange="LoadFavorite(this[this.selectedIndex].value)"></asp:ListBox>
                </td>
               </tr>
               <tr>
                <td><b>Hours Allocated:</b></td>
                <td><div id="favAllocated" /></td>
               </tr>
              </asp:PlaceHolder>
      
              <tr id="trOwner" runat="server">
               <td valign="top" style="white-space: nowrap"><b>Owner Comments:</b></td>
               <td width="450">
                <div id="addTaskOComments" />
                <asp:Label ID="lblEditTaskOComments" runat="server" />
               </td>
              </tr>
              <tr id="trProgram" runat="server">
               <td><b>Program:</b> <span style="color: Red; font-weight: bold">*</span></td>
               <td>
                <asp:DropDownList ID="ddProgram" runat="server" />
               </td>
              </tr>
              <tr>
               <td valign="top" style="white-space: nowrap"><b>Task Type:</b> <span style="color: Red; font-weight: bold">*</span></td>
               <td>
                <div id="divAddTaskTypeTree" runat="server">
                 <cc:TreeSelector ID="addTaskType" Height="115" Width="250" DescWidth="200" LoadDesc="false" ShowScript="true" runat="server" />
                </div>
                <div id="divAddTaskType"></div>
                <asp:Label ID="lblEditTaskType" runat="server" />
               </td>
              </tr>
              <asp:PlaceHolder ID="phAddTaskComp" runat="server">
               <tr id="trAddTaskComp">
                <td><b>Complexity:</b></td>
                <td>
                 <div id="divAddTaskComp"></div>
                 <asp:Label ID="lblEditTaskComp" runat="server" />
                </td>
               </tr>
              </asp:PlaceHolder>
              <tr id="trAddTaskRE">
               <td style="white-space: nowrap"><b>R/E:</b> <span style="color: Red; font-weight: bold">*</span></td>
               <td>
                <asp:TextBox ID="addTaskRE" runat="server" Columns="5"/>
                <div id="divAddTaskRE"></div>
                <asp:Label ID="lblEditTaskRE" runat="server" />
               </td>
              </tr>
              <asp:PlaceHolder ID="phAddTaskCode" runat="server">
               <tr>
                <td valign="top" style="white-space: nowrap"><b>Unplanned Code:</b> <span style="color: Red; font-weight: bold">*</span></td>
                <td>
                 <cc:TreeSelector ID="addUnplanCode" Height="115" Width="370" LoadDesc="false" ShowScript="false" runat="server" />
                </td>
               </tr>
              </asp:PlaceHolder>
              <tr>
               <td valign="top" style="white-space: nowrap"><b>Exit Criteria:</b></td>
               <td>
                <asp:TextBox ID="addTaskExit" Rows="4" Columns="50" TextMode="MultiLine" runat="server"></asp:TextBox>
                <div id="divAddTaskExit"></div>
                <asp:Label ID="lblEditTaskExit" runat="server" />
               </td>
              </tr>
              <tr>
               <td valign="top"><b>Comments:</b></td>
               <td><asp:TextBox ID="addTaskAComments" Rows="4" Columns="50" TextMode="MultiLine" runat="server"></asp:TextBox></td>
              </tr>
              <tr>
               <td colspan="2" align="right">
                <asp:HiddenField ID="addTaskPlanned" runat="server" />
                <asp:Button ID="btnAddTaskToPlan" OnClick="btnAddTaskToPlan_Click" OnClientClick="return ValidateAddTask()" runat="server" Text="Add Task" />
                <asp:Button ID="btnAddFavToPlan" OnClick="btnAddFavToPlan_Click" OnClientClick="return ValidateFavorite()" runat="server" Text="Add Favorite" />
                <asp:Button ID="btnUpdTask" OnClick="btnUpdTask_Click" OnClientClick="return ValidateEditTask()" runat="server" Text="Update Task" />
                <input type="button" onclick="$('.popupTask').dialog('close')" value="Cancel" />
               </td>
              </tr>
             </table>
            </asp:Panel>
            
            <asp:Panel ID="popupPrevTasks" CssClass="popupPrevTasks" title="Add Tasks From Previous Week" Visible="false" style="display: none" runat="server">
             <table cellpadding="3" cellspacing="0" class="dialogTbl" width="100%">
              <tr>
               <td><b>Open Tasks From <asp:Label ID="lblPrevWeek" runat="server" />:</b></td>
              </tr>
              <tr>
               <td>
                <asp:ListBox ID="lbPrevTasks" Width="100%" Rows="8" SelectionMode="Multiple" runat="server" /><br />
                You can select multiple tasks by holding down the Ctrl key when clicking on each task.
               </td>
              </tr>
              <tr>
               <td align="right" height="35">
                <asp:Button ID="btnAddPrev" Text="Add Task(s)" OnClientClick="return CheckPrevTasks()" OnClick="btnAddPrev_Click" runat="server" />
                <input type="button" onclick="$('.popupPrevTasks').dialog('close')" value="Cancel" />
               </td>
              </tr>
             </table>
            </asp:Panel>

            <asp:Panel ID="popupBarrier" CssClass="popupBarrier" Visible="false" style="display: none" runat="server">
             <table cellpadding="3" cellspacing="0" class="dialogTbl" width="100%">
              <tr>
               <td><b>Task:</b></td>
               <td>
                <asp:Label ID="barTask" runat="server"></asp:Label>
                <asp:HiddenField ID="barTaskID" runat="server" />
               </td>
              </tr>
              <tr>
               <td valign="top" style="white-space: nowrap"><b>Barrier Code:</b> <span style="color: Red; font-weight: bold">*</span></td>
               <td>
                <cc:TreeSelector ID="barCode" Height="200" Width="280" DescWidth="220" LoadDesc="true" ShowScript="true" runat="server" />
               </td>
              </tr>
              <tr>
               <td><b>Type:</b></td>
               <td>
                <asp:RadioButton ID="barTypeEfficiency" GroupName="barType" runat="server" Text="Efficiency" Checked="true" />
                <asp:RadioButton ID="barTypeDelay" GroupName="barType" runat="server" Text="Delay" />
               </td>
              </tr>
              <tr>
               <td valign="top" style="white-space: nowrap"><b>Comment:</b> <span style="color: Red; font-weight: bold">*</span></td>
               <td><asp:TextBox ID="barComment" Rows="4" Columns="50" TextMode="MultiLine" runat="server"></asp:TextBox></td>
              </tr>
              <tr>
               <td style="white-space:nowrap"><b>IS Ticket #:</b></td>
               <td><asp:TextBox ID="barTicket" Columns="16" runat="server"></asp:TextBox></td>
              </tr>
              <tr>
               <td colspan="2" align="right">
                <asp:Button ID="btnAddBarrier" Text="Add Barrier" OnClientClick="return ValidateBarrier()" OnClick="btnAddBarrier_Click" runat="server" />
                <asp:Button ID="btnUpdBarrier" Text="Update Barrier" OnClientClick="return ValidateBarrier()" OnClick="btnUpdBarrier_Click" runat="server" />
                <input type="button" onclick="$('.popupBarrier').dialog('close');" value="Cancel" />
               </td>
              </tr>
             </table>
            </asp:Panel>

            <asp:Panel ID="popupAlert" title="Create Alert" CssClass="popupAlert" Visible="false" style="display: none" runat="server">
             <table cellpadding="3" cellspacing="0" class="dialogTbl" width="100%">
              <tr>
               <td><b>Task:</b></td>
               <td id="cellAlertTaskTitle" runat="server"></td>
              </tr>
              <tr id="rowAlertBarrier" runat="server">
               <td><b>Barrier:</b></td>
               <td id="cellAlertBarrier" runat="server"></td>
              </tr>
              <tr>
               <td><b>Subject:</b> <span style="color: Red; font-weight: bold">*</span></td>
               <td><asp:TextBox ID="alertSubject" Columns="60" runat="server"></asp:TextBox></td>
              </tr>
              <tr>
               <td valign="top"><b>Message:</b></td>
               <td><asp:TextBox ID="alertMessage" Columns="60" Rows="6" TextMode="MultiLine" runat="server"></asp:TextBox></td>
              </tr>
              <tr>
               <td><b>Send To:</b></td>
               <td>
                <asp:Table ID="tblAlertSendTo" runat="server">
                 <asp:TableRow>
                  <asp:TableCell ID="cellAlertALM">
                   <asp:CheckBox ID="alertALM" runat="server" />
                   <asp:Label ID="lblAlertALM" runat="server" />
                  </asp:TableCell>
                  <asp:TableCell ID="cellAlertMgr"><asp:CheckBox ID="alertMgr" runat="server" /></asp:TableCell>
                  <asp:TableCell ID="cellAlertOwner"><asp:CheckBox ID="alertOwner" runat="server" /></asp:TableCell>
                  <asp:TableCell ID="cellAlertAssignee"></asp:TableCell>
                 </asp:TableRow>
                </asp:Table>
               </td>
              </tr>
              <tr>
               <td colspan="2" align="right">
                <asp:Button ID="btnCreateAlert" Text="Create Alert" OnClientClick="return ValidateAlert()" OnClick="btnCreateAlert_Click" runat="server" />
                <input type="button" onclick="$('.popupAlert').dialog('close');" value="Cancel" />
               </td>
              </tr>
             </table>
            </asp:Panel>

            <asp:Panel ID="popupCarry" title="Carry Tasks Forward" CssClass="popupCarry" Visible="false" style="display: none" runat="server">
             <table cellpadding="3" cellspacing="0" class="dialogTbl" width="100%">
              <tr>
               <td>Add the selected task(s) to the plan/log on:</td>
              </tr>
              <tr>
               <td><asp:DropDownList ID="ddCarryDate" runat="server"></asp:DropDownList></td>
              </tr>
              <tr>
               <td align="center" valign="bottom" height="35">
                <asp:Button ID="btnCarry" Text="Submit" OnClientClick="return UpdateCheck()" OnClick="btnCarry_Click" runat="server" />
                <input type="button" onclick="$('.popupCarry').dialog('close');" value="Cancel" />
               </td>
              </tr>
             </table>
            </asp:Panel>
        </ContentTemplate>

        <Triggers>
         <asp:PostBackTrigger ControlID="lnkNewTask" />
         <asp:PostBackTrigger ControlID="lnkLeave" />
         <asp:PostBackTrigger ControlID="lnkRemove" />
         <asp:PostBackTrigger ControlID="lnkAddToFavs" />
         <asp:PostBackTrigger ControlID="btnAddTaskToPlan" />
         <asp:PostBackTrigger ControlID="btnAddPrev" />
         <asp:PostBackTrigger ControlID="btnAddFavToPlan" />
         <asp:PostBackTrigger ControlID="btnUpdTask" />
         <asp:PostBackTrigger ControlID="btnAddBarrier" />
         <asp:PostBackTrigger ControlID="btnUpdBarrier" />
         <asp:PostBackTrigger ControlID="btnCreateAlert" />
         <asp:PostBackTrigger ControlID="btnCarry" />
         <asp:AsyncPostBackTrigger ControlID="lnkAddTask" EventName="Click" />
         <asp:AsyncPostBackTrigger ControlID="lnkAddPrev" EventName="Click" />
         <asp:AsyncPostBackTrigger ControlID="lnkAddFav" EventName="Click" />
         <asp:AsyncPostBackTrigger ControlID="lnkAddBarrier" EventName="Click" />
         <asp:AsyncPostBackTrigger ControlID="lnkEdit" EventName="Click" />
         <asp:AsyncPostBackTrigger ControlID="lnkAlert" EventName="Click" />
         <asp:AsyncPostBackTrigger ControlID="lnkCarry" EventName="Click" />
        </Triggers>
       </asp:UpdatePanel>

      </asp:TableCell>
      <asp:TableCell HorizontalAlign="Right" style="padding: 0px">
       <table cellspacing="0" cellpadding="0">
        <tr>
         <td style="padding-right: 8px; border-right: 2px solid Black" height="24">
          <b>Status:</b>
          <asp:Label ID="lblStatus" runat="server"></asp:Label>
         </td>
        </tr>
       </table>
      </asp:TableCell>
     </asp:TableRow>
     <asp:TableRow ID="rowTblPlanLog">
      <asp:TableCell ColumnSpan="3" style="padding: 0px">
       <asp:Table ID="tblPlanLog" runat="server" Width="100%" class="weeklyTbl">
        <asp:TableHeaderRow ID="hdrPlan1">
         <asp:TableHeaderCell CssClass="field" ColumnSpan="2">Title</asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="field" ID="hdrCode">Unplanned Code</asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="field">Type</asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="field">Program</asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="field">Status</asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="field" ID="hdrComp">Complexity</asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="field">R/E</asp:TableHeaderCell>
         <asp:TableHeaderCell ID="hdrCellSpent">Hours<br />Spent</asp:TableHeaderCell>
         <asp:TableHeaderCell ID="hdrCellComp"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDay"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDay"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDay"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDay"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDay"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDay" ID="hdrToggleWE" VerticalAlign="Top" Width="10">
          <asp:LinkButton ID="lnkToggleWE" OnClientClick="return ToggleWE(this)" Font-Underline="false" runat="server">+</asp:LinkButton>
         </asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDayWE" ID="hdrSat" style="display: none"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDayWE" ID="hdrSun" style="display: none"></asp:TableHeaderCell>
         <asp:TableHeaderCell CssClass="planDay">Total</asp:TableHeaderCell>
        </asp:TableHeaderRow>
        <asp:TableHeaderRow ID="hdrPlan2">
         <asp:TableHeaderCell Width="40" CssClass="plan">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actual">Actual</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="plan">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actual">Actual</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="plan">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actual">Actual</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="plan">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actual">Actual</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="plan">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actual">Actual</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="planWE" style="display: none">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actualWE" style="display: none">Actual</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="planWE" style="display: none">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actualWE" style="display: none">Actual</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="plan">Plan</asp:TableHeaderCell>
         <asp:TableHeaderCell Width="40" CssClass="actual">Actual</asp:TableHeaderCell>
        </asp:TableHeaderRow>
       </asp:Table>
      </asp:TableCell>
     </asp:TableRow>

     <asp:TableRow>
      <asp:TableCell VerticalAlign="Top" style="font-size: 8pt; color: #666666">
       <asp:Label ID="lblMod" runat="server" />
      </asp:TableCell>
      <asp:TableCell HorizontalAlign="right" style="padding: 4px 0px 0px 0px">
       <table id="tblButtons" runat="server" cellspacing="0" cellpadding="0">
        <tr>
         <td>
          <asp:Button ID="btnUndo" Text="Undo Changes" UseSubmitBehavior="false"
            OnClick="btnUndo_Click" OnClientClick="if(!confirm('Are you sure you want to lose all changes?'))return false" Width="110" runat="server" />
         </td>
         <td style="padding-left: 3px"><asp:Button ID="btnSave" Text="Save" OnClick="btnSave_Click" Width="100" runat="server" /></td>
         <td id="cellReady" style="padding-left: 3px"><asp:Button ID="btnReady" OnClick="btnReady_Click" UseSubmitBehavior="false" runat="server" /></td>
         <td id="cellApprove" style="padding-left: 3px"><asp:Button ID="btnApprove" OnClick="btnApprove_Click" UseSubmitBehavior="false" runat="server" /></td>
        </tr>
       </table>
      </asp:TableCell>
     </asp:TableRow>

     <asp:TableRow ID="rowTotalsTbl">
      <asp:TableCell ColumnSpan="2">
       <asp:Table runat="server">
        <asp:TableRow><asp:TableCell style="font-size: 16pt"><b>Total Hours</b></asp:TableCell></asp:TableRow>
        <asp:TableRow>
         <asp:TableCell>
          <asp:Table ID="tblTotals" runat="server" class="weeklyTbl">
           <asp:TableHeaderRow ID="hdrTotals">
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field"></asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="field">Total</asp:TableHeaderCell>
           </asp:TableHeaderRow>
           <asp:TableRow ID="rowTotalsPlan">
            <asp:TableCell><b>Planned</b></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
           </asp:TableRow>
           <asp:TableRow ID="rowTotalsPlanAct">
            <asp:TableCell><b>Hours Against Planned</b></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
           </asp:TableRow>
           <asp:TableRow ID="rowTotalsUnplan">
            <asp:TableCell><b>Hours Against Unplanned</b></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
           </asp:TableRow>
           <asp:TableRow ID="rowTotalsActual">
            <asp:TableCell><b>Total Actuals</b></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
           </asp:TableRow>
           <asp:TableRow ID="rowTotalsBarrier">
            <asp:TableCell><b>Efficiency Barriers</b></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
           </asp:TableRow>
           <asp:TableRow ID="rowTotalsDelay">
            <asp:TableCell><b>Delay Barriers</b></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
            <asp:TableCell CssClass="totalHour"></asp:TableCell>
           </asp:TableRow>
          </asp:Table>
         </asp:TableCell>
        </asp:TableRow>
       </asp:Table>
      </asp:TableCell>
     </asp:TableRow>
    </asp:Table>
</asp:Content>
