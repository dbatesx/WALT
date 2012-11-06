<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Directorates.aspx.cs" Inherits="WALT.UIL.Admin.Directorates"
    EnableEventValidation="false" MasterPageFile="~/Site1.Master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="AjaxControlToolkit, Version=3.5.51116.0, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e"
    Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register TagPrefix="asp" TagName="ProfileSelector" Src="~/Controls/ProfileSelector.ascx" %>
<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<% if (WALT.BLL.ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.DIRECTORATE_ADMIN))
   { %>
    <asp:UpdateProgress runat="server" ID="updateProgress1" AssociatedUpdatePanelID="upanel1">
        <ProgressTemplate>
            <div id="progressBackgroundFilter">
            </div>
            <div id="processMessage" >
                <img alt="Loading..." src="/css/images/ajax-loader.gif" />
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:Panel runat="server" ID="panelDirectoratesPage" DefaultButton="btnDoNothingDirectorates">
    <div style="font-size: 16pt; font-weight: bold">Admin - Directorates</div><br />
    <asp:UpdatePanel runat="server" ID="upanel1">
        <ContentTemplate>
            <table>
                <tr>
                    <td style="vertical-align: top">
                        <b>Directorates:</b>
                        <br />
                        <asp:ListBox runat="server" ID="lstDirectorates" Height="300" Width="200" OnSelectedIndexChanged="lstDirectorates_SelectedIndexChanged"
                            AutoPostBack="true"></asp:ListBox>
                        <br />
                        <asp:Panel runat="server" ID="panelDirectoratesButtons">
                            <button type="button" id="btnLinkAddDirectorate" onclick="$('.btnAddDirectorate').trigger('click');">
                                Add</button>
                            <asp:Panel runat="server" ID="panelEditDirectorateButton" Visible="false" style="display:inline;">
                                <button type="button" id="btnLinkEditDirectorate" onclick="$('.btnEditDirectorate').trigger('click');">
                                    Edit</button>
                            </asp:Panel>
                        </asp:Panel>
                    </td>
                    <td style="vertical-align: top">
                        <b>Org Codes:</b>
                        <br />
                        <asp:ListBox runat="server" ID="lstOrgCodes" Height="300" Width="120" OnSelectedIndexChanged="lstOrgCodes_SelectedIndexChanged" AutoPostBack="true"></asp:ListBox>
                        <br />
                        <asp:Panel runat="server" ID="panelOrgCodeButtons" Visible="false">
                            <button type="button" id="btnLinkAddOrgCode" onclick="$('.btnAddOrgCodes').trigger('click');">
                                Add</button>
                            <asp:Button runat="server" ID="btnRemoveOrgCode" Text="Remove" OnClick="btnRemoveOrgCode_Click" Visible="false" />
                        </asp:Panel>
                    </td>
                    <td style="vertical-align: top">
                        <b>Members:</b>
                        <br />
                        <asp:ListBox runat="server" ID="lstMembers" SelectionMode="Multiple" Height="300" Width="175"></asp:ListBox>
                        <br />
                        <asp:PlaceHolder ID="phMemberBtns" Visible="false" runat="server">
                         <asp:Button ID="btnRequired" Text="Set Plan Required" OnClick="btnSetExempt_Click" style="width: 175px" runat="server" /><br />
                         <asp:Button ID="btnExempt" Text="Set Plan Not Required" OnClick="btnSetExempt_Click" style="width: 175px" runat="server" />
                        </asp:PlaceHolder>
                        <div style="font-size:x-small;">X = Plan not required<br />* = not an ALT member</div>
                    </td>
                    <td style="vertical-align: top">
                        <b>Directorate Admins:</b>
                        <br />
                        <asp:ListBox runat="server" ID="lstAdmins" SelectionMode="Multiple" Height="300"
                            Width="175" OnSelectedIndexChanged="lstAdmins_SelectedIndexChanged" AutoPostBack="true"></asp:ListBox>
                        <br />
                        <asp:Panel runat="server" ID="panelAdministratorsButtons" Visible="false">
                            <button type="button" id="btnLinkAddDA" onclick="$('.btnAddDA').trigger('click');">
                                Add</button>
                            <asp:Button runat="server" ID="btnRemoveAdmins" OnClick="btnRemoveAdmins_Click" Text="Remove" Visible="false" />
                        </asp:Panel>
                    </td>
                    <td style="vertical-align: top">
                        <b>Directorate Mgrs:</b>
                        <br />
                        <asp:ListBox runat="server" ID="lstManagers" SelectionMode="Multiple" Height="300"
                            Width="175" OnSelectedIndexChanged="lstManagers_SelectedIndexChanged" AutoPostBack="true">
                        </asp:ListBox>
                        <br />
                        <asp:Panel runat="server" ID="panelManagersButtons" Visible="false">
                            <button type="button" id="btnLinkAddDM" onclick="$('.btnAddDM').trigger('click');">
                                Add</button>
                            <asp:Button runat="server" ID="btnRemoveManagers" OnClick="btnRemoveManagers_Click" Text="Remove" Visible="false" />
                        </asp:Panel>
                    </td>
                    <td style="vertical-align: top">
                        <b>Activity Log Teams:</b>
                        <br />
                        <asp:ListBox runat="server" ID="lstTeams" Height="300" Width="160"></asp:ListBox>
                        <br />
                        <asp:Panel runat="server" ID="panelAddTeamButton" Visible="false">
                            <button type="button" id="btnLinkAddTeam" onclick="$('.btnAddTeam').trigger('click');">
                                Add</button>
                        </asp:Panel>
                    </td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>

    <%--Add Directorate Popup--%>
    <asp:Button runat="server" CssClass="btnAddDirectorate" ID="btnAddDirectorate" Text="Add"
        Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddDirectorate" 
        TargetControlID="btnAddDirectorate" PopupControlID="panelAddDirectoratePopup"
        PopupDragHandleControlID="divAddDirectoratePopupHeader" Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelAddDirectoratePopup" DefaultButton="btnDoNothingAddDirectorate"
        style="display: none">
        <div class="AddDirectoratePopup">
            <div class="PopupHeader" id="divAddDirectoratePopupHeader">
                Add Directorate</div>
            <div class="PopupBody">
                
                <br />
                <table>
                    <tr>
                        <td style="width: 100px; vertical-align: top;">
                            Name:
                        </td>
                        <td style="vertical-align: top;">
                            <asp:TextBox runat="server" ID="txtNewDirectorate" Width="150px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 100px; vertical-align: top;">
                            Org Codes:
                        </td>
                        <td style="vertical-align: top;">
                            <asp:UpdatePanel runat="server" ID="upanelNewDirectorateOrgCode">
                                <ContentTemplate>
                                    <asp:TextBox runat="server" ID="txtNewDirectorateOrgCode"></asp:TextBox>
                                    <asp:Button runat="server" ID="btnAddDirectorateOrgCode" Text="Add" OnClick="btnAddDirectorateOrgCode_Click" />
                                    <asp:Button runat="server" ID="btnRemoveDirectorateOrgCode" Text="Remove" OnClick="btnRemoveDirectorateOrgCode_Click" />
                                    <br />
                                    <asp:ListBox runat="server" ID="lstNewDirectorateOrgCodes" Width="155px"></asp:ListBox>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 100px; vertical-align: top;">
                            Admin(s):
                        </td>
                        <td style="vertical-align: top;">
                            <asp:ProfileSelector runat="server" ID="profileSelectorDA" DirectorateAdminsOnly="true" IncludePlanExempt="true"></asp:ProfileSelector>
                        </td>
                    </tr>
                </table>
                <br />
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveAddDirectorate" Text="Save" OnClick="btnSaveDirectorate_Click" />
                <asp:Button runat="server" ID="btnCancelAddDirectorate" Text="Cancel" OnClick="btnCancelAddDirectorate_Click" />
                <asp:Button runat="server" ID="btnDoNothingAddDirectorate" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
    <%--Edit Directorate Popup--%>
    <asp:Button runat="server" CssClass="btnEditDirectorate" ID="btnEditDirectorate"
        Text="Add" Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderEditDirectorate"
        TargetControlID="btnEditDirectorate" PopupControlID="panelEditDirectoratePopup"
        PopupDragHandleControlID="divEditDirectoratePopupHeader" Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelEditDirectoratePopup" DefaultButton="btnDoNothingEditDirectorate"
        style="display: none">
        <div class="EditDirectoratePopup">
            <div class="PopupHeader" id="divEditDirectoratePopupHeader">
                Edit Directorate</div>
            <div class="PopupBody">
                <asp:UpdatePanel runat="server" ID="upanelEditDirectoratePopup">
                    <ContentTemplate>
                        <br />
                        Name:
                        <br />
                        <asp:TextBox runat="server" ID="txtEditDirectorateName" Width="150px"></asp:TextBox>
                        <br />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveEditDirectorate" Text="Save" OnClick="btnSaveDirectorate_Click" />
                <asp:Button runat="server" ID="btnCancelEditDirectorate" Text="Cancel" OnClick="btnCancelEditDirectorate_Click" />
                <asp:Button runat="server" ID="btnDoNothingEditDirectorate" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
    <%--Add Directorate Administrators Popup--%>
    <asp:Button runat="server" CssClass="btnAddDA" ID="btnAddDA" Text="Add" Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddDA" 
        TargetControlID="btnAddDA" PopupControlID="panelAddDAPopup" PopupDragHandleControlID="divAddDAPopupHeader"
        Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelAddDAPopup" DefaultButton="btnDoNothingAddDA"
        style="display: none">
        <div class="AddDAPopup">
            <div class="PopupHeader" id="div1">
                Add Directorate Administrators</div>
            <div class="PopupBody">
                <br />
                <asp:ProfileSelector runat="server" ID="profileSelectorAddDA" DirectorateAdminsOnly="true"
                    IncludePlanExempt="true" AllowMultiple="true"></asp:ProfileSelector>
                <br />
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveDA" Text="Save" OnClick="btnSaveDA_Click" />
                <asp:Button runat="server" ID="btnCancelAddDA" Text="Cancel" OnClick="btnCancelAddDA_Click" />
                <asp:Button runat="server" ID="btnDoNothingAddDA" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
    <%--Add Directorate Org Codes Popup--%>
    <asp:Button runat="server" CssClass="btnAddOrgCodes" ID="btnAddOrgCodes" Text="Add"
        Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddOrgCodes" 
        TargetControlID="btnAddOrgCodes" PopupControlID="panelAddOrgCodesPopup" PopupDragHandleControlID="divAddOrgCodesPopupHeader"
        Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelAddOrgCodesPopup" DefaultButton="btnDoNothingAddOrgCodes"
        Style="display: none">
        <div class="AddOrgCodesPopup">
            <div class="PopupHeader" id="divAddOrgCodesPopupHeader">
                Add Organization Codes</div>
            <div class="PopupBody">
                <br />
                <asp:UpdatePanel runat="server" ID="upanelAddOrgCodes">
                    <ContentTemplate>
                        <table>
                            <tr>
                                <td>
                                    <asp:TextBox runat="server" ID="txtNewOrgCode"></asp:TextBox>
                                    <br />
                                    <asp:ListBox runat="server" ID="lstNewOrgCodes" Width="155px"></asp:ListBox>
                                </td>
                                <td style="vertical-align:top">
                                    <asp:Button runat="server" ID="btnAddOrgCodeToList" Text="Add" OnClick="btnAddOrgCodeToList_Click" />
                                    <br />
                                    <asp:Button runat="server" ID="btnRemoveOrgCodeFromList" Text="Remove" OnClick="btnRemoveOrgCodeFromList_Click" />
                                </td>
                            </tr>
                        </table>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <br />
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveOrgCodes" Text="Save" OnClick="btnSaveOrgCodes_Click" />
                <asp:Button runat="server" ID="btnCancelAddOrgCodes" Text="Cancel" OnClick="btnCancelAddOrgCodes_Click" />
                <asp:Button runat="server" ID="btnDoNothingAddOrgCodes" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
    <%--Add Directorate Managers Popup--%>
    <asp:Button runat="server" CssClass="btnAddDM" ID="btnAddDM" Text="Add" Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddDM" 
        TargetControlID="btnAddDM" PopupControlID="panelAddDMPopup" PopupDragHandleControlID="divAddDMPopupHeader"
        Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelAddDMPopup" DefaultButton="btnDoNothingAddDM" Style="display: none">
        <div class="AddDMPopup">
            <div class="PopupHeader" id="divAddDMPopupHeader">
                Add Directorate Managers</div>
            <div class="PopupBody">
                <br />
                <asp:ProfileSelector runat="server" ID="profileSelectorAddDM" DirectorateManagersOnly="true"
                    IncludePlanExempt="true"></asp:ProfileSelector>
                <br />
            </div>
            <div class="Controls">
                <asp:Button runat="server" ID="btnSaveDM" Text="Save" OnClick="btnSaveDM_Click" />
                <asp:Button runat="server" ID="btnCancelAddDM" Text="Cancel" OnClick="btnCancelAddDM_Click" />
                <asp:Button runat="server" ID="btnDoNothingAddDM" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
    <%--Add Team Popup--%>
    <asp:Button runat="server" CssClass="btnAddTeam" ID="btnAddTeam" Text="Add" Style="display: none" />
    <asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddTeam" 
        TargetControlID="btnAddTeam" PopupControlID="panelAddTeamPopup" PopupDragHandleControlID="divAddTeamPopupHeader"
        Drag="true" BackgroundCssClass="ModalPopupBG">
    </asp:ModalPopupExtender>
    <asp:Panel runat="server" ID="panelAddTeamPopup" DefaultButton="btnDoNothingAddTeam"
        style="display: none">
        <div class="AddTeamPopup">
            <div class="PopupHeader" id="divAddTeamPopupHeader">
                Add Team</div>
            <div class="PopupBody">
                <asp:UpdatePanel runat="server" ID="upanelAddTeamPopup">
                    <ContentTemplate>
                     <table cellpadding="3" cellspacing="0">
                      <tr>
                       <td>
                        <asp:Label runat="server" ID="lblTeamNameToAdd" Text="Team Name:" Font-Bold="true" AssociatedControlID="txtTeamNameToAdd"></asp:Label>
                       </td>
                       <td><asp:TextBox runat="server" ID="txtTeamNameToAdd" Width="300"></asp:TextBox></td>
                      </tr>
                      <tr>
                       <td>
                        <asp:Label runat="server" ID="lblTeamManagerToAdd" Text="Team Manager:" Font-Bold="true" AssociatedControlID="ddlTeamManagerToAdd"></asp:Label>
                       </td>
                       <td><asp:DropDownList runat="server" ID="ddlTeamManagerToAdd"></asp:DropDownList></td>
                      </tr>
                      <tr>
                       <td><b>Use All Barriers:</b></td>
                       <td><asp:CheckBox ID="chkApplyBarriers" Checked="true" runat="server" /></td>
                      </tr>
                      <tr>
                       <td><b>Use All Unplanned Codes:</b></td>
                       <td><asp:CheckBox ID="chkApplyUnplanned" Checked="true" runat="server" /></td>
                      </tr>
                      <tr>
                       <td><b>Use All Task Types:</b></td>
                       <td><asp:CheckBox ID="chkApplyTaskTypes" Checked="true" runat="server" /></td>
                      </tr>
                     </table>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="Controls" style="text-align: right">
                <asp:Button runat="server" ID="btnSaveAddTeam" Text="Save" OnClick="btnSaveTeam_Click" />
                <asp:Button runat="server" ID="btnCancelAddTeam" Text="Cancel" OnClick="btnCancelAddTeam_Click" />
                <asp:Button runat="server" ID="btnDoNothingAddTeam" Enabled="false" Style="display: none" />
            </div>
        </div>
    </asp:Panel>
    <asp:Button runat="server" ID="btnDoNothingDirectorates" style="visibility:hidden;" />
    </asp:Panel>
    <% } %>
</asp:Content>
