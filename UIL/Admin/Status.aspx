﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Status.aspx.cs" Inherits="WALT.UIL.Status" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<script type="text/javascript" language="javascript">
    function SendSysAlert() {
        var check = $('.txtSysAlertSub');

        if (check.val() == '') {
            alert("Please enter a subject");
            check = document.getElementById(check.attr('id'));
            check.focus();
            return false;
        }

        check = $('.txtSysAlertMsg');

        if (check.val() == '') {
            alert("Please enter a message");
            check = document.getElementById(check.attr('id'));
            check.focus();
            return false;
        }

        return confirm('Are you sure you want to send an alert to all users?');
    }
</script>

<div style="font-size: 16pt; font-weight: bold">Admin - System Status</div><br />
<table cellpadding="5">

<tr align="right">
<td>Number of Sessions:</td><td>
    <asp:Label ID="Label1" runat="server" Text="0"></asp:Label></td>
</tr>

</table>

    <asp:GridView ID="GridView1" runat="server" CellPadding="4" ForeColor="#333333" 
        GridLines="None" AutoGenerateColumns="False" BorderStyle="Solid" BorderColor="Black" BorderWidth="1" 
        AutoGenerateDeleteButton="True" onrowdeleting="GridView1_RowDeleting" 
        AllowSorting="True">
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
        <Columns>
            <asp:BoundField DataField="ID" HeaderText="ID"/>
            <asp:BoundField DataField="Created" HeaderText="Created"/>
            <asp:BoundField DataField="PageLoad" HeaderText="Last Page Load" />
            <asp:BoundField DataField="ObjectCount" HeaderText="Objects"/>
            <asp:BoundField DataField="Profile" HeaderText="Profile"/>
        </Columns>
        <EditRowStyle BackColor="#999999" />
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>
<p />

<table cellpadding="5" cellspacing="0" style="border: 1px solid Black">
 <tr>
  <td style="background-color: #EEEEEE"><b>Set System Message</b></td>
 </tr>
 <tr>
  <td>
    <asp:TextBox ID="TextBox1" runat="server" Width="500px"></asp:TextBox>
    <asp:Button ID="Button1" runat="server" Text="Save" onclick="Button1_Click" />
  </td>
 </tr>
</table>
<p />
<table cellpadding="5" cellspacing="0" style="border: 1px solid Black">
 <tr>
  <td colspan="2" style="background-color: #EEEEEE"><b>Send System Alert</b></td>
 </tr>
 <tr>
  <td align="right">Subject:</td>
  <td>
   <asp:TextBox ID="txtSysAlertSub" class="txtSysAlertSub" Width="500px" runat="server"></asp:TextBox>
  </td>
 </tr>
 <tr>
  <td align="right" valign="top">Message:</td>
  <td>
   <asp:TextBox ID="txtSysAlertMsg" class="txtSysAlertMsg" TextMode="MultiLine" Width="500px" Rows="6" runat="server"></asp:TextBox>
  </td>
 </tr>
 <tr>
  <td></td>
  <td>
   <asp:Button ID="btnSendSysAlert" Text="Send Alert" OnClick="btnSendSysAlert_Click" runat="server"
        OnClientClick="if(!SendSysAlert())return false" />
  </td>
 </tr>
</table>

</asp:Content>