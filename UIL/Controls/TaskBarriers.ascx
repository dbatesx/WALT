<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskBarriers.ascx.cs"
    Inherits="WALT.UIL.Controls.TaskBarriers" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<script type="text/javascript" language="javascript">
    var xPosTB, yPosTB;
    var prmTB = Sys.WebForms.PageRequestManager.getInstance();
    prmTB.add_beginRequest(BeginRequestHandler);
    prmTB.add_endRequest(EndRequestHandler);
    function BeginRequestHandler(sender, args) {
        xPosTB = $get('divTreeViewBarriers').scrollLeft;
        yPosTB = $get('divTreeViewBarriers').scrollTop;
    }
    function EndRequestHandler(sender, args) {
        $get('divTreeViewBarriers').scrollLeft = xPosTB;
        $get('divTreeViewBarriers').scrollTop = yPosTB;
    }

    function BarrierApplyAllClick(input) {
        if (input.checked) {
            $('#BarrierApplyText').html('Warning: The barrier will show up for all teams in the directorate. Please make sure all teams will use this barrier before adding it with apply to all teams checked.');
        }
        else {
            $('#BarrierApplyText').html('The barrier will not display for any teams in the directorate. You will need to select each team on the parent page that will use this barrier and check it in the barrier tree list.');
        }
    }
</script>
<table style="border-collapse: collapse; padding: 3px">
    <tr>
        <td class="TasksPageTreeViewCell">
            <asp:Label runat="server" ID="lblTreeSelectorBarriers" Text="Task Barriers:" Font-Bold="true" AssociatedControlID="treeViewBarriers"></asp:Label>
        </td>
        <td style="text-align: right; white-space: nowrap; font-size: 8.5pt">
         <asp:LinkButton ID="lnkSelectAll" OnClick="lnkSelectAll_Click" runat="server">Select All</asp:LinkButton>
        </td>
        <td class="TasksPageItemViewCell" rowspan="3">
            <asp:Panel runat="server" ID="panelBarrierReadOnly" Visible="false">
                <asp:Label runat="server" ID="lblROBarrierActive" BackColor="LightGray" Width="300px"
                    CssClass="TasksPageItemViewActiveLabel"></asp:Label>
                <br />
                <br />
                <b>Code:</b>
                <br />
                <asp:Label runat="server" ID="lblROBarrierCode" BackColor="LightGray" Width="300px"
                    CssClass="TasksPageLabels"></asp:Label>
                <br />
                <br />
                <b>Title:</b>
                <br />
                <asp:Label runat="server" ID="lblROBarrierTitle" BackColor="LightGray" Width="300px"
                    CssClass="TasksPageLabels"></asp:Label>
                <br />
                <br />
                <b>Description:</b>
                <asp:TextBox runat="server" ID="txtROBarrierSADescription" Width="300px" Wrap="true"
                    ReadOnly="true" TextMode="MultiLine" 
                    BackColor="LightGray" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px"></asp:TextBox>
                <asp:TextBox runat="server" ID="txtROBarrierDADescription" Height="100px" Width="300px"
                    TextMode="MultiLine" Wrap="true" ReadOnly="true" 
                    BackColor="LightGray" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px"></asp:TextBox>
                <br />
                <br />
                <asp:Panel runat="server" ID="panelROSaveBarrierButton" CssClass="TasksPageItemViewSavePanel" Visible="false">
                    <asp:Button runat="server" ID="btnROSaveBarrier" Text="Save" OnClick="btnROSaveBarrier_Click" />
                    <asp:Label runat="server" ID="lblROSaveBarrierStatus" Visible="false"></asp:Label>
                </asp:Panel>                
            </asp:Panel>
            <asp:Panel runat="server" ID="panelBarrierEditing" Visible="false">
                <asp:CheckBox runat="server" ID="cbBarrierActive" Text="Active" Visible="false" />
                <asp:Button runat="server" ID="btnBarrierActive" Text="" OnClick="btnBarrierActive_Click"
                    CssClass="TasksPageItemViewActiveButton" />
                <br />
                <br />
                <b>Code:</b>
                <br />
                <asp:TextBox runat="server" ID="txtBarrierCode"></asp:TextBox>
                <br />
                <br />
                <b>Title:</b>
                <br />
                <asp:TextBox runat="server" ID="txtBarrierTitle" Width="300px"></asp:TextBox>
                <br />
                <br />
                <b>Description:</b>
                <br />
                <asp:TextBox runat="server" ID="txtBarrierSADescription" Height="100px" Width="300px"
                    TextMode="MultiLine" Wrap="true" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px">
                </asp:TextBox>
                <asp:TextBox runat="server" ID="txtBarrierDADescription" Height="100px" Width="300px"
                    TextMode="MultiLine" Wrap="true" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px"></asp:TextBox>
                <br />
                <br />
                <asp:Panel runat="server" ID="panelSaveBarrierButton" CssClass="TasksPageItemViewSavePanel">
                    <asp:Button runat="server" ID="btnSaveBarrier" Text="Save" OnClick="btnSaveBarrier_Click" />
                    <asp:Label runat="server" ID="lblSaveBarrierStatus" Visible="false"></asp:Label>
                </asp:Panel>
            </asp:Panel>
        </td>
    </tr>
    <tr>
     <td colspan="2">
      <div id="divTreeViewBarriers" style="overflow-y: scroll; padding: 0px 3px 0px 3px; border: 1px solid Black">
                <asp:TreeView runat="server" ID="treeViewBarriers" Height="300px"
                    ShowCheckBoxes="None" PathSeparator="~" OnSelectedNodeChanged="barriers_SelectedNodeChanged"
                    OnTreeNodeCheckChanged="barriers_TreeNodeCheckChanged" SelectedNodeStyle-BackColor="Orange"
                    SelectedNodeStyle-CssClass="TypeSelectedNode" NodeWrap="true">
                </asp:TreeView>
            </div>
     </td>
    </tr>
    <tr>
     <td colspan="2">
            <asp:Button runat="server" ID="btnApplyBarriersToTeam" Text="Apply" OnClick="btnApplyBarriersToTeam_Click"
                Visible="false" />
            <asp:Label runat="server" ID="lblApplyBarriersStatus" Text="Saved!" ForeColor="Green"
                Visible="false"></asp:Label>
            <asp:PlaceHolder runat="server" ID="panelAddBarrier">
                <button type="button" id="btnLinkAddBarrier" onclick="$('.btnAddBarrier').trigger('click');">
                    Add</button>
            </asp:PlaceHolder>
     </td>
    </tr>
</table>
<%--Add Barrier Popup--%>
<asp:Button runat="server" CssClass="btnAddBarrier" ID="btnAddBarrier" Text="Add"
    Style="display: none" />
<asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddBarrier" TargetControlID="btnAddBarrier"
    PopupControlID="panelAddBarrierPopup" PopupDragHandleControlID="divAddBarrierPopupHeader"
    Drag="true" BackgroundCssClass="ModalPopupBG">
</asp:ModalPopupExtender>
<asp:Panel runat="server" ID="panelAddBarrierPopup" DefaultButton="btnDoNothingBarrier">
    <div class="AddBarrierPopup">
        <div class="PopupHeader" id="divAddBarrierPopupHeader">
            Add Barrier</div>
        <div class="PopupBody">
            <asp:UpdatePanel runat="server" ID="upanelNewBarrierCode">
                <ContentTemplate>
                 <table cellpadding="3" cellspacing="0">
                  <tr>
                   <td>
                    <asp:Label runat="server" ID="lblNewBarrierTitle" Text="Title:" Font-Bold="true" AssociatedControlID="txtNewBarrierTitle"></asp:Label>
                   </td>
                   <td><asp:TextBox runat="server" ID="txtNewBarrierTitle" Width="325"></asp:TextBox></td>
                  </tr>
                  <tr>
                   <td valign="top">
                    <asp:Label runat="server" ID="lblNewBarrierDescription" Text="Description:" Font-Bold="true" AssociatedControlID="txtNewBarrierDescription"></asp:Label>
                   </td>
                   <td><asp:TextBox runat="server" ID="txtNewBarrierDescription" TextMode="MultiLine" Width="325" Rows="5"></asp:TextBox></td>
                  </tr>
                  <tr>
                   <td><asp:Label runat="server" ID="lblNewBarrierParent" Text="Parent:" Font-Bold="true" AssociatedControlID="ddlNewBarrierParent"></asp:Label></td>
                   <td><asp:DropDownList runat="server" ID="ddlNewBarrierParent"></asp:DropDownList></td>
                  </tr>
                  <tr>
                   <td><b>Active:</b></td>
                   <td><asp:CheckBox runat="server" ID="cbNewBarrierActive" Checked="true" /></td>
                  </tr>
                  <tr id="rowApplyAll" runat="server">
                   <td valign="top"><b>Apply to all Teams:</b></td>
                   <td valign="top">
                    <table>
                     <tr>
                      <td valign="top"><asp:CheckBox ID="chkApplyAll" runat="server" onclick="BarrierApplyAllClick(this)" Checked="false" />&nbsp;</td>
                      <td id="BarrierApplyText" width="300" style="font-size: 8pt">
                        The barrier will not display for any teams in the directorate.
                        You will need to select each team on the parent page that will use this barrier and check it in the barrier tree list.
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
            <asp:Button runat="server" ID="btnSaveBarrierPopup" Text="Save" OnClick="btnSaveNewBarrier_Click" />
            <asp:Button runat="server" ID="btnCancelBarrierPopup" Text="Cancel" OnClick="btnCancelBarrierPopup_Click" />
            <asp:Button runat="server" ID="btnDoNothingBarrier" Enabled="false" Style="display: none" />
        </div>
    </div>
</asp:Panel>
