<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskUnplannedCodes.ascx.cs"
    Inherits="WALT.UIL.Controls.TaskUnplannedCodes" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<script type="text/javascript" language="javascript">
    var xPosTU, yPosTU;
    var prmTU = Sys.WebForms.PageRequestManager.getInstance();
    prmTU.add_beginRequest(BeginRequestHandler);
    prmTU.add_endRequest(EndRequestHandler);
    function BeginRequestHandler(sender, args) {
        xPosTU = $get('divTreeViewUnplanned').scrollLeft;
        yPosTU = $get('divTreeViewUnplanned').scrollTop;
    }
    function EndRequestHandler(sender, args) {
        $get('divTreeViewUnplanned').scrollLeft = xPosTU;
        $get('divTreeViewUnplanned').scrollTop = yPosTU;
    }

    function UCApplyAllClick(input) {
        if (input.checked) {
            $('#UCApplyText').html('Warning: The unplanned code will show up for all teams in the directorate. Please make sure all teams will use this code before adding it with apply to all teams checked.');
        }
        else {
            $('#UCApplyText').html('The unplanned code will not display for any teams in the directorate. You will need to select each team on the parent page that will use this code and check it in the unplanned code tree list.');
        }
    }
</script>
<table style="border-collapse: collapse; padding: 3px">
    <tr>
        <td class="TasksPageTreeViewCell">
            <asp:Label runat="server" ID="lblTreeSelectorUnplanned" Text="Unplanned Task Codes:" Font-Bold="true" AssociatedControlID="treeViewUnplanned"></asp:Label>
        </td>
        <td style="text-align: right; white-space: nowrap; font-size: 8.5pt">
         <asp:LinkButton ID="lnkSelectAll" OnClick="lnkSelectAll_Click" runat="server">Select All</asp:LinkButton>
        </td>
        <td class="TasksPageItemViewCell" rowspan="3">
            <asp:Panel runat="server" ID="panelUnplannedReadOnly" Visible="false">
                <asp:Label runat="server" ID="lblROUnplannedActive" BackColor="LightGray" Width="300px"
                    CssClass="TasksPageItemViewActiveLabel"></asp:Label>
                <br />
                <br />
                <b>Code:</b>
                <br />
                <asp:Label runat="server" ID="lblROUnplannedCode" BackColor="LightGray" Width="300px"
                    CssClass="TasksPageLabels"></asp:Label>
                <br />
                <br />
                <b>Title:</b>
                <br />
                <asp:Label runat="server" ID="lblROUnplannedTitle" BackColor="LightGray" Width="300px"
                    CssClass="TasksPageLabels"></asp:Label>
                <br />
                <br />
                <b>Description:</b>
                <asp:TextBox runat="server" ID="txtROUnplannedSADescription" Width="300px" TextMode="MultiLine"
                    Wrap="true" ReadOnly="true" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px"
                    BackColor="LightGray"></asp:TextBox>
                <asp:TextBox runat="server" ID="txtROUnplannedDADescription" Height="100px" Width="300px"
                    TextMode="MultiLine" Wrap="true" ReadOnly="true" BorderColor="Gray" BorderStyle="Solid"
                    BorderWidth="1px" BackColor="LightGray">
                </asp:TextBox>
                <br />
                <br />
                <asp:Panel runat="server" ID="panelROSaveUnplannedButton" CssClass="TasksPageItemViewSavePanel"
                    Visible="false">
                    <asp:Button runat="server" ID="btnROSaveUnplanned" Text="Save" OnClick="btnROSaveUnplanned_Click" />
                    <asp:Label runat="server" ID="lblROSaveUnplannedStatus" Visible="false"></asp:Label>
                </asp:Panel>
            </asp:Panel>
            <asp:Panel runat="server" ID="panelUnplannedEditing" Visible="false">
                <asp:CheckBox runat="server" ID="cbUnplannedActive" Text="Active" Visible="false" />
                <asp:Button runat="server" ID="btnUnplannedActive" Text="" OnClick="btnUnplannedActive_Click"
                    CssClass="TasksPageItemViewActiveButton" />
                <br />
                <br />
                <b>Code:</b>
                <br />
                <asp:Label runat="server" ID="lblUnplannedCodeParent" AssociatedControlID="txtUnplannedCode"></asp:Label>
                <asp:TextBox runat="server" ID="txtUnplannedCode"></asp:TextBox>
                <br />
                <br />
                <b>Title:</b>
                <br />
                <asp:TextBox runat="server" ID="txtUnplannedTitle" CssClass="TasksPageItemViewTitle"></asp:TextBox>
                <br />
                <br />
                <b>Description:</b>
                <br />
                <asp:TextBox runat="server" ID="txtUnplannedSADescription" CssClass="TasksPageItemViewDescription"
                    TextMode="MultiLine" Wrap="true" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px">
                </asp:TextBox>
                <asp:TextBox runat="server" ID="txtUnplannedDADescription" CssClass="TasksPageItemViewDescription"
                    TextMode="MultiLine" Wrap="true" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px">
                </asp:TextBox>
                <br />
                <br />
                <asp:Panel runat="server" ID="panelSaveUnplannedButton" CssClass="TasksPageItemViewSavePanel">
                    <asp:Button runat="server" ID="btnSaveUnplanned" Text="Save" OnClick="btnSaveUnplanned_Click" />
                    <asp:Label runat="server" ID="lblSaveUnplannedStatus" Visible="false"></asp:Label>
                </asp:Panel>
            </asp:Panel>
        </td>
    </tr>
    <tr>
     <td colspan="2">
      <div id="divTreeViewUnplanned" style="overflow-y: scroll; padding: 0px 3px 0px 3px; border: 1px solid Black">
                <asp:TreeView runat="server" ID="treeViewUnplanned" Height="300px" CssClass="TasksPageTreeView"
                    ShowCheckBoxes="None" PathSeparator="~" OnSelectedNodeChanged="unplanned_SelectedNodeChanged"
                    OnTreeNodeCheckChanged="unplanned_TreeNodeCheckChanged" SelectedNodeStyle-BackColor="Orange"
                    SelectedNodeStyle-CssClass="TypeSelectedNode" NodeWrap="true">
                </asp:TreeView>
            </div>
      </td>
    </tr>
    <tr>
     <td colspan="2">
            <asp:Button runat="server" ID="btnApplyUnplannedToTeam" Text="Apply" OnClick="btnApplyUnplannedToTeam_Click"
                Visible="false" />
            <asp:Label runat="server" ID="lblApplyUnplannedStatus" Text="Saved!" ForeColor="Green"
                Visible="false"></asp:Label>
            <asp:PlaceHolder runat="server" ID="panelAddUnplanned">
                <button type="button" id="btnLinkAddUnplanned" onclick="$('.btnAddUnplanned').trigger('click');">
                    Add</button>
            </asp:PlaceHolder>
      </td>
    </tr>
</table>
<%--Add Unplanned Popup--%>
<asp:Button runat="server" CssClass="btnAddUnplanned" ID="btnAddUnplanned" Text="Add"
    Style="display: none" />
<asp:ModalPopupExtender runat="server" ID="modalPopupExtenderAddUnplanned" TargetControlID="btnAddUnplanned"
    PopupControlID="panelAddUnplannedPopup" PopupDragHandleControlID="divAddUnplannedPopupHeader"
    Drag="true" BackgroundCssClass="ModalPopupBG">
</asp:ModalPopupExtender>
<asp:Panel runat="server" ID="panelAddUnplannedPopup" DefaultButton="btnDoNothingUnplanned">
    <div class="AddUnplannedPopup">
        <div class="PopupHeader" id="divAddUnplannedPopupHeader">
            Add Unplanned Code</div>
        <div class="PopupBody">
            <asp:UpdatePanel runat="server" ID="upanelNewUnplannedCode">
                <ContentTemplate>
                 <table cellpadding="3" cellspacing="0">
                  <tr>
                   <td>
                    <asp:Label runat="server" ID="lblNewUnplannedTitle" Text="Title:" Font-Bold="true" AssociatedControlID="txtNewUnplannedTitle"></asp:Label>
                   </td>
                   <td><asp:TextBox runat="server" ID="txtNewUnplannedTitle" Width="325"></asp:TextBox></td>
                  </tr>
                  <tr>
                   <td valign="top">
                    <asp:Label runat="server" ID="lblNewUnplannedDescription" Text="Description:" Font-Bold="true" AssociatedControlID="txtNewUnplannedDescription"></asp:Label>
                   </td>
                   <td><asp:TextBox runat="server" ID="txtNewUnplannedDescription" TextMode="MultiLine" Width="325" Rows="5"></asp:TextBox></td>
                  </tr>
                  <tr>
                   <td>
                    <asp:Label runat="server" ID="lblNewUnplannedParent" Text="Parent:" Font-Bold="true" AssociatedControlID="ddlNewUnplannedParent"></asp:Label>
                   </td>
                   <td>
                    <asp:DropDownList runat="server" ID="ddlNewUnplannedParent"></asp:DropDownList>
                   </td>
                  </tr>
                  <tr>
                   <td><b>Active:</b></td>
                   <td><asp:CheckBox runat="server" ID="cbNewUnplannedActive" Checked="true" /></td>
                  </tr>
                  <tr id="rowApplyAll" runat="server">
                   <td valign="top"><b>Apply to all Teams:</b></td>
                   <td valign="top">
                    <table>
                     <tr>
                      <td valign="top"><asp:CheckBox ID="chkApplyAll" runat="server" onclick="UCApplyAllClick(this)" Checked="false" />&nbsp;</td>
                      <td id="UCApplyText" width="300" style="font-size: 8pt">
                        The unplanned code will not display for any teams in the directorate.
                        You will need to select each team on the parent page that will use this code and check it in the unplanned code tree list.
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
            <asp:Button runat="server" ID="btnSaveUnplannedPopup" Text="Save" OnClick="btnSaveNewUnplanned_Click" />
            <asp:Button runat="server" ID="btnCancelUnplannedPopup" Text="Cancel" OnClick="btnCancelUnplannedPopup_Click" />
            <asp:Button runat="server" ID="btnDoNothingUnplanned" Enabled="false" Style="display: none" />
        </div>
    </div>
</asp:Panel>
