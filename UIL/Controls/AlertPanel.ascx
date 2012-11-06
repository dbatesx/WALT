<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AlertPanel.ascx.cs" Inherits="WALT.UIL.Controls.AlertPanel" %>

<style type="text/css">
    table.alertTbl
    {
	    border-collapse: collapse;
	    font-family: tahoma;
        font-size: 8.5pt;
        padding: 3px;
    }
    
    table.alertTbl td.bordered
    {
        padding: 0px;
        border-width: 1px;
        border-style: solid;
        border-color: black;
    }
    
    table.alertTbl td.divider
    {
        border-width: 1px 0px 0px 0px;
        border-style: solid;
        border-color: black;
    }
    
    table.alertTbl td.bottom
    {
        border-width: 0px 0px 1px 0px;
        border-style: solid;
        border-color: black;
    }
    
    table.alertSearch
    {
        border: 1px 1px 1px 1px;
        border-style: solid;
        border-width: 1px;
        padding: 0px;
    }
</style>

<script type="text/javascript" language="javascript">

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(LoadDatePickers);

    function LoadDatePickers() {
        $(function () {
            $(".datepicker").datepicker();
        });
    }

    LoadDatePickers();
</script>

<asp:UpdatePanel ID="alertUpdatePanel" runat="server">
 <ContentTemplate>
  <table cellpadding="3" cellspacing="0" class="alertTbl">
   <tr>
    <td style="font-size: 12pt"><b>Alerts</b></td>
    <td align="right"><b>View:</b>
     <asp:DropDownList ID="alertView" OnSelectedIndexChanged="alertView_SelectedIndexChanged" AutoPostBack="true" runat="server">
      <asp:ListItem Text="Unread" Value="0" Selected="True"></asp:ListItem>
      <asp:ListItem Text="Read" Value="1"></asp:ListItem>
      <asp:ListItem Text="Sent" Value="2"></asp:ListItem>
     </asp:DropDownList>
    </td>
    <td></td>
   </tr>
   <tr>
    <td class="bordered" valign="top" width="500" colspan="2">
     <asp:GridView ID="alertGrid" runat="server" AutoGenerateColumns="false" DataKeyNames="ID" 
        AllowSorting="true" AllowPaging="true" OnSorting="alertGrid_Sorting" OnPageIndexChanging="alertGrid_PageIndexChanging" PageSize="15"
        HeaderStyle-BackColor="#EEEEEE" PagerStyle-BackColor="#EEEEEE" BorderStyle="None" CellPadding="4" CellSpacing="0" Width="500">
      <Columns>
       <asp:TemplateField HeaderText="Read" >
        <ItemStyle HorizontalAlign="Center" />
        <ItemTemplate>
         <asp:CheckBox ID="chkRead" runat="server" />
        </ItemTemplate>
       </asp:TemplateField>
       <asp:TemplateField HeaderText="Subject" SortExpression="SUBJECT">
        <ItemTemplate>
         <asp:LinkButton ID="lnkSubject" Text='<%# Bind("Subject") %>' OnClick="lnkSubject_Click" runat="server"></asp:LinkButton>
        </ItemTemplate>
       </asp:TemplateField>
       <asp:BoundField DataField="Type" HeaderText="Type" SortExpression="LINKEDTYPE" />
       <asp:BoundField DataField="Sender" HeaderText="Sender" SortExpression="CREATOR" />
       <asp:BoundField DataField="SentTo" HeaderText="Sent To" SortExpression="PROFILE" Visible="false" />
       <asp:TemplateField HeaderText="Created" SortExpression="ENTRYDATE">
        <ItemTemplate>
         <asp:Label ID="lblEntryDate" Text='<%# WALT.UIL.Utility.ConvertToLocal((DateTime)Eval("EntryDate")) %>' runat="server" />
        </ItemTemplate>
       </asp:TemplateField>
      </Columns>
     </asp:GridView>
     <asp:Button ID="btnMarkRead" Text="Mark Read" OnClick="btnMarkRead_Click" runat="server" />
     <asp:Label ID="lblNone" Text="&nbsp; No alerts found" runat="server"></asp:Label>
    </td>
    <td valign="top" class="bordered" runat="server" id="cellDetail" visible="false">
     <asp:Table ID="tblAlertDetail" CellPadding="3" CellSpacing="0" runat="server" Width="400">
      <asp:TableRow>
       <asp:TableCell ColumnSpan="2" BackColor="#EEEEEE"><b>Alert Detail</b></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow>
       <asp:TableCell Width="75"><b>Subject:</b></asp:TableCell>
       <asp:TableCell Width="325"><asp:Label ID="lblSubject" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow>
       <asp:TableCell ID="cellProfile"></asp:TableCell>
       <asp:TableCell><asp:Label ID="lblProfile" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow>
       <asp:TableCell ID="cellCreated1"><b>Created:</b></asp:TableCell>
       <asp:TableCell ID="cellCreated2"><asp:Label ID="lblCreated" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowMessage1">
       <asp:TableCell ColumnSpan="2" BackColor="#EEEEEE"><b>Message:</b></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowMessage2">
       <asp:TableCell ColumnSpan="2"><asp:Label ID="lblMessage" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowLink" BackColor="#EEEEEE">
       <asp:TableCell ColumnSpan="2" ID="cellLink"><b>Alert Created Against:</b></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowLinkBarrier1">
       <asp:TableCell Width="75" VerticalAlign="Top"><b><asp:Label ID="lblBarrierType" runat="server"></asp:Label> Barrier:</b></asp:TableCell>
       <asp:TableCell VerticalAlign="Top"><asp:Label ID="lblBarrierLink" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowLinkBarrier2">
       <asp:TableCell Width="75" VerticalAlign="Top"><b>Barrier Comment:</b></asp:TableCell>
       <asp:TableCell VerticalAlign="Top"><asp:Label ID="lblBarrierComment" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowLinkTask">
       <asp:TableCell Width="75"><b>Task:</b></asp:TableCell>
       <asp:TableCell><asp:Label ID="lblTaskLink" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowLinkPlan">
       <asp:TableCell Width="75"><b>Plan/Log:</b></asp:TableCell>
       <asp:TableCell><asp:Label ID="lblPlanLink" runat="server"></asp:Label></asp:TableCell>
      </asp:TableRow>
     </asp:Table>
    </td>
   </tr>
   <tr>
    <td colspan="3" style="padding: 6px 0px 0px 0px">
     <asp:Table ID="tblSearch" CellSpacing="0" CssClass="alertSearch" runat="server">
      <asp:TableRow>
       <asp:TableCell BackColor="#EEEEEE" Width="310" style="padding: 3px">
        <b><asp:LinkButton ID="lnkToggleSearch" runat="server" OnClick="lnkToggleSearch_Click">Search Alerts</asp:LinkButton></b>
       </asp:TableCell>
      </asp:TableRow>
      <asp:TableRow ID="rowSearch">
       <asp:TableCell>
        <table cellpadding="3" cellspacing="0">
         <tr>
          <td align="right"><b>Subject:</b></td>
          <td><asp:TextBox ID="txtSubject" Width="205" runat="server"></asp:TextBox></td>
         </tr>
         <tr>
          <td align="right"><b>Message:</b></td>
          <td><asp:TextBox ID="txtMessage" Width="205" runat="server"></asp:TextBox></td>
         </tr>
         <tr id="rowSender" runat="server">
          <td align="right"><b>Sender:</b></td>
          <td><asp:DropDownList ID="ddSender" runat="server"></asp:DropDownList></td>
         </tr>
         <tr id="rowSentTo" runat="server">
          <td align="right"><b>Sent To:</b></td>
          <td><asp:DropDownList ID="ddSentTo" runat="server"></asp:DropDownList></td>
         </tr>
         <tr>
          <td align="right"><b>Created From:</b></td>
          <td><asp:TextBox ID="dateFrom" width="205" class="datepicker" runat="server"></asp:TextBox></td>
         </tr>
         <tr>
          <td align="right"><b>To:</b></td>
          <td><asp:TextBox ID="dateTo" width="205" class="datepicker" runat="server"></asp:TextBox></td>
         </tr>
         <tr>
          <td colspan="2" align="right">
           <asp:Button ID="btnSearch" Text="Search Alerts" OnClick="btnSearch_Click" runat="server" />
           <asp:Button ID="btnClear" Text="Clear Search" OnClick="btnClear_Click" runat="server" />
          </td>
         </tr>
        </table>
       </asp:TableCell>
      </asp:TableRow>
     </asp:Table>
    </td>
   </tr>
  </table>
 </ContentTemplate>

 <Triggers>
  <asp:AsyncPostBackTrigger ControlID="alertView" EventName="SelectedIndexChanged" />
  <asp:AsyncPostBackTrigger ControlID="alertGrid" EventName="PageIndexChanging" />
  <asp:AsyncPostBackTrigger ControlID="alertGrid" EventName="Sorting" />
  <asp:AsyncPostBackTrigger ControlID="btnMarkRead" EventName="Click" />
  <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
 </Triggers>
</asp:UpdatePanel>
