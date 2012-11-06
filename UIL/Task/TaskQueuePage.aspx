<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TaskQueuePage.aspx.cs"
    EnableEventValidation="false" Inherits="WALT.UIL.Task.TaskQueuePage" MasterPageFile="/Site1.Master" %>


<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <script type="text/javascript">

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(PopupForm);

        function PopupForm() {
            $(function () {
                var popup = $('#popupForm');

                if (popup.size() > 0) {

                    var dialog;

                    if (popup.val() == 'dialogPanel') {
                        dialog = $('.dialogPanel');

                        dialog.first().dialog({
                            width: 350,
                            modal: true,
                            open: function (type, data) { $(this).parent().appendTo('form'); }
                        });
                    }
                    else {
                        dialog = $('.popupAlert');

                        dialog.first().dialog({
                            width: 550,
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
        
        function toggleFilters() {
                          
            $('.columnSelector').css({ display: 'none' });
            $('*[id*=closeReorderDialogHF]').val('No');

            if ($('.filterWrapper').is(':visible')) {
                $('*[id*=filterShowHiddenField]').val('No');
                $(".filterArrow").attr("src", "../css/images/arrow_right.png");
                $('.filterWrapper').slideUp('slow');
            } else {
                $('*[id*=filterShowHiddenField]').val('Yes');
                $(".filterArrow").attr("src", "../css/images/arrow_down.png");
                $('.filterWrapper').slideDown('slow');
            }
        }

        function SelectCheck() {
            var valid = false;

            $('.idCheckBox').each(function () {
                var chk = this.firstChild;

                if (chk.checked) {
                    valid = true;
                }
            });

            if (!valid) {
                alert('No tasks selected');
                return false;
            }
        }

        function SingleCheck() {
            var count = 0;

            $('.idCheckBox').each(function () {
                var chk = this.firstChild;

                if (chk.checked) {
                    count++;
                }
            });

            if (count == 0) {
                alert('No tasks selected');
                return false;
            }
            else if (count > 1) {
                alert('Multiple tasks selected, please select a single task');
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

        function GetPrefix() {
            var prefix = $('a[id$="_lnkAlert"]').attr('id');
            return prefix.substr(0, prefix.length - 8);
        }
    </script>

    <asp:HiddenField ID="filterShowHiddenField" runat="server" />
     <asp:PlaceHolder ID="taskQueueHeaderPlaceHolder" runat="server">
        <table width="100%" class="gridPageHeader" cellpadding="0" cellspacing="0">
            <tr>
                <td align="right">
                    <table style="text-align: left;">
                        <tr>                           
                            <td style="padding-right: 5px; padding-left: 5px;">
                                <asp:PlaceHolder ID="TeamPlaceHolder" runat="server" Visible="false"><b>Teams:</b>
                                    <br />
                                    <asp:DropDownList ID="TeamDropDownList" runat="server" AutoPostBack="true" CssClass="ddStyle"
                                        OnSelectedIndexChanged="TeamDropDownList_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </asp:PlaceHolder>
                            </td>
                            <td style="padding-right: 5px; padding-left: 5px;">
                                <asp:PlaceHolder ID="TeamMembersPlaceHolder" runat="server" Visible="false"><b>Team
                                    Members: </b>
                                    <br />
                                    <asp:DropDownList ID="TeamMembersDropDownList" runat="server" AutoPostBack="true"
                                        CssClass="ddStyle" OnSelectedIndexChanged="TeamMembersDropDownList_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </asp:PlaceHolder>
                            </td>
                            <td style="padding-right: 5px; padding-left: 5px;">
                                <b>Role:</b>
                                <br />
                                <asp:DropDownList ID="taskRoleDropDownList" runat="server" AutoPostBack="true" CssClass="ddStyle"
                                    OnSelectedIndexChanged="taskFilters_SelectedIndexChanged">
                                    <asp:ListItem Value="Assignee">Assignee</asp:ListItem>
                                    <asp:ListItem Value="Owner">Owner</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td style="padding-right: 5px; padding-left: 5px;">
                                <b>Task Status: </b>
                                <br />
                                <asp:DropDownList ID="taskFilterDropDownList" runat="server" AutoPostBack="true"
                                    CssClass="ddStyle" OnSelectedIndexChanged="taskFilters_SelectedIndexChanged">
                                </asp:DropDownList>
                                <asp:Button ID="btnSaveView" runat="server" Text="Save as Default" OnClick="btnSaveView_Click"
                                    Style="margin-left: 5px;" />
                              <%--  <asp:LinkButton ID="saveViewLinkButton" runat="server" OnClick="saveViewLinkButton_Click" style=" margin-left:5px;"
                                    Font-Size="10px">Save as Default</asp:LinkButton>--%>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
           <%-- <tr>
                <td colspan="2">
                    <asp:PlaceHolder ID="TeamPlaceHolder" runat="server" Visible="false">Teams:
                        <asp:DropDownList ID="TeamDropDownList" runat="server" AutoPostBack="true" CssClass="ddStyle"
                            OnSelectedIndexChanged="TeamDropDownList_SelectedIndexChanged">
                        </asp:DropDownList>
                    </asp:PlaceHolder>
                </td>
            </tr>--%>
           <%-- <tr>
                <td colspan="2">
                    <asp:PlaceHolder ID="TeamMembersPlaceHolder" runat="server" Visible="false">Team Members:
                        <asp:DropDownList ID="TeamMembersDropDownList" runat="server" AutoPostBack="true"
                            CssClass="ddStyle" OnSelectedIndexChanged="TeamMembersDropDownList_SelectedIndexChanged">
                        </asp:DropDownList>
                    </asp:PlaceHolder>
                </td>
            </tr>--%>
           <%-- <tr>
                <td colspan="2">
                    Role:
                    <asp:DropDownList ID="taskRoleDropDownList" runat="server" AutoPostBack="true" CssClass="ddStyle"
                        OnSelectedIndexChanged="taskFilters_SelectedIndexChanged">
                        <asp:ListItem Value="Assignee">Assignee</asp:ListItem>
                        <asp:ListItem Value="Owner">Owner</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>--%>
            <tr>
                <td  style="text-align: left;" class="lbtnStyle">
                  <asp:UpdatePanel ID="popupPanel" runat="server">
                   <ContentTemplate>
                   
                   <asp:Table ID="Table1" CssClass="menueWrapper" Width="100%" CellSpacing="0" CellPadding="5" runat="server" style=" color:White; background-color:#666666;">
                        <asp:TableRow>

                             <asp:TableCell ID="cellEdit" Width="100" HorizontalAlign="Center" style="text-align:center; cursor: pointer; padding-left:10px; padding-right:10px;"
                                 onmouseover="this.bgColor='#000000'" onmouseout="this.bgColor='#666666'">
                             <a id="A3" href="TaskForm.aspx" class="lbtnStyle" runat="server">Create&nbsp;New&nbsp;Task</a>
                             </asp:TableCell>
                             <asp:TableCell ID="cellRemove" Width="40" HorizontalAlign="Center" Style="text-align: center;
                                 cursor: pointer; padding-left: 10px; padding-right: 10px;" onmouseover="this.bgColor='#000000'"
                                 onmouseout="this.bgColor='#666666'">
                             <a id="A4" href="Import.aspx" runat="server">Import</a>
                             </asp:TableCell> 
                              <asp:TableCell ID="TableCell1" Width="100" HorizontalAlign="Center" Style="text-align: center;
                                  cursor: pointer; padding-left:10px; padding-right: 10px;" onmouseover="this.bgColor='#000000'"
                                  onmouseout="this.bgColor='#666666'">
                           <asp:LinkButton ID="lnkAddFav" Text="Add&nbsp;to&nbsp;Favorites" OnClientClick="return SelectCheck()" OnClick="lnkAddFav_Click" runat="server" />
                             </asp:TableCell> 
                               <asp:TableCell ID="TableCell2" Width="100" HorizontalAlign="Center" Style="text-align: center;
                                   cursor: pointer; padding-left: 10px; padding-right: 10px;" onmouseover="this.bgColor='#000000'"
                                   onmouseout="this.bgColor='#666666'">
                           <asp:LinkButton ID="lnkAlert" Text="Create&nbsp;Alert" OnClientClick="return SingleCheck()" OnClick="lnkAlert_Click" runat="server" />
                             </asp:TableCell> 
                               <asp:TableCell ID="TableCell3" Width="40" HorizontalAlign="Center" Style="text-align: center;
                                   cursor: pointer; padding-left: 10px; padding-right: 10px;" onmouseover="this.bgColor='#000000'"
                                   onmouseout="this.bgColor='#666666'">
                           <asp:LinkButton ID="lnkReject" Text="Reject" OnClientClick="return SelectCheck()" OnClick="lnkReject_Click" runat="server" /> 
                             </asp:TableCell> 
                               <asp:TableCell ID="TableCell4" Width="40" HorizontalAlign="Center" Style="text-align: center;
                                   cursor: pointer; padding-left: 10px; padding-right: 10px;" onmouseover="this.bgColor='#000000'"
                                   onmouseout="this.bgColor='#666666'">
                           <asp:LinkButton ID="lnkDelete" Text="Delete" OnClientClick="return SelectCheck()" OnClick="lnkDelete_Click" runat="server" />
                             </asp:TableCell> 

                              <asp:TableCell ID="TableCell5" Width="100%" HorizontalAlign="Center" style="">
                          
                             </asp:TableCell> 
               
                        </asp:TableRow>
                   </asp:Table>
           

                   <%-- <a id="A1" href="TaskForm.aspx" class="lbtnStyle" runat="server">Create New Task</a> &nbsp;|&nbsp;
                    <a id="A2" href="Import.aspx" runat="server">Import</a> &nbsp;|&nbsp;
                    <asp:LinkButton ID="lnkAddFav" Text="Add to Favorites" OnClientClick="return SelectCheck()" OnClick="lnkAddFav_Click" runat="server" /> &nbsp;|&nbsp;
                    <asp:LinkButton ID="lnkAlert" Text="Create Alert" OnClientClick="return SingleCheck()" OnClick="lnkAlert_Click" runat="server" /> &nbsp;|&nbsp;                   
                    <asp:LinkButton ID="lnkReject" Text="Reject" OnClientClick="return SelectCheck()" OnClick="lnkReject_Click" runat="server" /> &nbsp;|&nbsp;
                    <asp:LinkButton ID="lnkDelete" Text="Delete" OnClientClick="return SelectCheck()" OnClick="lnkDelete_Click" runat="server" />
--%>
                    <asp:PlaceHolder ID="phUpdatePanel" runat="server"></asp:PlaceHolder>

                    <asp:Panel ID="dialogPanel" CssClass="dialogPanel" Visible="false" style="display: none" runat="server">
                     <asp:Table ID="dialogTbl" CssClass="dialogTbl" runat="server" Width="100%">
                      <asp:TableRow>
                       <asp:TableCell ID="dialogHdr"></asp:TableCell>
                      </asp:TableRow>
                      <asp:TableRow>
                       <asp:TableCell ID="dialogBody"></asp:TableCell>
                      </asp:TableRow>
                      <asp:TableRow ID="dialogExtraRow">
                       <asp:TableCell ID="dialogExtra"></asp:TableCell>
                      </asp:TableRow>
                      <asp:TableRow>
                       <asp:TableCell HorizontalAlign="Center" Height="35">
                        <asp:Button ID="dialogBtn" OnClick="dialogBtn_Click" runat="server" />
                        <input type="button" onclick="$('.dialogPanel').dialog('close')" id="dialogBtnClose" runat="server" />
                       </asp:TableCell>
                      </asp:TableRow>
                     </asp:Table>    
                    </asp:Panel>

                    <asp:Panel ID="popupAlert" title="Create Alert" CssClass="popupAlert" Visible="false" style="display: none" runat="server">
                     <table cellpadding="3" cellspacing="0" class="dialogTbl" width="100%">
                      <tr>
                       <td><b>Task:</b></td>
                       <td id="cellAlertTaskTitle" runat="server"></td>
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
                   </ContentTemplate>

                   <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lnkAlert" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="lnkReject" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="lnkDelete" EventName="Click" />
                    <asp:PostBackTrigger ControlID="lnkAddFav" />
                    <asp:PostBackTrigger ControlID="btnCreateAlert" />
                    <asp:PostBackTrigger ControlID="dialogBtn" />
                   </Triggers>
                  </asp:UpdatePanel>
                </td>
              <%--  <td>
                    Task Status:
                    <asp:DropDownList ID="taskFilterDropDownList" runat="server" AutoPostBack="true"
                        CssClass="ddStyle" OnSelectedIndexChanged="taskFilters_SelectedIndexChanged">
                    </asp:DropDownList>
                    <br />
                    <asp:LinkButton ID="saveViewLinkButton" runat="server" OnClick="saveViewLinkButton_Click"
                        Font-Size="10px">Save as Default</asp:LinkButton>
                </td>--%>
            </tr>           
        </table>
    </asp:PlaceHolder>
    <asp:UpdatePanel runat="server" ID="filterUpdatePanel" UpdateMode="Conditional">
        <ContentTemplate>
            <span class="filterBtn">
                <img class="filterArrow" alt="Filter" src="../css/images/arrow_right.png" />&nbsp;<a
                    href="javascript:void(0);" onclick="toggleFilters();">Filters</a></span>
            <div class="filterWrapper">   
                <asp:Table ID="filterTable" runat="server" CssClass="filterTable">
                    <asp:TableRow ID="TableRow1">
                        <asp:TableCell  ColumnSpan="2">
                            Filters:&nbsp;<asp:DropDownList ID="filterPickDropDownList" runat="server" AutoPostBack="True"
                                onchange="$('*[id*=closeReorderDialogHF]').val('No');" OnSelectedIndexChanged="filterPickDropDownList_SelectedIndexChanged">
                                <asp:ListItem Value=""></asp:ListItem>
                                <asp:ListItem Value="OWNER">Owner</asp:ListItem>
                                <asp:ListItem Value="ASSIGNED">Assignee</asp:ListItem>
                                <asp:ListItem Value="TITLE">Title</asp:ListItem>
                                <asp:ListItem Value="TASKTYPE">Task Type</asp:ListItem>
                                <asp:ListItem Value="SOURCE">Source</asp:ListItem>
                                <asp:ListItem Value="SOURCEID">Source Id</asp:ListItem>
                                <asp:ListItem Value="STARTDATE">Start Date</asp:ListItem>
                                <asp:ListItem Value="DUEDATE">Due Date</asp:ListItem>
                                <asp:ListItem Value="STATUS">Status</asp:ListItem>
                                <asp:ListItem Value="HOURS">Hours Allocated</asp:ListItem>                               
                                <asp:ListItem Value="ESTIMATE">R/E</asp:ListItem>
                                <asp:ListItem Value="COMPLEXITY">Complexity</asp:ListItem>
                                <asp:ListItem Value="PROGRAM">Program</asp:ListItem>
                                <asp:ListItem Value="CREATED">Created Date</asp:ListItem>
                                <asp:ListItem Value="COMPLETEDDATE">Completed Date</asp:ListItem>
                                <asp:ListItem Value="ONHOLDDATE">On Hold Date</asp:ListItem>
                                <asp:ListItem Value="INSTANTIATED">Instantiated</asp:ListItem>                               
                                <asp:ListItem Value="EXITCRITERIA">Exit Criteria</asp:ListItem>
                                <asp:ListItem Value="WBS">WBS</asp:ListItem>
                                <asp:ListItem Value="OWNERCOMMENTS">Owner Comments</asp:ListItem>
                                <asp:ListItem Value="ASSIGNEECOMMENTS">Assignee Comments</asp:ListItem>
                            </asp:DropDownList>
                            &nbsp;
                            <asp:Label ID="updateLabel" runat="server" Text="Updated!" ForeColor="Green"
                                Visible="false"></asp:Label>
                        </asp:TableCell>                      
                    </asp:TableRow>
                    <asp:TableRow ID="titleFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton1" OnClick="RemoveLinkButton_Click" CommandArgument="TITLE"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Title:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="titleFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="ownerFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton2" OnClick="RemoveLinkButton_Click" CommandArgument="OWNER"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Owner:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="ownerFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="assigneeFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton3" OnClick="RemoveLinkButton_Click" CommandArgument="ASSIGNED"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Assignee:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="assigneeFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="typeFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton4" OnClick="RemoveLinkButton_Click" CommandArgument="TASKTYPE"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Task Type:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="typeFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="sourceFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton5" OnClick="RemoveLinkButton_Click" CommandArgument="SOURCE"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Source:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="sourceFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="sourceIdFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton6" OnClick="RemoveLinkButton_Click" CommandArgument="SOURCEID"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Source ID:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="sourceIdFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="startFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton7" OnClick="RemoveLinkButton_Click" CommandArgument="STARTDATE"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Start Date:
                        </asp:TableCell>
                        <asp:TableCell>
                        &nbsp;After
                            <asp:TextBox ID="startAfterFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>                                                        
                            <asp:CompareValidator ID="CompareValidator1" runat="server" ControlToValidate="startAfterFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>                               
                            &nbsp;&nbsp;
                            Before
                            <asp:TextBox ID="startBeforeFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>


                            <asp:CompareValidator ID="CompareValidator3" runat="server" ControlToValidate="startBeforeFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>


                            <asp:CompareValidator ID="CompareValidator2" runat="server" ControlToCompare="startBeforeFilterTextBox"
                                ControlToValidate="startAfterFilterTextBox" Display="Dynamic" ErrorMessage="<br />Before date must be set prior to after date. "
                                Operator="LessThan" Type="Date" ValueToCompare="<%= startBeforeFilterTextBox.Text.ToShortString() %>"></asp:CompareValidator>
                          
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="dueFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton8" OnClick="RemoveLinkButton_Click" CommandArgument="DUEDATE"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Due Date:
                        </asp:TableCell>
                        <asp:TableCell>
	                         &nbsp;After
                            <asp:TextBox ID="dueAfterFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>&nbsp;&nbsp;
                            <asp:CompareValidator ID="CompareValidator4" runat="server" ControlToValidate="dueAfterFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            Before
                            <asp:TextBox ID="dueBeforeFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>
                            <asp:CompareValidator ID="CompareValidator5" runat="server" ControlToValidate="dueBeforeFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            <asp:CompareValidator ID="CompareValidator6" runat="server" ControlToCompare="dueBeforeFilterTextBox"
                                ControlToValidate="dueAfterFilterTextBox" Display="Dynamic" ErrorMessage="<br />Before date must be set prior to after date. "
                                Operator="LessThan" Type="Date" ValueToCompare="<%= dueBeforeFilterTextBox.Text.ToShortString() %>"></asp:CompareValidator>

                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="statusFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton9" OnClick="RemoveLinkButton_Click" CommandArgument="STATUS"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" CausesValidation="false"
                                runat="server" Font-Bold="True" Font-Underline="false" ForeColor="Red" CssClass="filters">X</asp:LinkButton>
                            Status:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:RadioButtonList ID="statusRadioButtonList" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Value="OPEN">Open</asp:ListItem>
                                <asp:ListItem Value="COMPLETED">Completed</asp:ListItem>
                                <asp:ListItem Value="HOLD">Hold</asp:ListItem>
                                <asp:ListItem Value="REJECTED">Rejected</asp:ListItem>
                                <asp:ListItem Value="OBE">OBE</asp:ListItem>
                            </asp:RadioButtonList>                  
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="hoursFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton10" OnClick="RemoveLinkButton_Click" CommandArgument="HOURS"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" CausesValidation="false"
                                runat="server" Font-Bold="True" Font-Underline="false" ForeColor="Red" CssClass="filters">X</asp:LinkButton>
                            Hours Allocated:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:DropDownList ID="hrsOprDropDownList" runat="server">
                                <asp:ListItem Value="Equal" Selected="False">Equal</asp:ListItem>
                                <asp:ListItem Value="Greater">Greater than</asp:ListItem>
                                <asp:ListItem Value="Less">Less than</asp:ListItem>
                            </asp:DropDownList>
                           &nbsp; 
                            <asp:TextBox ID="hoursFilterTextBox" runat="server"></asp:TextBox>
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="hoursFilterTextBox"
                                ValidationExpression="^(\.\d)|(\d+(\.\d)?)$" ErrorMessage="<br />Please Enter a valid hours"
                                ForeColor="Red" Display="Dynamic"></asp:RegularExpressionValidator>                            
                        </asp:TableCell>
                    </asp:TableRow>                    
                    <asp:TableRow ID="reFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton11" OnClick="RemoveLinkButton_Click" CommandArgument="ESTIMATE"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" CausesValidation="false"
                                runat="server" Font-Bold="True" Font-Underline="false" ForeColor="Red" CssClass="filters">X</asp:LinkButton>
                            R/E:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:DropDownList ID="reOprDropDownList" runat="server">
                                <asp:ListItem Value="Equal" Selected="False">Equal</asp:ListItem>
                                <asp:ListItem Value="Greater">Greater than</asp:ListItem>
                                <asp:ListItem Value="Less">Less than</asp:ListItem>
                            </asp:DropDownList>
                           &nbsp; 
                            <asp:TextBox ID="reFilterTextBox" runat="server" ></asp:TextBox>
                            <asp:RegularExpressionValidator ID="RERegularExpressionValidator" runat="server"
                                ControlToValidate="reFilterTextBox" ValidationExpression="^(\.\d)|(\d+(\.\d)?)$"
                                ErrorMessage="<br />Please Enter a valid R/E" ForeColor="Red" Display="Dynamic"></asp:RegularExpressionValidator>                           
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="complexityFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton12" OnClick="RemoveLinkButton_Click" CommandArgument="COMPLEXITY"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Complexity:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="complexityFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="programFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton13" OnClick="RemoveLinkButton_Click" CommandArgument="PROGRAM"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Program:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:DropDownList ID="programDropDownList" runat="server">
                            </asp:DropDownList>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="createdFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="LinkButton1" OnClick="RemoveLinkButton_Click" CommandArgument="CREATED"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Created Date:
                        </asp:TableCell>
                        <asp:TableCell>
                            &nbsp;After
                            <asp:TextBox ID="createdAfterFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>&nbsp;&nbsp;
                            <asp:CompareValidator ID="CompareValidator10" runat="server" ControlToValidate="createdAfterFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            Before
                            <asp:TextBox ID="createdBeforeFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>
                            <asp:CompareValidator ID="CompareValidator11" runat="server" ControlToValidate="createdBeforeFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            <asp:CompareValidator ID="CompareValidator12" runat="server" ControlToCompare="createdBeforeFilterTextBox"
                                ControlToValidate="createdAfterFilterTextBox" Display="Dynamic" ErrorMessage="<br />Before date must be set prior to after date. "
                                Operator="LessThan" Type="Date" ValueToCompare="<%= createdBeforeFilterTextBox.Text.ToShortString() %>"></asp:CompareValidator>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="completeFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton14" OnClick="RemoveLinkButton_Click" CommandArgument="COMPLETEDDATE"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Complete Date:
                        </asp:TableCell>
                        <asp:TableCell>
                        &nbsp;After
                            <asp:TextBox ID="completeAfterFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>&nbsp;&nbsp;
                            <asp:CompareValidator ID="CompareValidator7" runat="server" ControlToValidate="completeAfterFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            Before
                            <asp:TextBox ID="completeBeforeFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>
                            <asp:CompareValidator ID="CompareValidator8" runat="server" ControlToValidate="completeBeforeFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            <asp:CompareValidator ID="CompareValidator9" runat="server" ControlToCompare="completeBeforeFilterTextBox"
                                ControlToValidate="completeAfterFilterTextBox" Display="Dynamic" ErrorMessage="<br />Before date must be set prior to after date. "
                                Operator="LessThan" Type="Date" ValueToCompare="<%= completeBeforeFilterTextBox.Text.ToShortString() %>"></asp:CompareValidator>

                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="holdFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="LinkButton2" OnClick="RemoveLinkButton_Click" CommandArgument="ONHOLDDATE"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            On Hold Date:
                        </asp:TableCell>
                        <asp:TableCell>
                            &nbsp;After
                            <asp:TextBox ID="holdAfterFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>&nbsp;&nbsp;
                            <asp:CompareValidator ID="CompareValidator13" runat="server" ControlToValidate="holdAfterFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            Before
                            <asp:TextBox ID="holdBeforeFilterTextBox" runat="server" CssClass="datepicker"></asp:TextBox>
                            <asp:CompareValidator ID="CompareValidator14" runat="server" ControlToValidate="holdBeforeFilterTextBox"
                                Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                ForeColor="Red" Display="Dynamic"></asp:CompareValidator>
                            <asp:CompareValidator ID="CompareValidator15" runat="server" ControlToCompare="holdBeforeFilterTextBox"
                                ControlToValidate="holdAfterFilterTextBox" Display="Dynamic" ErrorMessage="<br />Before date must be set prior to after date. "
                                Operator="LessThan" Type="Date" ValueToCompare="<%= holdBeforeFilterTextBox.Text.ToShortString() %>"></asp:CompareValidator>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="instFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton15" OnClick="RemoveLinkButton_Click" CommandArgument="INSTANTIATED"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Instantiated:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:RadioButtonList ID="instFilterRadioButtonList" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Value="true">Instantiated</asp:ListItem>
                                <asp:ListItem Value="false">Uninstantiated</asp:ListItem>
                            </asp:RadioButtonList>                           
                        </asp:TableCell>
                    </asp:TableRow>                   
                    <asp:TableRow ID="exitFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton17" OnClick="RemoveLinkButton_Click" CommandArgument="EXITCRITERIA"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Exit Criteria:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="exitFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="wbsFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton18" OnClick="RemoveLinkButton_Click" CommandArgument="WBS"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            WBS:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="wbsFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="ownerCommFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton19" OnClick="RemoveLinkButton_Click" CommandArgument="OWNERCOMMENTS"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Owner Comments:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="ownerCommFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="assigneeCommFilterRow" Visible="false">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveLinkButton20" OnClick="RemoveLinkButton_Click" CommandArgument="ASSIGNEECOMMENTS"
                                CausesValidation="false" runat="server" Font-Bold="True" Font-Underline="false"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" ForeColor="Red"
                                CssClass="filters">X</asp:LinkButton>
                            Assignee Comments:
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="assigneeCommFilterTextBox" runat="server" Width="300px"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>                 
                    <asp:TableRow ID="TableRow2">
                        <asp:TableCell CssClass="leftFilterCell">
                            <asp:LinkButton ID="RemoveAllLinkButton" runat="server" OnClick="RemoveAllLinkButton_Click"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" Visible="false">Remove All</asp:LinkButton>
                        </asp:TableCell>
                        <asp:TableCell style=" text-align:right;">
                            <asp:Button ID="filterButton" runat="server" Text="Update" OnClick="filterButton_Click"
                                OnClientClick=" $('*[id*=closeReorderDialogHF]').val('No');" Visible="false" />                   
                        </asp:TableCell>
                    </asp:TableRow>
                 
                </asp:Table>            
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>  
    <uc1:TaskGrid ID="TaskGrid1" runat="server" />

</asp:Content>

