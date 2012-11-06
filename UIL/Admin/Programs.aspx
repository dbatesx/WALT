<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Programs.aspx.cs" Inherits="WALT.UIL.Admin.Programs"
    MasterPageFile="~/Site1.Master" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <% if (WALT.BLL.ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.METADATA_MANAGE))
       { %>
    <asp:Panel runat="server" ID="panelAddProgram">
        <div style="font-size: 16pt; font-weight: bold">Admin - Programs</div><br />
        <table>
            <tr>
            <td colspan="4">
                Please update the excel template after adding or modifying programs.
            </td>
            </tr>
            <tr>
                <td>
                    Name:
                </td>
                <td>
                    <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                </td>
                <td>
                    <asp:Button ID="Button2" runat="server" Text="Add" OnClick="Button2_Click" />
                </td>
                <td>
                    <asp:Button ID="Button3" runat="server" Text="Update Excel Template" OnClick="Button3_Click" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <table>
        <tr>
            <td>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateSelectButton="False"
                    CellPadding="4" Width="500px" ForeColor="#333333" GridLines="None" OnPageIndexChanging="GridView1_PageIndexChanging"
                    OnRowCancelingEdit="GridView1_RowCancelingEdit" OnRowDeleting="GridView1_RowDeleting"
                    OnRowEditing="GridView1_RowEditing" OnRowUpdating="GridView1_RowUpdating" OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
                    AutoGenerateColumns="False" EmptyDataText="No Programs are defined" AutoGenerateEditButton="True">
                    <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                    <Columns>
                        <asp:BoundField DataField="Title" HeaderText="Title" ControlStyle-Width="350" />
                        <asp:CheckBoxField DataField="Active" HeaderText="Active" />
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
    <% } %>
</asp:Content>
