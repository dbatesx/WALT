<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserPreferences.aspx.cs"
    Inherits="WALT.UIL.UserPreferences" MasterPageFile="Site1.Master" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="font-size: 16pt">
        <b>User Preferences</b></div>
    <br />
    <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse: collapse;
        width: 100%; padding-top: 5px;">
        <tr>
            <td style="border-top: 1px solid #C0C0C0; padding-bottom:10px; padding-top:5px;">
                <font style="font-size: 15px; font-weight: bold;">Default Programs</font>
            </td>
        </tr>
        <tr>
            <td style="padding-bottom: 5px;">
                <b> Programs:&nbsp</b>
                <asp:DropDownList ID="ddlPrograms" runat="server">
                </asp:DropDownList>
                &nbsp&nbsp<asp:Button ID="btnAddProgram" runat="server" Text="Add" OnClick="btnAddProgram_Click" />
                <br />
            </td>
        </tr>
        <tr>
            <td>
                <span style="font-weight: bold; padding-bottom:5px;">Selected Programs</span>
                <br />  
                <div style="padding-top: 5px;">             
                <asp:Repeater ID="rptProgPrefrence" runat="server">
                    <HeaderTemplate>
                        <table cellpadding="0" cellspacing="0" style="border-collapse: collapse; padding-top:10px;">
                            <tr>
                                <th align="center" style="border: 1px solid black; background-color: #dddddd; padding: 5px;">
                                </th>
                                <th align="center" style="border: 1px solid black; background-color: #dddddd; padding: 5px;">
                                    Program
                                </th>
                                <th align="center" style="border: 1px solid black; background-color: #dddddd; padding: 5px;">
                                    Default
                                </th>
                            </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td align="center" style="border: 1px solid black; padding: 5px;">
                                <asp:LinkButton ID="lbtnRemove" runat="server" CommandArgument='<%# DataBinder.Eval(Container, "DataItem.progId") %>'
                                    ForeColor="Red" OnClick="lbtnRemove_Click">Remove</asp:LinkButton>
                            </td>
                            <td align="center" style="border: 1px solid black; padding: 5px;">
                                <asp:HiddenField ID="hdnComplexityId" runat="server" Value='<%# DataBinder.Eval(Container, "DataItem.progId") %>' />
                                <asp:Label ID="lblProgTitle" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.progTitle") %>'></asp:Label>
                            </td>
                            <td align="center" style="border: 1px solid black; padding: 5px;">
                                <asp:CheckBox ID="cbDefault" runat="server" AutoPostBack="true" Checked='<%# DataBinder.Eval(Container, "DataItem.default") %>'
                                    OnCheckedChanged="cbDefault_CheckedChanged" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table><br />
                    </FooterTemplate>
                </asp:Repeater>
                </div>
            </td>       
        </tr>
    </table>

</asp:Content>
