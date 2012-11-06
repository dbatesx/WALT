<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ActiveDirectory.aspx.cs" Inherits="WALT.UIL.test"
    MasterPageFile="~/Site1.Master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxtoolkit" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div style="font-size: 16pt; font-weight: bold">Admin - Active Directory Search</div><br />
<table>
<tr><td><b>Search Field:</b></td><td><asp:DropDownList ID="DropDownList1" runat="server"></asp:DropDownList></td></tr>
<tr><td><b>Search String:</b></td><td><asp:TextBox ID="txtADSearchStr" runat="server" Width="200"></asp:TextBox></td></tr>
</table>
<p />
<asp:Button ID="btnADSearch" Text="Search AD" OnClick="btnADSearch_Click" runat="server" /><br />
<p />
<asp:Label ID="lblADSearchResults" runat="server"></asp:Label>

</asp:Content>
    
