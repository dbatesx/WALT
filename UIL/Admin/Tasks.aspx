<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tasks.aspx.cs" Inherits="WALT.UIL.Admin.Tasks"
    MasterPageFile="~/Site1.Master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register TagPrefix="task" TagName="TaskBarriers" Src="~/Controls/TaskBarriers.ascx" %>
<%@ Register TagPrefix="task" TagName="TaskUnplannedCodes" Src="~/Controls/TaskUnplannedCodes.ascx" %>
<%@ Register TagPrefix="task" TagName="TaskTypes" Src="~/Controls/TaskTypes.ascx" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeaderSubContent" runat="server">
    <style type="text/css">
        input:not([type='checkbox']) 
        {
            border:1px solid gray;
        }
    
    </style>
</asp:Content>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <% if (WALT.BLL.ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE)
           || WALT.BLL.ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
       { %>

    <script type="text/javascript">
        function postBackByObject() {
            var o = window.event.srcElement;
            if (o.tagName == "INPUT" && o.type == "checkbox") {
                __doPostBack("<%=upanelTasks.UniqueID %>", "");
            }
        }
    </script>

    <asp:UpdateProgress runat="server" ID="updateProgress1" AssociatedUpdatePanelID="upanelTasks" >
        <ProgressTemplate>
            <div id="progressBackgroundFilter">
            </div>
            <div id="processMessage">
                <img alt="Loading..." src="/css/images/ajax-loader.gif" />
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel runat="server" ID="upanelTasks">
        <ContentTemplate>
            <div style="font-size: 16pt"><b>Admin - Tasks</b></div><br />
            <table style="border-collapse: collapse;">
                <tr>
                 <td>
                  <table cellpadding="3">
                   <tr>
                    <td>
                        <asp:Label runat="server" ID="lblDirectorate" Text="<b>Directorate:</b>" AssociatedControlID="ddlDirectorate"></asp:Label>
                    </td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlDirectorate" OnSelectedIndexChanged="ddlDirectorate_SelectedIndexChanged"
                            AutoPostBack="true"></asp:DropDownList>
                    </td>
                   </tr>
                   <tr>
                    <td>
                        <asp:Label runat="server" ID="lblTeam" Text="<b>Team:</b>" AssociatedControlID="ddlTeam" Visible="false"></asp:Label>
                    </td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlTeam" OnSelectedIndexChanged="ddlTeam_SelectedIndexChanged"
                            AutoPostBack="true" Visible="false">
                        </asp:DropDownList>
                    </td>
                   </tr>
                  </table>
                 </td>
                </tr>
                <tr>
                    <td style="width: 700px; padding-top: 20px;">
                        <asp:TabContainer ID="TabContainer1" runat="server" AutoPostBack="true" OnActiveTabChanged="TabContainer1_ActiveTabChanged" ActiveTabIndex="0">
                            <asp:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                <HeaderTemplate>
                                    Barriers
                                </HeaderTemplate>
                                <ContentTemplate>
                                    <task:TaskBarriers runat="server" ID="controlTaskBarriers" />
                                </ContentTemplate>
                            </asp:TabPanel>
                            <asp:TabPanel ID="TabPanel2" runat="server" HeaderText="TabPanel2">
                                <HeaderTemplate>
                                    Unplanned Codes
                                </HeaderTemplate>
                                <ContentTemplate>
                                    <task:TaskUnplannedCodes runat="server" ID="controlTaskUnplannedCodes" />
                                </ContentTemplate>
                            </asp:TabPanel>
                            <asp:TabPanel ID="TabPanel3" runat="server" HeaderText="TabPanel3">
                                <HeaderTemplate>
                                    Task Types
                                </HeaderTemplate>
                                <ContentTemplate>
                                    <task:TaskTypes runat="server" ID="controlTaskTypes" />
                                </ContentTemplate>
                            </asp:TabPanel>
                        </asp:TabContainer>
                    </td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
    <% } %>
</asp:Content>
