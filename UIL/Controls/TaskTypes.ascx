<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskTypes.ascx.cs" 
    Inherits="WALT.UIL.Controls.TaskTypes" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<script type="text/javascript" language="javascript">
    var xPosTT, yPosTT;
    var prmTT = Sys.WebForms.PageRequestManager.getInstance();
    prmTT.add_beginRequest(BeginRequestHandler);
    prmTT.add_endRequest(EndRequestHandler);
    function BeginRequestHandler(sender, args) {
        xPosTT = $get('divTreeViewTaskTypes').scrollLeft;
        yPosTT = $get('divTreeViewTaskTypes').scrollTop;
    }
    function EndRequestHandler(sender, args) {
        $get('divTreeViewTaskTypes').scrollLeft = xPosTT;
        $get('divTreeViewTaskTypes').scrollTop = yPosTT;
    }

    function TTApplyAllClick(input) {
        if (input.checked) {
            $('#TTApplyText').html('Warning: The task type will show up for all teams in the directorate. Please make sure all teams will use this task type before adding it with apply to all teams checked.');
        }
        else {
            $('#TTApplyText').html('The task type will not display for any teams in the directorate. You will need to select each team on the parent page that will use this task type and check it in the task type tree list.');
        }
    }
</script>
<table style="border-collapse: collapse; padding: 3px">
    <tr>
        <td>
         <asp:Label runat="server" ID="lblTreeSelectorTaskTypes" Text="Task Types:" Font-Bold="true"
                AssociatedControlID="treeViewTaskTypes"></asp:Label>
        </td>
        <td style="text-align: right; white-space: nowrap; font-size: 8.5pt">
            <asp:LinkButton ID="lnkSelectAll" OnClick="lnkSelectAll_Click" runat="server">Select All</asp:LinkButton>
        </td>
        <td rowspan="2" width="16">&nbsp;</td>
        <td></td>
    </tr>
    <tr>
        <td valign="top" colspan="2">
            
            <div id="divTreeViewTaskTypes" style="height: 300px; width: 300px; padding: 0px 3px 0px 3px; overflow-y: auto; border: 1px solid Black">
                <asp:TreeView runat="server" ID="treeViewTaskTypes" CssClass="TasksPageTreeView"
                    Width="100%" ShowCheckBoxes="None" PathSeparator="~" OnSelectedNodeChanged="taskTypes_SelectedNodeChanged"
                    OnTreeNodeCheckChanged="taskTypes_TreeNodeCheckChanged" SelectedNodeStyle-BackColor="Orange"
                    SelectedNodeStyle-CssClass="TypeSelectedNode" NodeWrap="true">
                </asp:TreeView>
            </div>
            <table>
             <tr><td height="28" valign="bottom">
              <asp:Button runat="server" ID="btnApplyTaskTypesToTeam" Text="Apply" OnClick="btnApplyTaskTypesToTeam_Click"
                Visible="false" />
              <asp:Button ID="btnAddTaskType" CssClass="btnAddTaskType" Text="Add Task Type" runat="server" />
              <asp:Button ID="btnImport" Text="Import" OnClick="btnImport_Click" runat="server" />
             </td></tr>
            </table>
        </td>
        <td valign="top">
          <asp:Panel runat="server" ID="panelTaskTypeEditing" Enabled="false">
            <table cellpadding="2" cellspacing="0">
                <tr>
                    <td>
                        <asp:CheckBox runat="server" ID="cbTaskTypeActive" Text="Active" Visible="false" />
                        <asp:Button runat="server" ID="btnTaskTypeActive" Text="" OnClick="btnTaskTypeActive_Click"
                            CssClass="TasksPageItemViewActiveButton" />
                        <asp:Label runat="server" ID="lblROTaskTypeActive" BackColor="LightGray" Width="300px"
                            CssClass="TasksPageItemViewActiveLabel"></asp:Label>
                     </td>
                </tr>
                <tr><td><b>Title:</b></td></tr>
                <tr><td>
                    <asp:TextBox runat="server" ID="txtTaskTypeTitle" CssClass="TasksPageItemViewTitle"></asp:TextBox>
                </td></tr>
                <tr><td><b>Description:</b></td></tr>
                <tr><td>
                 <asp:TextBox runat="server" ID="txtTaskTypeDescription" CssClass="TasksPageItemViewDescription"
                    TextMode="MultiLine" Wrap="true" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px"></asp:TextBox>
                </td></tr>
                <tr id="rowParent" runat="server">
                 <td>
                  <b>Parent:</b> &nbsp;
                  <asp:DropDownList ID="ddParent" runat="server"></asp:DropDownList>
                 </td>
                </tr>
                <tr><td align="right">
                    <asp:Panel runat="server" ID="panelSaveTaskTypeButton">
                        <asp:Button runat="server" ID="btnSaveTaskType" Text="Save" OnClick="btnSaveTaskType_Click" />
                        <asp:Label runat="server" ID="lblSaveTaskTypeStatus" Visible="false"></asp:Label>
                    </asp:Panel>
                </td></tr>
              </table>
            </asp:Panel>
            <asp:Panel runat="server" ID="panelComplexEditing" Enabled="false">
                <table id="complexityTbl" runat="server">
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="Label2" Text="Complexity Code:" Font-Bold="true"></asp:Label>                                                                               
                        </td>                                   
                    </tr>
                    <tr>
                        <td>                              
                            <div>
                                <asp:UpdatePanel runat="server" ID="upanelComplexityCode" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Repeater ID="rptComplexity" runat="server" OnItemDataBound="rptComplexity_ItemDataBound">
                                            <HeaderTemplate>
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <th width="100px" align="center" style="border: 1px solid black; background-color: #dddddd; 
                                                            padding:5px;">
                                                            Title
                                                        </th>
                                                        <th width="100px" align="center" style="border: 1px solid black; background-color: #dddddd;
                                                            padding: 5px;">
                                                            Hours
                                                        </th>                                                                    
                                                    </tr>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td width="100px" align="center" style="border: 1px solid black;">                                                                
                                                        <asp:HiddenField ID="hdnComplexityId" runat="server" Value='<%# GetId(Container.DataItem) %>' />
                                                        <asp:Panel ID="pnlItemWrapper" runat="server" Width="100%" Height="22px" style=" padding-top:3px; vertical-align:middle; text-align:center;">                                                                    
                                                        <asp:Label ID="lblComplexityTitle" runat="server" Text='<%# ((WALT.DTO.Complexity)Container.DataItem).Title%>'></asp:Label>
                                                        </asp:Panel>
                                                    </td>
                                                    <td width="100px" align="center" style="border: 1px solid black;">
                                                        <asp:TextBox ID="txtComplexityHrs" runat="server" Text='<%# ((WALT.DTO.Complexity)Container.DataItem).Hours%>'
                                                            Width="30px"></asp:TextBox>
                                                        <asp:RegularExpressionValidator ID="RERegularExpressionValidator" runat="server"
                                                            ControlToValidate="txtComplexityHrs" ValidationExpression="^(\.\d)|(\d+(\.\d)?)$"
                                                            ErrorMessage="<br />Not valid" ForeColor="Red" Display="Dynamic"></asp:RegularExpressionValidator>  
                                                        <asp:Label ID="lblComplexityHrs" runat="server" Text='<%# ((WALT.DTO.Complexity)Container.DataItem).Hours%>' Visible="false"></asp:Label>                                                                    
                                                    </td>                                                                
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </table>
                                            </FooterTemplate>
                                        </asp:Repeater>                                            
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>                                       
                        </td>                                    
                    </tr>
                    <tr>
                    <td align="right" id="rowAddComplexityCode" runat="server">                                  
                        <asp:Button runat="server" ID="btnSaveComplexityCode" Text="Save" OnClick="btnSaveComplexityCode_Click" />
                    </td>
                    </tr>
                </table>
            </asp:Panel>
        </td>
     </tr>
</table>
<asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddTaskType" TargetControlID="btnAddTaskType"
    PopupControlID="panelAddTaskTypePopup" PopupDragHandleControlID="divAddTaskTypePopupHeader"
    Drag="true" BackgroundCssClass="ModalPopupBG">
</asp:ModalPopupExtender>
<asp:Panel runat="server" ID="panelAddTaskTypePopup" DefaultButton="btnDoNothingTaskType"
    Style="display: none">
    <div class="AddTaskTypePopup">
        <div class="PopupHeader" id="divAddTaskTypePopupHeader">
            Add Task Type</div>
        <div class="PopupBody">
            <asp:UpdatePanel runat="server" ID="upanelNewTaskTypeCode">
                <ContentTemplate>
                 <table cellpadding="3" cellspacing="0">
                  <tr>
                   <td>
                    <asp:Label runat="server" ID="lblNewTaskTypeTitle" Text="Title:" Font-Bold="true" AssociatedControlID="txtNewTaskTypeTitle"></asp:Label>
                   </td>
                   <td><asp:TextBox runat="server" ID="txtNewTaskTypeTitle" Width="325"></asp:TextBox></td>
                  </tr>
                  <tr>
                   <td valign="top">
                    <asp:Label runat="server" ID="lblNewTaskTypeDescription" Text="Description:" Font-Bold="true" AssociatedControlID="txtNewTaskTypeDescription"></asp:Label>
                   </td>
                   <td><asp:TextBox runat="server" ID="txtNewTaskTypeDescription" TextMode="MultiLine" Width="325" Rows="5"></asp:TextBox></td>
                  </tr>
                  <tr>
                   <td>
                    <asp:Label runat="server" ID="lblNewTaskTypeParent" Text="Parent:" Font-Bold="true" AssociatedControlID="ddlNewTaskTypeParent"></asp:Label>
                   </td>
                   <td><asp:DropDownList runat="server" ID="ddlNewTaskTypeParent"></asp:DropDownList></td>
                  </tr>
                  <tr>
                   <td><b>Active:</b></td>
                   <td><asp:CheckBox runat="server" ID="cbNewTaskTypeActive" Checked="true" /></td>
                  </tr>
                  <tr id="rowApplyAll" runat="server">
                   <td valign="top"><b>Apply to all Teams:</b></td>
                   <td valign="top">
                    <table>
                     <tr>
                      <td valign="top"><asp:CheckBox ID="chkApplyAll" runat="server" onclick="TTApplyAllClick(this)" Checked="false" />&nbsp;</td>
                      <td id="TTApplyText" width="300" style="font-size: 8pt">
                        The task type will not display for any teams in the directorate.
                        You will need to select each team on the parent page that will use this task type and check it in the task type tree list.
                      </td>
                     </tr>
                    </table>
                   </td>
                  </tr>
                 </table>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="Controls" style="text-align: right">
            <asp:Button runat="server" ID="btnSaveTaskTypePopup" Text="Save" OnClick="btnSaveNewTaskType_Click" />
            <asp:Button runat="server" ID="btnCancelTaskTypePopup" Text="Cancel" OnClick="btnCancelTaskTypePopup_Click" />
            <asp:Button runat="server" ID="btnDoNothingTaskType" Enabled="false" Style="display: none" />
        </div>
    </div>
</asp:Panel>
