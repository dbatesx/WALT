<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Roles.aspx.cs" Inherits="WALT.UIL.Admin.Roles"
    MasterPageFile="~/Site1.Master" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<% if (WALT.BLL.ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.ROLE_MANAGE)) { %>
    <asp:Panel runat="server" ID="panelAddRole">
    <div style="font-size: 16pt; font-weight: bold">Admin - Roles</div><br />
    <table cellpadding="10px" style="border: 1px solid #000000">
        <tr>
            <td>
                <b>Title:</b>
            </td>
            <td>
                <asp:TextBox ID="txtAddRoleTitle" runat="server"></asp:TextBox>
            </td>
            <td>
                <b>Description:</b>
            </td>
            <td>
                <asp:TextBox ID="txtAddRoleDescription" runat="server"></asp:TextBox>
            </td>
            <td>
                <asp:Button ID="btnAddRole" runat="server" Text="Add Role" OnClick="btnAddRole_Click" />
            </td>
        </tr>
    </table>
    </asp:Panel>
    <p />
    <table>
        <tr valign="top">
            <td>
                <table>
                    <tr>
                        <td>
                            <b>Roles</b>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:GridView ID="GridView1" runat="server" AllowPaging="True"
                                AutoGenerateSelectButton="True" CellPadding="4" ForeColor="#333333" GridLines="None"
                                OnSelectedIndexChanged="GridView1_SelectedIndexChanged" Width="400px" OnRowCancelingEdit="GridView1_RowCancelingEdit"
                                OnRowEditing="GridView1_RowEditing" OnRowUpdating="GridView1_RowUpdating" OnPageIndexChanging="GridView1_PageIndexChanging" DataKeyNames="Description">
                                <Columns>
                                    <asp:BoundField DataField="Title" HeaderText="Title" />
                                    <asp:BoundField DataField="Description" HeaderText="Description" />
                                    <asp:CheckBoxField DataField="Active" HeaderText="Active" />
                                </Columns>
                                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
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
            </td>
            <td>
                <table>
                    <tr>
                        <td>
                            <b>Role Actions</b>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:ListBox ID="lstRoleActions" runat="server" Height="315px" Width="200px" SelectionMode="Multiple"></asp:ListBox>
                        </td>
                    </tr>
                </table>
            </td>
            <td valign="middle">
                <table>
                    <tr>
                        <td>
                            <asp:Button ID="btnMoveLeft" runat="server" Text="&lt;" OnClick="btnMoveLeft_Click" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Button ID="btnMoveRight" runat="server" Text="&gt;" OnClick="btnMoveRight_Click" />
                        </td>
                    </tr>
                </table>
            </td>
            <td>
                <table>
                    <tr>
                        <td>
                            <b>System Actions</b>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:ListBox ID="lstSystemActions" runat="server" Height="315px" Width="200px" SelectionMode="Multiple"></asp:ListBox>
                        </td>
                    </tr>
                </table>
            </td>
            <td>
            </td>
        </tr>
    </table>
    <% } %>
</asp:Content>
