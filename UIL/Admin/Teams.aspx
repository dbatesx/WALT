<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Teams.aspx.cs" Inherits="WALT.UIL.Admin.Teams"
    EnableEventValidation="false" MasterPageFile="~/Site1.Master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="AjaxControlToolkit, Version=3.5.51116.0, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e"
    Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register TagPrefix="asp" TagName="ProfileSelector" Src="~/Controls/ProfileSelector.ascx" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <% if (WALT.BLL.ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
   { %><%--
    <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" EnablePartialRendering="true">
    </asp:ToolkitScriptManager>--%>

    <asp:UpdateProgress runat="server" ID="updateProgress1" AssociatedUpdatePanelID="upanelTeams">
        <ProgressTemplate>
            <div id="progressBackgroundFilter">
            </div>
            <div id="processMessage">
                <img alt="Loading..." src="/css/images/ajax-loader.gif" />
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel runat="server" ID="upanelTeams">
        <ContentTemplate>
            <div style="font-size: 16pt; font-weight: bold">Admin - Teams</div><br />
            <table cellpadding="10px">
                <tr valign="top">
                    <td>
                        <b>Directorate:</b>&nbsp
                        <asp:DropDownList ID="ddlDirectorate" runat="server" OnSelectedIndexChanged="ddlDirectorate_SelectedIndexChanged"
                            AutoPostBack="True">
                        </asp:DropDownList>
                    </td>
                    <td rowspan="2">
                        <asp:Panel runat="server" ID="panelTeamData" Enabled="false">
                            <table width="500px" cellpadding="3px" style="border: 1px solid #cccccc; background: #eeeeee">
                                <tr>
                                    <td width="85"><b>Name:</b></td>
                                    <td width="415">
                                        <asp:TextBox ID="tbEditTeamName" runat="server"></asp:TextBox>
                                        <asp:Button runat="server" ID="btnSaveTeamName" OnClick="btnSaveTeamName_Click" Text="Save" Visible="false" />
                                    </td>
                                </tr>
                                <tr>
                                    <td><b>AL Manager:</b></td>
                                    <td>
                                        <asp:DropDownList ID="ddlManager" runat="server" AutoPostBack="True"
                                            DataTextField="DisplayName" OnSelectedIndexChanged="ddlManager_SelectedIndexChanged">
                                        </asp:DropDownList>
                                        <asp:TextBox runat="server" ID="txtALManagerRO" ReadOnly="true" Visible="false"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top"><b>R/E Type:</b></td>
                                    <td>
                                        <asp:RadioButton GroupName="RE" ID="rbComplexityBased" runat="server" Text="Complexity Based"
                                            AutoPostBack="true" OnCheckedChanged="rbComplexityBased_CheckedChanged" />
                                        <br />
                                        <asp:RadioButton GroupName="RE" ID="rbHandEntered" runat="server" Text="Hand Entered"
                                            AutoPostBack="true" OnCheckedChanged="rbHandEntered_CheckedChanged" />
                                    </td>
                                </tr>
                                <tr>
                                 <td><b>Active:</b></td>
                                 <td>
                                    <asp:CheckBox ID="cbActive" runat="server" AutoPostBack="True" OnCheckedChanged="cbActive_CheckedChanged" />
                                 </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                     <table>
                                      <tr>
                                       <td>
                                        <b>Backup AL Managers:</b>
                                        <asp:ListBox ID="lbBackups" runat="server" SelectionMode="Multiple" DataTextField="DisplayName"
                                            Rows="10" Width="200px"></asp:ListBox>
                                        <br />
                                        
                                        <asp:Panel runat="server" ID="panelAddBackupButton" Visible="false">
                                            <button type="button" id="btnLinkAddBackup" onclick="$('.btnAddBackup').trigger('click');">
                                                Add</button>
                                            <asp:Button ID="btnRemoveBackups" runat="server" Text="Remove" OnClick="btnRemoveBackups_Click" />
                                        </asp:Panel>
                                        
                                    
                                    </td>
                                    <td>
                                        <b>Team Members:</b>
                                        <asp:ListBox ID="lbMembers" runat="server" SelectionMode="Multiple" DataTextField="DisplayName"
                                            Rows="10" Width="200px"></asp:ListBox>
                                        <br />
                                        <asp:Panel runat="server" ID="panelAddMemberButton" Visible="false">
                                            <button type="button" id="btnLinkAddMember" onclick="$('.btnAddMember').trigger('click');">
                                                Add</button>
                                            <asp:Button ID="btnRemoveMembers" runat="server" Text="Remove" OnClick="btnRemoveMembers_Click" />
                                        </asp:Panel>
                                        
                                    
                                    </td>
                                   </tr>
                                  </table>
                                 </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td width="400px" valign="top">
                        <asp:GridView ID="gridTeams" runat="server" AllowPaging="True" AutoGenerateSelectButton="True"
                            CellPadding="4" ForeColor="#333333" GridLines="None" OnPageIndexChanging="gridTeams_PageIndexChanging"
                            OnRowCancelingEdit="gridTeams_RowCancelingEdit" OnRowDeleting="gridTeams_RowDeleting"
                            OnRowEditing="gridTeams_RowEditing" OnRowUpdating="gridTeams_RowUpdating" OnSelectedIndexChanged="gridTeams_SelectedIndexChanged"
                            AutoGenerateColumns="False" EmptyDataText="Directorate has no teams">
                            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                            <Columns>
                                <asp:TemplateField HeaderText="Name">                                   
                                    <ItemTemplate>
                                        <asp:Label ID="Label1" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                                        <asp:HiddenField ID="hfdTeamId" runat="server" Value='<%# GetId(Container.DataItem) %>' />
                                        
                                        
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:CheckBoxField ItemStyle-HorizontalAlign="Center" DataField="Active" 
                                    HeaderText="Active" >
                                <ItemStyle HorizontalAlign="Center" />
                                </asp:CheckBoxField>
                                <asp:CheckBoxField ItemStyle-HorizontalAlign="Center" DataField="ComplexityBased"
                                    HeaderText="Complexity Based" >
                                <ItemStyle HorizontalAlign="Center" />
                                </asp:CheckBoxField>
                            </Columns>
                            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                            <EditRowStyle BackColor="#999999" />
                            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                        </asp:GridView>
                    </td>
                </tr>
            </table>

        </ContentTemplate>
    </asp:UpdatePanel>

    <%--Add Backup AL Managers Popup--%>
    <asp:Button runat="server" CssClass="btnAddBackup" ID="btnAddBackup" Text="Add" Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddBackups"
        TargetControlID="btnAddBackup" PopupControlID="panelAddBackupsPopup" PopupDragHandleControlID="divAddBackupsPopupHeader"
        Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelAddBackupsPopup" DefaultButton="btnDoNothingBackups"
        Style="display: none">
        <div class="AddUsersPopup">
            <div class="PopupHeader" id="divAddBackupsPopupHeader">
                Add Backup AL Managers</div>
            <div class="PopupBody">
                <asp:ProfileSelector runat="server" ID="profileSelectorBackups" IncludePlanExempt="false" AllowMultiple="true" />
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveBackupsPopup" Text="Save" OnClick="btnSaveBackups_Click" />
                <asp:Button runat="server" ID="btnAddBackupCancel" Text="Cancel" OnClick="btnAddBackupCancel_Click" />               
                <asp:Button runat="server" ID="btnDoNothingBackups" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>

    <%--Add Team Members Popup--%>
    <asp:Button runat="server" CssClass="btnAddMember" ID="btnAddMember" Text="Add" Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddMembers"
        TargetControlID="btnAddMember" PopupControlID="panelAddMembersPopup" PopupDragHandleControlID="divAddMembersPopupHeader"
         Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelAddMembersPopup" DefaultButton="btnDoNothingMembers"
        Style="display: none">
        <div class="AddUsersPopup">
            <div class="PopupHeader" id="divAddMembersPopupHeader">
                Add Team Members</div>
            <div class="PopupBody">
                <asp:ProfileSelector runat="server" ID="profileSelectorMembers" IncludePlanExempt="false" />
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveMembersPopup" Text="Save" OnClick="btnSaveMembers_Click" />
                <asp:Button runat="server" ID="btnAddMemberCancel" Text="Cancel" OnClick="btnAddMemberCancel_Click" />                
                <asp:Button runat="server" ID="btnDoNothingMembers" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
    <% } %>
</asp:Content>
