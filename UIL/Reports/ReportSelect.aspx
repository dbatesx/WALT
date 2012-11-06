<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportSelect.aspx.cs" Inherits="WALT.UIL.Reports.ReportSelect"
    MasterPageFile="~/Site1.Master" EnableEventValidation="false" %>

<%@ Register Assembly="AjaxControlToolkit, Version=3.5.51116.0, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e"
    Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register TagPrefix="asp" TagName="ProfileSelector" Src="~/Controls/ProfileSelector.ascx" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <style type="text/css">
        .textCell{ width: 180px; min-width: 180px; white-space: nowrap; font-size:14px; vertical-align: top; background-color: White; }
        .inputCell{ width: 550px; background-color: #EBEBEB; vertical-align: top; }
        .asterisk{ font-size: 12px; color: red; }
        .lastSavedBy{ font-size: 10px; color: Gray; vertical-align: top; }
        .style2{ color: #FF0000; }
        .style3{ height: 38px; }
        .style6{ text-align: right; }
    </style>
    <script type="text/javascript" language="JavaScript">

        function GroupAddProfile(txt, user) {
            var listbox = $('.lbGroupProfiles');

            if (listbox.html().indexOf('>' + user + '<') == -1) {
                listbox.append('<option value="' + user + '">' + user + '</option>');
            }

            setTimeout(function () { txt.value = ''; }, 10);

        }

        function GroupAdd(type) {
            var dd = document.getElementById($('.ddGroup' + type + 's').attr('id'));

            if (dd.selectedIndex > 0) {
                var sel = dd.options[dd.selectedIndex];
                var prefix = dd.id.substr(0, dd.id.length - 8 - type.length);
                var listbox = $('#' + prefix + 'lbGroup' + type + 's');

                if (sel.text == 'All') {
                    for (var i = 2; i < dd.options.length; i++) {
                        sel = dd.options[i];

                        if (listbox.html().indexOf('="' + sel.value + '">') == -1) {
                            listbox.append('<option value="' + sel.value + '">' + sel.text + '</option>');
                        }
                    }

                    while (dd.options.length > 2) {
                        dd.options.remove(2);
                    }
                }
                else {
                    if (listbox.html().indexOf('="' + sel.value + '">') == -1) {
                        listbox.append('<option value="' + sel.value + '">' + sel.text + '</option>');
                    }

                    dd.options.remove(dd.selectedIndex);
                }

                dd.selectedIndex = -1;
            }
        }

        function GroupRemove(type) {
            var listbox = document.getElementById($('.lbGroup' + type + 's').attr('id'));
            var idx = 0;
            var sel = new Array();

            for (var i = 0; i < listbox.options.length; i++) {
                if (listbox.options[i].selected) {
                    sel[idx] = i;
                    idx++;
                }
            }

            if (sel.length > 0) {
                var prefix = listbox.id.substr(0, listbox.id.length - 8 - type.length);
                var dd = $('#' + prefix + 'ddGroup' + type + 's');

                for (var i = 0; i < sel.length; i++) {
                    var op = listbox.options[sel[i] - i];

                    if (type != 'Profile' && dd.html().indexOf('=' + op.value + '>') == -1) {
                        dd.append('<option value=' + op.value + '>' + op.text + '</option>');
                    }

                    listbox.options.remove(sel[i] - i);
                }
            }
        }

        function SaveGroup(id) {

            var prefix = id.substring(0, id.length - 13);
            var name = document.getElementById(prefix + 'txtGroupName');

            if (name.value == '') {
                alert('Please enter a Group Name');
                name.focus();
                return false;
            }

            var count = 0;
            var list = '';
            var ops = document.getElementById(prefix + 'lbGroupProfiles').options;

            for (var i = 0; i < ops.length; i++) {
                list += ops[i].value + '|';
                count++;
            }

            $('#' + prefix + 'selProfiles').val(list);

            list = '';
            ops = document.getElementById(prefix + 'lbGroupTeams').options;
            
            for (var i = 0; i < ops.length; i++) {
                list += ops[i].value + ',';
                count++;
            }

            $('#' + prefix + 'selTeams').val(list);

            list = '';
            ops = document.getElementById(prefix + 'lbGroupDirs').options;

            for (var i = 0; i < ops.length; i++) {
                list += ops[i].value + ',';
                count++;
            }

            $('#' + prefix + 'selDirs').val(list);

            list = '';
            ops = document.getElementById(prefix + 'lbGroupGroups');

            if (ops) {
                ops = ops.options;

                for (var i = 0; i < ops.length; i++) {
                    list += ops[i].value + ',';
                    count++;
                }

                $('#' + prefix + 'selGroups').val(list);
            }

            if (count == 0) {
                alert('Group does not contain any individuals, teams, directorates, or groups');
                return false;
            }

            return true;
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);

        function EndRequestHandler() {
            $(function () {
                $('input[id$="weekEnding"]').datepicker({
                    changeMonth: true,
                    changeYear: true,
                    numberOfMonths: 2
                });

                $('input[id$="tbDateRangeFrom"]').datepicker({
                    changeMonth: true,
                    changeYear: true,
                    numberOfMonths: 2
                });

                $('input[id$="tbDateRangeTo"]').datepicker({
                    changeMonth: true,
                    changeYear: true,
                    numberOfMonths: 2
                });

                if ($('#showdialog').size() == 1) {
                    var dialog = $('.GroupFilterPanel');

                    dialog.first().dialog({
                        title: 'Group Filter Editor',
                        width: 895,
                        resizable: false,
                        modal: true,
                        open: function () { $(this).parent().appendTo("form"); }
                    });

                    for (var i = 1; i < dialog.size(); i++) {
                        $(dialog[i]).remove();
                    }
                }
            });
        }

        EndRequestHandler();

    </script>

    <asp:Label ID="Label6" runat="server" Text=""></asp:Label>

    <asp:UpdatePanel runat="server" ID="upanelReportSelect">
        <ContentTemplate>
            <asp:Panel ID="Panel2" runat="server">
             <table cellpadding="0" cellspacing="0">
              <tr>
               <td colspan="2">
                <table cellpadding="10px" border="0" cellspacing="1" style="background: #CCCCCC">
                    <tr>
                        <td class="textCell">
                            <span>&nbsp</span>Existing Reports:
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                   <td>
                                      <asp:CheckBox ID="cbShowPrivate" runat="server"
                                          Text="Show Private"
                                          AutoPostBack="true"
                                          OnCheckedChanged="cbShowPrivate_CheckChanged"
                                          Checked="true" />
                                   </td>
                                   <td>&nbsp;</td>
                                   <td>
                                      <asp:DropDownList ID="ddlExistingReports" runat="server" AutoPostBack="True"
                                           OnSelectedIndexChanged="ddlExistingReports_SelectedIndexChanged" />
                                           
                                   </td>
                                   <td>
                                    <asp:Button ID="btnDeleteExisitingReport" runat="server" Text="Delete"
                                        OnClick="btnDeleteExisitingReport_Click" OnClientClick="return confirm('Are you sure?')" />
                                   </td>
                                </tr>
                                <tr>
                                 <td colspan="2">
                                      <asp:CheckBox ID="cbShowPublic" runat="server"
                                          Text="Show Public"
                                          AutoPostBack="true"
                                          OnCheckedChanged="cbShowPublic_CheckChanged"
                                          Checked="true" />
                                 </td>
                                 <td colspan="2">
                                  <asp:Label ID="lExistingReportLastSavedBy" runat="server" Text="" CssClass="lastSavedBy"></asp:Label>
                                 </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="textCell">
                           User Filter Type:<span class="asterisk">*</span>
                        </td>
                        <td class="inputCell">
                            <asp:RadioButtonList ID="rbUserFilterType" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rbUserFilterType_SelectedIndexChanged">
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                    <tr id="Tr3" runat="server">
                        <td class="textCell">
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                    <td>
                                        <span class="asterisk">*</span>Public Group:
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="ddlPublicFilter" runat="server"
                                            OnSelectedIndexChanged="ddlPublicFilter_SelectedIndexChanged" AutoPostBack="true" />
                                    </td>
                                    <td>
                                        <asp:Button ID="Button3" runat="server" Text="Modify" OnClick="Button3_Click" Visible="False" />
                                    </td>
                                    <td>
                                        <asp:Button ID="Button4" runat="server" Text="Delete" OnClick="Button4_Click" Visible="False"
                                            OnClientClick="return confirm('Are you sure?')" />
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td colspan="3">
                                        <asp:RequiredFieldValidator
                                                 ID="lPublicFilterLastSavedBy_RequiredFieldValidator" 
                                                 runat="server" 
                                                 ControlToValidate="ddlPublicFilter" 
                                                 InitialValue="" 
                                                 ErrorMessage="Public Filter is a required field."
                                                 ForeColor="Red"
                                                 Display="Dynamic"
                                                 Font-Size="10px"> 
                                        </asp:RequiredFieldValidator>  
                                        <asp:Label ID="lPublicFilterLastSavedBy" runat="server" Text="" CssClass="lastSavedBy"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr id="Tr4" runat="server">
                        <td class="textCell">
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                    <td>
                                        <span class="asterisk">*</span>Individual Contributor:
                                    </td>
                                    <td>
                                        <asp:Panel ID="panelSelectIcButton" runat="server">
                                            <asp:TextBox ID="tbDisplayName" runat="server" Width="133px" ReadOnly="true"></asp:TextBox>
                                            <button type="button" id="btnLinkSelectIc" onclick="$('.btnSelectIc').trigger('click');">
                                                Select</button>
                                            <br />
                                            <asp:RequiredFieldValidator ID="tbDisplayName_RequiredFieldValidator" runat="server" ControlToValidate="tbDisplayName"
                                                 ErrorMessage="Individual Contributor is a required field." ForeColor="Red" 
                                                 Display="Dynamic" Font-Size="10px"></asp:RequiredFieldValidator>
                                        </asp:Panel>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr id="Tr5" runat="server">
                        <td class="textCell">
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                    <td>
                                        <span class="asterisk">*</span>Activity Log Team:
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="ddlActivityLogTeam" runat="server" OnSelectedIndexChanged="ddlActivityLogTeam_SelectedIndexChanged"
                                            AutoPostBack="true" />
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td colspan="3">
                                        <asp:RequiredFieldValidator
                                                 ID="ddlActivityLogTeam_RequiredFieldValidator" 
                                                 runat="server" 
                                                 ControlToValidate="ddlActivityLogTeam" 
                                                 InitialValue="" 
                                                 ErrorMessage="Activity Log Team is a required field."
                                                 ForeColor="Red"
                                                 Display="Dynamic"
                                                 Font-Size="10px"> 
                                        </asp:RequiredFieldValidator>
                                    </td>
                               </tr>
                            </table>
                        </td>
                    </tr>
                    <tr id="Tr6" runat="server">
                        <td class="textCell">
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                    <td>
                                        <span class="asterisk">*</span>Private Group:
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="privateFilter" runat="server" DataTextField="Name" DataValueField="Name"
                                            OnSelectedIndexChanged="privateFilter_SelectedIndexChanged" AutoPostBack="true" />
                                    </td>
                                    <td>
                                        <asp:Button ID="Button17" runat="server" Text="Modify" OnClick="Button17_Click" Visible="False" />
                                    </td>
                                    <td>
                                        <asp:Button ID="Button18" runat="server" Text="Delete" OnClick="Button18_Click" Visible="False"
                                            OnClientClick="return confirm('Are you sure?')" />
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td colspan="3">
                                        <asp:RequiredFieldValidator
                                                 ID="privateFilter_RequiredFieldValidator" 
                                                 runat="server" 
                                                 ControlToValidate="privateFilter" 
                                                 InitialValue="" 
                                                 ErrorMessage="Private Filter is a required field."
                                                 ForeColor="Red"
                                                 Display="Dynamic"
                                                 Font-Size="10px"> 
                                        </asp:RequiredFieldValidator>
                                    </td>
                               </tr>
                            </table>
                        </td>
                    </tr>
                    <tr id="Tr7" runat="server">
                        <td class="textCell">
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                    <td>
                                        <span class="asterisk">*</span>Directorate:
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="directorate" runat="server" OnSelectedIndexChanged="directorate_SelectedIndexChanged"
                                            AutoPostBack="true" />
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td colspan="3">
                                        <asp:RequiredFieldValidator
                                                 ID="directorate_requiredFieldValidator" 
                                                 runat="server" 
                                                 ControlToValidate="directorate" 
                                                 InitialValue="" 
                                                 ErrorMessage="Directorate is a required field."
                                                 ForeColor="Red"
                                                 Display="Dynamic"
                                                 Font-Size="10px"> 
                                        </asp:RequiredFieldValidator>
                                    </td>
                               </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="textCell">Title:</td>
                        <td class="inputCell">
                         <table>
                          <tr>
                           <td colspan="2"><asp:TextBox ID="tbReportTitle" runat="server" Width="400px"></asp:TextBox></td>
                           <td>
                            <asp:PlaceHolder ID="phPublicChk" runat="server">
                             <asp:CheckBox ID="chkPublic" CssClass="cbPublicClass" runat="server" />&nbsp;Public
                            </asp:PlaceHolder>
                           </td>
                          </tr>
                          <tr>
                           <td valign="top">
                             <asp:Label ID="lblTitleReq" runat="server" ForeColor="Red" Font-Size="10px" Visible="false">
                                Title is a required when saving a report.
                            </asp:Label>
                           </td>
                           <td align="right">
                            <table>
                             <tr>
                              <td><asp:Button ID="btnExistingReportSave" runat="server" Text="Save" OnClick="btnExistingReportSave_Click" /></td>
                              <td><asp:Button ID="btnExistingReportSaveAs" runat="server" Text="Save As" OnClick="btnExistingReportSaveAs_Click" /></td>
                             </tr>
                            </table>
                           </td>
                          </tr>
                         </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="textCell">
                            Description:<span class="asterisk">&nbsp;</span>
                        </td>
                        <td class="inputCell">
                            <asp:TextBox ID="description" runat="server" Width="400px" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="textCell">
                            Report Type:<span class="asterisk">*</span>
                        </td>
                        <td class="inputCell">
                            <asp:DropDownList ID="ddlReportType" runat="server" DataTextField="Title" AutoPostBack="True"
                                OnSelectedIndexChanged="ddlReportType_SelectedIndexChanged" />
                        </td>
                    </tr>
                    <tr id="Tr1" runat="server">
                        <td class="textCell">
                            <asp:Label ID="Label5" runat="server" Text="Date Range:"></asp:Label>
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                    <td>
                                        <asp:Label ID="Label4" runat="server" Text="From Week:"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tbDateRangeFrom" runat="server" CssClass="drFrom" Width="100px" />
                                    </td>
                                    <td>
                                    </td>
                                    <td class="style6">
                                        <asp:Label ID="Label3" runat="server" Text="To Week:"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tbDateRangeTo" runat="server" CssClass="drTo" Width="100px" />
                                    </td>
                                    <td>
                                        <asp:Label ID="lDateRangeHelp" runat="server" Text="Date is week ending (Sunday).<br />Leave blank for last week." CssClass="lastSavedBy"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="5">
                                        <asp:CompareValidator ID="tbDateRangeFromDateTypeCompareValidator" runat="server" ControlToValidate="tbDateRangeFrom"
                                             Type="Date" Operator="DataTypeCheck" ErrorMessage="Please Enter a valid 'From Week' date with format mm/dd/yyyy.<br>"
                                             ForeColor="Red" Display="Dynamic" Font-Size="11px">
                                        </asp:CompareValidator>
                                        <asp:CompareValidator ID="tbDateRangeToDateTypeCompareValidator" runat="server" ControlToValidate="tbDateRangeTo"
                                             Type="Date" Operator="DataTypeCheck" ErrorMessage="Please Enter a valid 'To Week' date with format mm/dd/yyyy.<br>"
                                             ForeColor="Red" Display="Dynamic" Font-Size="11px">
                                        </asp:CompareValidator>

                                        <asp:CompareValidator ID="tbDateRangeToDateRangeCompareValidator" runat="server" ControlToCompare="tbDateRangeFrom"
                                             ControlToValidate="tbDateRangeTo" Display="Dynamic" ErrorMessage="'From Week' must be on or before 'To Week.'"
                                             Operator="GreaterThanEqual" Type="Date" ValueToCompare="<%= tbDateRangeTo.Text.ToShortString() %>"
                                             Font-Size="11px">
                                        </asp:CompareValidator>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr id="Tr2" runat="server">
                        <td class="textCell">
                            <asp:Label ID="Label9" runat="server" Text="Report Details:"></asp:Label>
                        </td>
                        <td class="inputCell">
                            <table>
                                <tr>
                                    <td style="border-collapse: collapse" valign="top" height="40">
                                        <asp:RadioButtonList ID="weeklyOrMonthly" runat="server" ToolTip="Weekly: Five weeks back from given date.  &#10;Monthly: Six months back from given date.">
                                            <asp:ListItem Selected="True">Weekly</asp:ListItem>
                                            <asp:ListItem>Monthly</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </td>
                                    <td style="width: 170px; border-collapse: collapse" valign="top">
                                        <asp:Label ID="Label12" runat="server" Text="Period End:&#10;"></asp:Label><br />
                                        <asp:TextBox ID="weekEnding" runat="server" Width="100px" />
                                        <br />
                                        <asp:Label ID="lReportDetailsHelp" runat="server" Text="Leave blank for last period." CssClass="lastSavedBy"></asp:Label>
                                    </td>
                                    <td rowspan="2" valign="top" id="tblBaseGoal" runat="server">
                                     <table>
                                      <tr>
                                       <td></td>
                                       <td>Base:</td>
                                       <td>Goal:</td>
                                      </tr>
                                      <tr>
                                       <td><asp:Label ID="lblBaseGoal" runat="server">Load:</asp:Label></td>
                                       <td>
                                        <asp:TextBox ID="percentBase" runat="server" Width="50px"></asp:TextBox><br />
                                        <asp:RangeValidator ID="baseValidator1" ControlToValidate="percentBase" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="baseValidator2" ControlToValidate="percentBase" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                       <td>
                                        <asp:TextBox ID="percentGoal" runat="server" Width="50px"></asp:TextBox><br />
                                        <asp:RangeValidator ID="goalValidator1" ControlToValidate="percentGoal" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="goalValidator2" ControlToValidate="percentGoal" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator> 
                                       </td>
                                     </tr>
                                     <asp:PlaceHolder ID="phBaseGoal" runat="server">
                                      <tr>
                                       <td>Barrier Time:</td>
                                       <td>
                                        <asp:TextBox ID="txtBarrierBase" runat="server" Width="50px" Text="0"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator1" ControlToValidate="txtBarrierBase" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtBarrierBase" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                       <td>
                                        <asp:TextBox ID="txtBarrierGoal" runat="server" Width="50px" Text="0"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator2" ControlToValidate="txtBarrierGoal" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtBarrierGoal" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                      </tr>
                                      <tr>
                                       <td>Plan Adherence:</td>
                                       <td>
                                        <asp:TextBox ID="txtAdhereBase" runat="server" Width="50px" Text="100"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator3" ControlToValidate="txtAdhereBase" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" ControlToValidate="txtAdhereBase" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                       <td>
                                        <asp:TextBox ID="txtAdhereGoal" runat="server" Width="50px" Text="100"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator4" ControlToValidate="txtAdhereGoal" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" ControlToValidate="txtAdhereGoal" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                      </tr>
                                      <tr>
                                       <td>Plan Attainment:</td>
                                       <td>
                                        <asp:TextBox ID="txtAttainBase" runat="server" Width="50px" Text="100"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator5" ControlToValidate="txtAttainBase" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" ControlToValidate="txtAttainBase" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                       <td>
                                        <asp:TextBox ID="txtAttainGoal" runat="server" Width="50px" Text="100"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator6" ControlToValidate="txtAttainGoal" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" ControlToValidate="txtAttainGoal" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                      </tr>
                                      <tr>
                                       <td>Productivity:</td>
                                       <td>
                                        <asp:TextBox ID="txtProdBase" runat="server" Width="50px" Text="100"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator7" ControlToValidate="txtProdBase" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" ControlToValidate="txtProdBase" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                       <td>
                                        <asp:TextBox ID="txtProdGoal" runat="server" Width="50px" Text="100"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator8" ControlToValidate="txtProdGoal" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" ControlToValidate="txtProdGoal" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                      </tr>
                                      <tr>
                                       <td>Unplanned:</td>
                                       <td>
                                        <asp:TextBox ID="txtUnplanBase" runat="server" Width="50px" Text="0"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator9" ControlToValidate="txtUnplanBase" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator9" ControlToValidate="txtUnplanBase" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                       <td>
                                        <asp:TextBox ID="txtUnplanGoal" runat="server" Width="50px" Text="0"></asp:TextBox><br />
                                        <asp:RangeValidator ID="RangeValidator10" ControlToValidate="txtUnplanGoal" Type="Integer" MinimumValue="0" MaximumValue="120"
                                            ErrorMessage="Must be between 0-120" ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RangeValidator>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator10" ControlToValidate="txtUnplanGoal" ErrorMessage="Can not be blank"
                                            ForeColor="Red" Display="Dynamic" Font-Size="11px" runat="server">
                                        </asp:RequiredFieldValidator>
                                       </td>
                                      </tr>
                                     </asp:PlaceHolder>
                                    </table>
                                   </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td valign="top">
                                        <asp:CompareValidator ID="weekEndingDateTypeCompareValidator" runat="server" ControlToValidate="weekEnding"
                                             Type="Date" Operator="DataTypeCheck" ErrorMessage="Please Enter a valid date with format mm/dd/yyyy."
                                             ForeColor="Red" Display="Dynamic" Font-Size="11px">
                                        </asp:CompareValidator>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
               </td>
              </tr>
              <tr>
                  <td>
                      <asp:Label ID="errLabel" runat="server" CssClass="errMsg" 
                          EnableViewState="False" Text="" Visible="false"></asp:Label>
                      <asp:Label ID="successLabel" runat="server" CssClass="successMsg" 
                          EnableViewState="False" Text="" Visible="false"></asp:Label>
                  </td>
                  <td align="right">
                      <table style="padding-top: 3px">
                          <tr>
                              <td>
                                  <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" 
                                      Text="Generate" />
                              </td>
                              <td>
                                  <asp:Button ID="Button12" runat="server" CausesValidation="false" 
                                      OnClick="Button12_Click" Text="Clear" />
                              </td>
                          </tr>
                      </table>
                  </td>
             </table>
            </asp:Panel>

            <asp:Panel ID="GroupFilterPanel" class="GroupFilterPanel" style="overflow: hidden; display: none" Visible="False" runat="server">
                <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
                <asp:Label ID="Label1" runat="server" Text="Label" Width="100%" BackColor="#CCCCCC"
                    ForeColor="#CC0000" BorderStyle="Solid"></asp:Label>
                <asp:HiddenField ID="selProfiles" runat="server" />
                <asp:HiddenField ID="selTeams" runat="server" />
                <asp:HiddenField ID="selDirs" runat="server" />
                <asp:HiddenField ID="selGroups" runat="server" />
                <table cellspacing="0" cellpadding="2">
                 <tr>
                  <td>
                   <table cellspacing="0" cellpadding="2">
                     <tr>
                        <td style="white-space: nowrap"><b>Group Name:</b><span class="asterisk">*</span></td>
                        <td><asp:TextBox ID="txtGroupName" Width="350" runat="server"></asp:TextBox></td>
                        <td style="white-space: nowrap"><asp:CheckBox ID="CheckBox1" runat="server" />&nbsp;Public</td>
                     </tr>
                     <tr>
                        <td valign="top"><b>Description:</b></td>
                        <td colspan="2" style="padding-bottom: 5px">
                            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Width="350" Rows="3"></asp:TextBox>
                        </td>
                     </tr>
                   </table>
                  </td>
                 </tr>
                 <tr>
                     <td>
                      <table cellspacing="0" cellpadding="2">
                       <tr>
                         <td colspan="3" style="border-style: solid; border-color: #999999; border-width: 1px 0px 0px 1px"></td>
                         <td colspan="3" style="border-style: solid; border-color: #999999; border-width: 1px 1px 0px 1px"></td>
                        </tr>
                        <tr>
                            <td style="border-left: 1px solid #999999; padding-left: 5px" rowspan="3" valign="top"><b>Teams:</b></td>
                            <td>
                             <asp:DropDownList ID="ddGroupTeams" CssClass="ddGroupTeams" Width="300" style="font-size: 8.5pt; height: 20px" runat="server"></asp:DropDownList>
                            </td>
                            <td>
                                <input type="button" value="Add" onclick="GroupAdd('Team')" />
                            </td>
                            <td style="border-left: 1px solid #999999; padding-left: 5px" rowspan="3" valign="top"><b>Directorates:</b></td>
                            <td>
                             <asp:DropDownList ID="ddGroupDirs" CssClass="ddGroupDirs" Width="300" style="font-size: 8.5pt; height: 20px" runat="server"></asp:DropDownList>
                            </td>
                            <td style="border-right: 1px solid #999999">
                                <input type="button" value="Add" onclick="GroupAdd('Dir')" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                             <asp:ListBox ID="lbGroupTeams" CssClass="lbGroupTeams" SelectionMode="Multiple" style="font-size: 8.5pt" Width="300" Rows="8" runat="server">
                             </asp:ListBox>
                            </td>
                            <td colspan="2" style="border-right: 1px solid #999999">
                             <asp:ListBox ID="lbGroupDirs" CssClass="lbGroupDirs" SelectionMode="Multiple" style="font-size: 8.5pt" Width="300" Rows="8" runat="server">
                             </asp:ListBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <input type="button" value="Remove" onclick="GroupRemove('Team')" />
                            </td>
                            <td colspan="2" style="border-right: 1px solid #999999">
                                <input type="button" value="Remove" onclick="GroupRemove('Dir')" />
                            </td>
                        </tr>
                        <tr>
                         <td colspan="3" style="border-style: solid; border-color: #999999; border-width: 1px 0px 0px 1px"></td>
                         <td colspan="3" style="border-style: solid; border-color: #999999; border-width: 1px 1px 0px 1px"></td>
                        </tr>
                        <tr>
                            <td style="border-left: 1px solid #999999; padding-left: 5px" rowspan="3" valign="top"><b>Groups:</b></td>
                            <td id="tdGroupGroupsDd" runat="server">
                             <asp:DropDownList ID="ddGroupGroups" CssClass="ddGroupGroups" Width="300" style="font-size: 8.5pt; height: 20px" runat="server"></asp:DropDownList>
                            </td>
                            <td id="tdGroupGroupsAddBtn" runat="server">
                                <input type="button" value="Add" onclick="GroupAdd('Group')" />
                            </td>
                            <td colspan="2" id="tdGroupGroupsDdHidden" runat="server"></td>
                            <td style="border-left: 1px solid #999999; padding-left: 5px" rowspan="3" valign="top"><b>Individuals:</b></td>
                            <td>
                             <asp:TextBox ID="txtGroupProfile" CssClass="txtGroupProfile" Width="294" runat="server"></asp:TextBox>
                            </td>
                            <td style="border-right: 1px solid #999999">
                                <img src="/css/images/search-icon.png" alt="" onclick="$('.txtGroupProfile').autocomplete('search')" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" id="tdGroupGroupsLb" runat="server">
                             <asp:ListBox ID="lbGroupGroups" CssClass="lbGroupGroups" SelectionMode="Multiple" style="font-size: 8.5pt" Width="300" Rows="8" runat="server">
                             </asp:ListBox>
                            </td>
                            <td id="tdGroupGroupsLbHidden1" style="border-width:1px; border-style:solid; border-color:#999999; text-align:center;" runat="server">
                              <p style="color:Gray;">This group is contained in another group.<br />Can not add group(s) to this group.</p>
                            </td>
                            <td id="tdGroupGroupsLbHidden2" runat="server"></td>
                            <td colspan="2" style="border-right: 1px solid #999999">
                             <asp:ListBox ID="lbGroupProfiles" CssClass="lbGroupProfiles" SelectionMode="Multiple" style="font-size: 8.5pt" runat="server" Width="300" Rows="8">
                             </asp:ListBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" id="tdGroupGroupsRemoveBtn" runat="server">
                                <input type="button" value="Remove" onclick="GroupRemove('Group')" />
                            </td>
                            <td colspan="2" id="tdGroupGroupsRemoveBtnHidden" runat="server"></td>
                            <td colspan="2" style="border-right: 1px solid #999999">
                                <input type="button" value="Remove" onclick="GroupRemove('Profile')" />
                            </td>
                        </tr>
                        <tr><td colspan="6" style="border-top: 1px solid #999999"></td></tr>
                       </table>
                     </td>
                    </tr>
                    <tr>
                     <td align="right">
                      <asp:Button ID="btnGroupSave1" runat="server" CausesValidation="false" Text="Save"
                        OnClientClick="if(!SaveGroup(this.id))return false" OnClick="Button5_Click" UseSubmitBehavior="false" />&nbsp;
                      <asp:Button ID="btnGroupSave2" runat="server" CausesValidation="false" Text="Save As"
                        OnClientClick="if(!SaveGroup(this.id))return false" OnClick="Button19_Click" UseSubmitBehavior="false" />&nbsp;
                      <input type="button" value="Cancel" onclick="$('.GroupFilterPanel').dialog('close')" />
                     </td>
                    </tr>
                </table>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <%--Add Individual Contributor Popup --%>
    <asp:Button runat="server" CssClass="btnSelectIc" ID="btnSelectIc" CausesValidation="false" Text="Add" Style="display: none" />
    <asp:TextBox runat="server" ID="tbUserName" Style="display: none" />

    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddBackups" CancelControlID="btnAddBackupCancel"
        TargetControlID="btnSelectIc" PopupControlID="panelAddBackupsPopup" PopupDragHandleControlID="divAddBackupsPopupHeader"
        Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>

    <asp:Panel runat="server" ID="panelAddBackupsPopup" DefaultButton="btnDoNothingBackups" Style="display: none">
        <div class="AddUsersPopup">
            <div class="PopupHeader" id="divAddBackupsPopupHeader">
                Select Individual Contributor</div>
            <div class="PopupBody">
                <asp:ProfileSelector runat="server" ID="profileSelectorBackups" IncludePlanExempt="false" AllowMultiple="false" />
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveBackupsPopup" CausesValidation="false" Text="Save" OnClick="btnSaveIc_Click" />
                <input id="btnAddBackupCancel" type="button" value="Cancel" />
                <asp:Button runat="server" ID="btnDoNothingBackups" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
