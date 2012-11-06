<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Import.aspx.cs" Inherits="WALT.UIL.Task.Import1"  MasterPageFile="/Site1.Master" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <link href="/css/WALT.css" rel="stylesheet" type="text/css" media="screen" />
    <asp:Panel ID="Panel1" runat="server">
        <ul>
            <li>You must import tasks by using the Task Template spreadsheet linked on this page.</li>
            <li>Do not make any modifications to the spreadsheet structure (Sheet names, columns, etc).</li>
            <li>Title and Program are required fields.</li>
            <li>If the Owner field is empty, ownership will be assigned to the person
                performing the import. </li>
            <li>If the Assignee field is incorrect or empty, ownership will be assigned to the person
                performing the import. </li>
        </ul>               
    <p />
    <a href="TaskTemplate.xlsx">Download Import Task Template Spreadsheet</a>
    <p />
    <table cellpadding="3" border="1" cellspacing="0">
        <tr>
            <th>
                Column
            </th>
            <th>
                Description
            </th>
        </tr>
        <tr>
            <td>
                Owner
            </td>
            <td>
                The owner of the task. Display name (typically lastname.firstname) or Employee ID can be used.
            </td>
        </tr>
        <tr>
            <td>
                Assignee
            </td>
            <td>
                The assignee for the task. Same format as Owner.
            </td>
        </tr>
        <tr>
            <td>
                Title<font color="red">*</font>
            </td>
            <td>
                The title of the task. This is a required field.
            </td>
        </tr>
        <tr>
            <td>
                Program<font color="red">*</font>
            </td>
            <td>
                The program name. This is a required field.
            </td>
        </tr>
        <tr>
            <td>
                WBS
            </td>
            <td>
                The WBS number of the task if applicable. If WBS is in dot format (ie, 1.2.3), Tasks
                will be created with parent/child relationship. Parent tasks must be listed first.
            </td>
        </tr>
        <tr>
            <td>
                Source
            </td>
            <td>
                This is an open text field to describe the source of the task (ie, Master Schedule)
            </td>
        </tr>
        <tr>
            <td>
                SourceID
            </td>
            <td>
                This is the unique ID as specified in the Source.
            </td>
        </tr>
        <tr>
            <td>
                StartDate
            </td>
            <td>
                Start date for the task in MM/DD/YYYY format.
            </td>
        </tr>
        <tr>
            <td>
                DueDate
            </td>
            <td>
                Due date for the task in MM/DD/YYYY format
            </td>
        </tr>       
        <tr>
            <td>
                HoursAllocated
            </td>
            <td>
                Hours allocated for the task.
            </td>
        </tr>
        <tr>
            <td>
                ExitCriteria
            </td>
            <td>
                Open text field to describe any exit criteria.
            </td>
        </tr>       
        <tr>
            <td>
                OwnerComments
            </td>
            <td>
                Open text field to add task owner comments.
            </td>
        </tr>
        <tr>
            <td>
                AssigneeComments
            </td>
            <td>
                Open text field to add task assignee comments.
            </td>
        </tr>
    </table>
    <font color="red">*</font> = Required field
    <p>
       Upload Tasks&nbsp;&nbsp;<asp:FileUpload ID="FileUpload1" runat="server" />&nbsp;&nbsp;
  
        <asp:Button ID="Button1" runat="server" Text="Process" 
            onclick="Button1_Click" />      
    </p>
    </asp:Panel>
    <asp:Panel ID="Panel2" runat="server" Visible="false">
    <h2>Preview Imported Tasks</h2>
        <asp:Button ID="Button2" runat="server" Text="Import" onclick="Button2_Click" />&nbsp;
        <asp:Button ID="Button3" runat="server" Text="Cancel" onclick="Button3_Click" />
        <p />
        <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
        <p />

        <asp:Table ID="Table1" runat="server">
        </asp:Table>
        
        <asp:GridView ID="GridView1" runat="server" OnPageIndexChanging="GridView1_PageIndexChanging"
            AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" OnRowDataBound="GridView1_RowDataBound"
            GridLines="None">
            <AlternatingRowStyle BackColor="White" ForeColor="#284775" BorderWidth="1" BorderColor="Black" />
            <RowStyle  BorderWidth="1" BorderColor="Black" />
            <Columns>              
                <asp:TemplateField HeaderText="Processing Info">
                    <ItemTemplate>
                        <asp:Label ID="lblErrMsg" runat="server" Text='<%# Bind("Error") %>'></asp:Label>
                    </ItemTemplate>                   
                    <ItemStyle HorizontalAlign="Left" Wrap="False" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Processing Status">
                    <ItemTemplate>
                        <asp:Label ID="lblErr" runat="server" Text='<%# Bind("Er") %>'></asp:Label>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="center"/>
                </asp:TemplateField>               
                <asp:BoundField DataField="OwnerDisplayName" HeaderText="Owner" />
                <asp:BoundField DataField="AssigneeDisplayName" HeaderText="Assignee" />
                <asp:BoundField DataField="Title" HeaderText="Title" />
                <asp:BoundField DataField="Source" HeaderText="Source" />
                <asp:BoundField DataField="SourceID" HeaderText="SourceID" />
                <asp:BoundField DataField="StartDate" HeaderText="StartDate" />
                <asp:BoundField DataField="DueDate" HeaderText="DueDate" />
                <asp:BoundField DataField="Hours" HeaderText="HoursAllocated" />
                <asp:BoundField DataField="ProgramTitle" HeaderText="Program">               
                <ItemStyle HorizontalAlign="Left" Wrap="False"   />
                </asp:BoundField>
                <asp:BoundField DataField="ExitCriteria" HeaderText="ExitCriteria" />
                <asp:BoundField DataField="WBS" HeaderText="WBS" />
                <asp:BoundField DataField="OwnerComments" HeaderText="OwnerComments" />
                <asp:BoundField DataField="AssigneeComments" HeaderText="AssigneeComments" />
            </Columns>
            <EditRowStyle BackColor="#999999" />
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" BorderWidth="1"
                BorderColor="Black" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#E9E7E2" />
            <SortedAscendingHeaderStyle BackColor="#506C8C" />
            <SortedDescendingCellStyle BackColor="#FFFDF8" />
            <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
        </asp:GridView>
    </asp:Panel>
</asp:Content>