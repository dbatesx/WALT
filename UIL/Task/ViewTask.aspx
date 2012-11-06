<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewTask.aspx.cs" Inherits="WALT.UIL.Task.ViewTask"
    MasterPageFile="/Site1.Master" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/JavaScript" language="JavaScript">

        $(function () {
            if ($('.TaskTreeSelectedNode').length > 0) {

                var treeDivHeight = $('#TaskTreeViewDiv').height() / 2;
                var treeDivTop = $('#TaskTreeViewDiv').position().top;
                var selectPosTop = $('.TaskTreeSelectedNode').position().top;
                var scrolToTop = selectPosTop - treeDivHeight - treeDivTop;

                $('#TaskTreeViewDiv').scrollTop(scrolToTop);
            }

            if ($('#FormTable').length > 0) {
                var myheight = $('#FormTable').height();
                $('#TaskTreeViewDiv').height(myheight - 22);
            }
        });       
    </script>
    <asp:PlaceHolder ID="ViewFormPlaceHolder" runat="server">
        <table cellpadding="0" cellspacing="0" width="100%;">
            <tr>
                <td width="350px" style="width: 350px; padding-right: 10px;">
                    <table cellpadding="0" cellspacing="0" width="100%">
                        <tr>
                            <td>
                            </td>
                            <td width="15px" style="border-right: 3px solid white;">
                                <asp:HyperLink ID="PreLink" runat="server" ImageUrl="~/css/images/UpButton.png" AlternateText="Previous"
                                    CausesValidation="false"></asp:HyperLink>
                            </td>
                            <td width="15px">
                                <asp:HyperLink ID="NextLink" runat="server" ImageAlign="Middle" ImageUrl="~/css/images/DownButton.png"
                                    CausesValidation="false">                                 
                                </asp:HyperLink>
                            </td>
                        </tr>
                    </table>
                </td>
                <td colspan="2">
                    <asp:Button ID="CloseButton1" runat="server" Text="Cancel" OnClick="Close_Click"
                        Width="100px" CssClass="btnStyleLeft" />
                    <asp:Button ID="RejectButton1" runat="server" Text="Reject" Width="100px" CssClass="btnStyleLeft"
                        OnClientClick="if( !confirm('Are you sure you want to reject this task?')) { return false;}"
                        OnClick="Reject_Click" Visible="false" />
                    <asp:Button ID="DeleteButton1" runat="server" Text="Delete" Width="100px" CssClass="btnStyleLeft"
                        OnClientClick="if( !confirm('Are you sure you want to delete this task?')) { return false;}"
                        OnClick="Delete_Click" Visible="false" />
                    <asp:Button ID="EditButton1" runat="server" Text="Edit" Width="100px" CssClass="btnStyleRight"
                        Visible="false" />
                    <asp:Button ID="AddChildButton1" runat="server" Text="Add Child" Width="100px" CssClass="btnStyleRight"
                        Visible="false" />
                </td>
            </tr>
            <tr>
                <td id="TaskTreeViewCell" valign="top" width="350px" style="width: 350px;" class="leftCell">
                    <div id="TaskTreeViewDiv" style="height: 300px; width: 350px; padding: 10px; overflow-y: auto;">
                        <asp:TreeView ID="TreeView1" ExpandDepth="1" runat="server" OnPreRender="TreeView1_PreRender"
                            ShowLines="True" Width="100%" NodeWrap="true" SelectedNodeStyle-VerticalPadding="3px"
                            SelectedNodeStyle-HorizontalPadding="3px" NodeStyle-CssClass="treeNodeStyle"
                            NodeStyle-HorizontalPadding="3px" NodeStyle-VerticalPadding="3px" SelectedNodeStyle-ForeColor="Black">
                            <SelectedNodeStyle CssClass="SelectedNode" Font-Underline="true" Font-Italic="true"
                                Font-Bold="true" />
                        </asp:TreeView>
                    </div>
                </td>
                <td valign="top">
                    <table id="FormTable" cellpadding="0" cellspacing="0" width="100%;">
                        <tr>
                            <td class="textCell">
                                Title
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="titleLabel" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="parentPlaceHolder" runat="server">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell" style="height: 20px;">
                                                Parent
                                            </td>
                                            <td class="inputCell">
                                                <asp:Label ID="parentStaticLabel" runat="server" Text=""></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Status
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="StatusLabel" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Created Date
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="CreatedLabel" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="CompletedPlaceHolder" runat="server" Visible="false">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell" style="height: 20px;">
                                                Completed Date
                                            </td>
                                            <td class="inputCell">
                                                <asp:Label ID="CompletedLabel" runat="server" Text=""></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="OnHoldPlaceHolder" runat="server" Visible="false">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell" style="height: 20px;">
                                                On Hold Date
                                            </td>
                                            <td class="inputCell">
                                                <asp:Label ID="OnHoldLabel" runat="server" Text=""></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Program
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="programLabel" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                WBS
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="WBSLabel" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Hours Allocated
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="hrsTextBox" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="fullyAllocatedPlaceHolder" runat="server" Visible="false">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell">
                                                Fully Allocated
                                            </td>
                                            <td class="inputCell">
                                                <asp:Label ID="fullyAllocatedLabel" runat="server" Text=""></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Hours Spent
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="hrsSpendLabel" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Requested Start Date
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="ReqStartLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Due Date
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="DueDateLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Created By
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="CreatedByLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Owner
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="OwnerLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Assignee
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="assigneeLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Task Type
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="TaskTypeLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="ComplexityPlaceHolder" runat="server" Visible="False">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell">
                                                Complexity
                                            </td>
                                            <td class="inputCell">
                                                <asp:Label ID="ComplexityLabel" runat="server" Text=""></asp:Label>&nbsp;
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="REPlaceHolder" runat="server" Visible="False">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell">
                                                R/E
                                            </td>
                                            <td class="inputCell">
                                                <asp:Label ID="RELabel" runat="server" Text=""></asp:Label>&nbsp;
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>                       
                        <tr>
                            <td class="textCell">
                                Owner Comments
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="OwnerCommentsLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Assignee Comments
                            </td>
                            <td class="inputCell">
                                <asp:Label ID="AssigneeCommentsLabel" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell" style="border-bottom: 1px solid #C0C0C0;">
                                Exit Criteria
                            </td>
                            <td class="inputCell" style="border-bottom: 1px solid #C0C0C0;">
                                <asp:Label ID="ExitCritriaTextBox" runat="server" Text=""></asp:Label>&nbsp;
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;
                </td>
                <td>
                    <asp:Button ID="CloseButton2" runat="server" Text="Cancel" OnClick="Close_Click"
                        Width="100px" CssClass="btnStyleLeft" />
                    <asp:Button ID="RejectButton2" runat="server" Text="Reject" Width="100px" CssClass="btnStyleLeft"
                        OnClientClick="if( !confirm('Are you sure you want to reject this task?')) { return false;}"
                        OnClick="Reject_Click" Visible="false" />
                    <asp:Button ID="DeleteButton2" runat="server" Text="Delete" Width="100px" CssClass="btnStyleLeft"
                        OnClientClick="if( !confirm('Are you sure you want to delete this task?')) { return false;}"
                        OnClick="Delete_Click" Visible="false" />
                    <asp:Button ID="EditButton2" runat="server" Text="Edit" Width="100px" CssClass="btnStyleRight"
                        Visible="false" />
                    <asp:Button ID="AddChildButton2" runat="server" Text="Add Child" Width="100px" CssClass="btnStyleRight"
                        Visible="false" />
                </td>
            </tr>
        </table>
    </asp:PlaceHolder>
</asp:Content>
