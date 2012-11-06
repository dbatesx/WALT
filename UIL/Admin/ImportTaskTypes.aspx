<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportTaskTypes.aspx.cs" Inherits="WALT.UIL.Admin.ImportTaskTypes"  MasterPageFile="/Site1.Master" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<table cellspacing="0" cellpadding="3">
 <tr>
  <td colspan="2" style="font-size: 16pt"><b>Admin - Import Task Types</b><p /></td>
 </tr>
 <tr>
  <td colspan="2">
   <a href="TaskTypeTemplate.xlsx">Download Import Task Types Template Spreadsheet</a><p />
  </td>
 </tr>
 <tr>
  <td><b>Directorate:</b></td>
  <td>
   <asp:Label ID="lblDirectorate" runat="server"></asp:Label>
  </td>
 </tr>
 <tr>
  <td><b>Import Task Types:</b></td>
  <td><asp:FileUpload ID="TaskTypeFile" Width="400" runat="server" /></td>
 </tr>
 <tr>
  <td></td>
  <td><asp:PlaceHolder ID="phFileError" runat="server"></asp:PlaceHolder></td>
 </tr>
 <tr>
  <td></td>
  <td><asp:Button ID="btnProcess" Text="Process" OnClick="btnProcess_Click" runat="server" /></td>
 </tr>
</table>

<asp:PlaceHolder ID="phImportTable" Visible="false" runat="server">
    <p /><br />
    Preview of import results for <asp:Label ID="lblFilename" runat="server"></asp:Label>:<br>
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CellPadding="3" GridLines="None" CellSpacing="1" BackColor="Black"
        OnRowDataBound="GridView1_RowDataBound">
        <RowStyle BorderWidth="1" BorderColor="Black" />
        <Columns>
            <asp:BoundField DataField="OutputErrorMessages" HeaderText="Processing Info" HtmlEncode="false" />
            <asp:BoundField DataField="Status" HeaderText="Status" />            
            <asp:BoundField DataField="ParentTitle" HeaderText="Task Type Parent" />
            <asp:BoundField DataField="Title" HeaderText="Task Type" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="Level1" HeaderText="Level 1" />
            <asp:BoundField DataField="Level2" HeaderText="Level 2" />
            <asp:BoundField DataField="Level3" HeaderText="Level 3" />
            <asp:BoundField DataField="Level4" HeaderText="Level 4" />
            <asp:BoundField DataField="Level5" HeaderText="Level 5" />
            <asp:BoundField DataField="Level6" HeaderText="Level 6" />
        </Columns>
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" BorderWidth="1" BorderColor="Black" />
    </asp:GridView><br />
    <asp:Label ID="lblSaveError" runat="server">
     <span style="color: red"><b>You must fix all errors to save the task types to the database.</b></span>
    </asp:Label>

    <asp:UpdatePanel ID="ttUpdatePanel" runat="server">
     <ContentTemplate>
      <asp:Button ID="btnSave" Text="Save Imported Task Types" OnClick="btnSave_Click" runat="server" /><br />
      <asp:Label ID="lblResult" runat="server"></asp:Label>
     </ContentTemplate>
     <Triggers>
      <asp:AsyncPostBackTrigger ControlID="btnSave" EventName="Click" />
     </Triggers>
    </asp:UpdatePanel>

 </asp:PlaceHolder>

 <p /><br />
 <a href="/Admin/Tasks.aspx">Return to Admin Tasks Page</a>
</asp:Content>
