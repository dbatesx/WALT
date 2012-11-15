<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Profiles.aspx.cs" Inherits="WALT.UIL.Admin.Profiles"
	MasterPageFile="~/Site1.Master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register TagPrefix="asp" TagName="ProfileSelector" Src="~/Controls/ProfileSelector.ascx" %>
<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<% if (WALT.BLL.ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.PROFILE_MANAGE))
	   { %><%--
	<asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" EnablePartialRendering="true">
	</asp:ToolkitScriptManager>--%>
	<asp:UpdateProgress runat="server" ID="updateProgress1" AssociatedUpdatePanelID="upanelProfiles">
		<ProgressTemplate>
			<div id="progressBackgroundFilter">
			</div>
			<div id="processMessage">
				<img alt="Loading..." src="/css/images/ajax-loader.gif" />
			</div>
		</ProgressTemplate>
	</asp:UpdateProgress>
	<asp:UpdatePanel runat="server" ID="upanelProfiles">
		<ContentTemplate>
			<div style="font-size: 16pt; font-weight: bold">Admin - Profiles</div><br />
			
			 <asp:Panel runat="server" ID="panelProfileTools" DefaultButton="btnAddUser">
			  <table cellpadding="2" cellspacing="0">
			   <tr><td valign="top">
				<table><tr><td>
					 <asp:PlaceHolder ID="phSync" runat="server">
						<asp:Button ID="btnSynchAD" runat="server" Text="Synchronize with AD" 
							OnClick="btnSynchAD_Click" UseSubmitBehavior="False" />
						&nbsp;or 
					 </asp:PlaceHolder>
					 Add New Profile:
					 <asp:TextBox runat="server" ID="txtAddUser"></asp:TextBox>
					 <asp:Button ID="btnAddUser" runat="server" Text="Add" 
						OnClick="btnAddUser_Click" UseSubmitBehavior="False" />
				</td></tr></table>
			   </td>
			   <td style="font-size: 7.5pt">
				Search for users by employee ID, login, username, or display name<br />
				username is usually [last name][first initial]<br />
				display name is usually [last name].[first name]<br />
				login is [domain]\[username]
			   </td>
			  </tr>
			 </table>
				
				<hr />
			</asp:Panel>

	<asp:Button runat="server" CssClass="btnEditProfile" ID="btnEditProfile"	OnClick="btnEditProfile_Click" Text="Add" Style="" />
	<asp:ModalPopupExtender runat="server" ID="modalPopupExtenderEditProfile" 
		TargetControlID="btnDoNothingEditProfile" PopupControlID="panelEditProfilePopup"
		PopupDragHandleControlID="divEditProfilePopupHeader" Drag="true" BackgroundCssClass="ModalPopupBG">
	</asp:ModalPopupExtender>
	<asp:Panel runat="server" ID="panelEditProfilePopup" DefaultButton="btnDoNothingEditProfile" Style="display: none">
		<div class="Popup">
			<asp:HiddenField runat="server" ID="IdxProfile" />
			<div runat="server" class="PopupHeader" id="divEditProfilePopupHeader">
				Add Profile</div>
			<div class="PopupBody">
				<br />
				<div class="tableRow">
					<label>User Name ({domain}\{username}):</label>
					<asp:TextBox runat="server" ID="txtUserName"/><br />
				</div>
				<div class="tableRow">
					<label>Display Name:</label>
					<asp:TextBox runat="server" ID="txtDisplayName" /><br />
				</div>
				<div class="tableRow">
					<label>Employee ID:</label>
					<asp:TextBox runat="server" ID="txtEmployeeID" /><br />
				</div>
				<div class="tableRow">
					<label>Org Code:</label>
					<asp:TextBox runat="server" ID="txtOrgCode" /><br />
				</div>
				<div class='tableRow'>
					<label>Manager:</label>
					<asp:DropDownList runat="server" ID="ddlManager" ></asp:DropDownList>
				</div>
				<div class="tableRow">
					<label>Role(s):</label>
					<asp:ListBox runat="server" ID="lbxRoles" />
				</div>
				<asp:ProfileSelector runat="server" ID="profileSelectorEditProfile" DirectorateManagersOnly="true"
					IncludePlanExempt="true"></asp:ProfileSelector>
				<br />
			</div>
			<div class="Controls">
				<asp:Button runat="server" ID="btnSaveProfile" Text="Save" OnClick="btnSaveProfile_Click" />
				<asp:Button runat="server" ID="btnCancelEditProfile" Text="Cancel" OnClick="btnCancelEditProfile_Click" />
				<asp:Button runat="server" ID="btnDoNothingEditProfile" Enabled="false" Style="display: none" />
			</div>
		</div>
	</asp:Panel>

			<p />
			<asp:Label ID="Label2" runat="server" Text="Filter:" Font-Bold="true"></asp:Label>&nbsp;
			<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
			<asp:Button ID="Button1"
				runat="server" Text="Update" onclick="Button1_Click" />
			&nbsp;
			<asp:Label ID="Label1" runat="server" Text=""></asp:Label>
			<table>
				<tr>
					<td valign="top" style="padding-right: 20px;">
						<br />
						<div id="divUserInfo">
									<!-- asp:ButtonField Text="Edit" HeaderText="" CommandName="GridView1_RowEditing" / -->
							<asp:GridView ID="GridView1" runat="server" AllowPaging="True" AutoGenerateSelectButton="True"
								CellPadding="4" ForeColor="#333333" GridLines="None" Width="500px"
								OnPageIndexChanging="GridView1_PageIndexChanging" OnRowCancelingEdit="GridView1_RowCancelingEdit" OnRowDeleting="GridView1_RowDeleting"
								OnRowEditing="GridView1_RowEditing" OnRowUpdating="GridView1_RowUpdating" OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
								AutoGenerateColumns="False" EmptyDataText="Please enter a filter." DataKeyNames="Username">
								<RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
								<Columns>
									<asp:BoundField DataField="DisplayName" HeaderText="Employee Name" />
									<asp:BoundField DataField="OrgCode" HeaderText="Org Code" />
									<asp:CheckBoxField ItemStyle-HorizontalAlign="Center" DataField="Active" HeaderText="Active" ItemStyle-Width="50px" />
									<asp:CheckBoxField ItemStyle-HorizontalAlign="Center" DataField="ExemptPlan" HeaderText="Plan Not Required" ItemStyle-Width="50px" />
								</Columns>
								<FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
								<PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
								<SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
								<HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
								<EditRowStyle BackColor="#999999" />
								<AlternatingRowStyle BackColor="White" ForeColor="#284775" />
							</asp:GridView>
						</div>
					</td>
					<td valign="bottom">
						<asp:Label CssClass="listBoxLabel" runat="server" ID="lblRolesList" Text="Roles:"
							AssociatedControlID="lstRoles" Font-Bold="true"></asp:Label>
						<br />
						<asp:ListBox runat="server" ID="lstRoles" Height="315px" Width="200px" SelectionMode="Multiple"></asp:ListBox>
					</td>
					<td>
						<asp:Button runat="server" ID="btnMoveLeft" Text="<" OnClick="ButtonMoveLeft_Click" />
						<br />
						<br />
						<asp:Button runat="server" ID="btnMoveRight" Text=">" OnClick="ButtonMoveRight_Click" />
					</td>
					<td valign="bottom">
						<asp:Label CssClass="listBoxLabel" runat="server" ID="lblAvailableRolesList" Text="Available Roles:"
							AssociatedControlID="lstAvailableRoles" Font-Bold="true"></asp:Label>
						<br />
						<asp:ListBox runat="server" ID="lstAvailableRoles" Height="315px" Width="200px" SelectionMode="Multiple">
						</asp:ListBox>
					</td>
				</tr>
			</table>
		</ContentTemplate>
	</asp:UpdatePanel>
	<% } %>
</asp:Content>
