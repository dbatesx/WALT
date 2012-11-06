<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TeamSummary.ascx.cs" Inherits="WALT.UIL.Controls.TeamSummary" %>

<asp:PlaceHolder ID="tsContent" runat="server">
<link rel="stylesheet" href="/css/WALT.css" type="text/css" media="screen" />

<asp:Table ID="tsTable" CssClass="weeklyOuterTbl" runat="server">
 <asp:TableRow ID="rowTitle">
  <asp:TableCell ColumnSpan="2" style="font-size: 12pt; border-bottom: 1px solid #CCCCCC; padding-bottom: 3px">
   <b>Weekly Plan/Log Team Summary</b>
  </asp:TableCell>
 </asp:TableRow>
 <asp:TableRow>
  <asp:TableCell ID="cellTeam" style="padding-top: 5px">
   <b>Team:</b>
   <asp:DropDownList ID="ddTeam" AutoPostBack="true" OnSelectedIndexChanged="ddTeam_SelectedIndexChanged" runat="server" /> &nbsp;
   <b>ALM:</b> <asp:Label ID="lblALM" runat="server"></asp:Label>
  </asp:TableCell>
  <asp:TableCell style="font-size: 12pt" ID="cellTitle">
   <b>My Plan/Log Summary</b>
  </asp:TableCell>
  <asp:TableCell HorizontalAlign="Right">
   <table>
    <tr>
     <td><asp:ImageButton ID="lnkPrevWeek" ImageUrl="/images/pointer-left.png" OnClick="lnkPrevWeek_Click" runat="server" /></td>
     <td><asp:Label ID="lblWeek" runat="server" /></td>
     <td><asp:ImageButton ID="lnkNextWeek" ImageUrl="/images/pointer-right.png" OnClick="lnkNextWeek_Click" runat="server" /></td>
    </tr>
   </table>
  </asp:TableCell>
 </asp:TableRow>
 <asp:TableRow>
  <asp:TableCell ColumnSpan="2">
   <asp:Table ID="tblSummary" CssClass="weeklyTbl" runat="server">
    <asp:TableHeaderRow>
     <asp:TableHeaderCell RowSpan="2" ID="hdrColumn"></asp:TableHeaderCell>
     <asp:TableHeaderCell RowSpan="2">Status</asp:TableHeaderCell>
     <asp:TableHeaderCell RowSpan="2">Modified</asp:TableHeaderCell>
     <asp:TableHeaderCell ColumnSpan="6">Hours</asp:TableHeaderCell>
    </asp:TableHeaderRow>
    <asp:TableHeaderRow>
     <asp:TableHeaderCell>Planned</asp:TableHeaderCell>
     <asp:TableHeaderCell>Hours Against<br />Planned</asp:TableHeaderCell>
     <asp:TableHeaderCell>Hours Against<br />Unplanned</asp:TableHeaderCell>
     <asp:TableHeaderCell>Total<br />Actuals</asp:TableHeaderCell>
     <asp:TableHeaderCell>Efficiency<br />Barrier</asp:TableHeaderCell>
     <asp:TableHeaderCell>Delay<br />Barrier</asp:TableHeaderCell>
    </asp:TableHeaderRow>
   </asp:Table>
  </asp:TableCell>
 </asp:TableRow>
</asp:Table>

</asp:PlaceHolder>
