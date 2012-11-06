<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskGrid.ascx.cs" Inherits="WALT.UIL.Controls.TaskGrid" %>
<%@ Register Assembly="AjaxControlToolkit, Version=3.5.51116.0, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e"
    Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>

<script type="text/JavaScript" language="JavaScript">

    function pageLoad() {
        $("li:contains('Not Visible')").css('background-image', 'url("/css/images/bg_nav_red.png")');
        $("li:contains('Not Visible')").css('padding-left', '25px');
        $("li:contains('Not Visible')").find('.dragHandle').css({ display: 'none' });
        $(".datepicker").datepicker();

        if ($('*[id*=closeReorderDialogHF]').val() == 'Yes') {
          
                var el = $('.columnSelector');
                var placeHolder = $('.columnSelectorMenu');
                var pos = placeHolder.show().position();
                               
                el.css({ display: 'inline', position: 'absolute',
                    marginLeft: 0, marginTop: 0,
                    top: pos.top, left: pos.left
                });
            }

            if ($('*[id*=filterShowHiddenField]').val() == 'Yes') {              
                $(".filterArrow").attr("src", "../css/images/arrow_down.png");
                $('.filterWrapper').show();
            }
           
            $('.columnSelectorMenu').click(function () {
            
                var el = $('.columnSelector');
                var placeHolder = $(this);
                var pos = placeHolder.show().position();

                el.css({ display: 'inline', position: 'absolute',
                    marginLeft: 0, marginTop: 0,
                    top: pos.top, left: pos.left
                });

                $('*[id*=closeReorderDialogHF]').val('Yes');
            });

            $('.saveColumnPref').click(function () {
                $('*[id*=savePrefHiddenField]').val('Yes');
            });

            $('.closeImg').click(function () {
                $('.columnSelector').css({ display: 'none' });
                $('*[id*=closeReorderDialogHF]').val('No');
            });

        }

</script>

<asp:UpdateProgress runat="server" ID="updateProgress1" AssociatedUpdatePanelID="GridUpdatePanel">
    <ProgressTemplate>
        <div id="progressBackgroundFilter">
        </div>
        <div id="processMessage">
            <img alt="Loading..." src="/css/images/ajax-loader.gif" />
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>

<asp:UpdatePanel ID="GridUpdatePanel" runat="server" >
    <ContentTemplate>
        <asp:HiddenField ID="hideColIndexHiddenField" runat="server" />
        <asp:HiddenField ID="closeReorderDialogHF" runat="server" />
            <asp:PlaceHolder runat="server" ID="emptyDataPlaceHolder">
            <div>
                There are no tasks that match your criteria.
            </div>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="taskQueuePlaceHolder">
            <div class="taskGridWrapperDiv">
                
            <span class="columnSelectorMenu">
                <img alt="Show" src="../css/images/arrow_right.png" />&nbsp;<a href="JavaScript:">Add/Remove Columns</a></span>                     
            <div class="columnSelector">
                <span class="staticSelectorMenu">
                    <img  alt="Show" src="../css/images/arrow_down.png" />&nbsp;Add/Remove Columns</span>    
               <div class="columnSelectorWrapper">
                <img class="closeImg" alt="Close" src="/css/images/Close-2-icon.png" style="float: right; margin-top: 5px; margin-right: 5px; cursor: pointer;" />          
                <div id="reorderDialog" style="width: 250px; margin-top:30px; margin-right:40px;">
                    <div class="reorderListDemo" style="padding-bottom: 15px">
                        <asp:ReorderList ID="rlItemList" DragHandleAlignment="Left" PostBackOnReorder="true"
                            CallbackCssStyle="callbackStyle" ItemInsertLocation="End" ShowInsertItem="false"
                            OnItemReorder="rlItemList_ItemReorder" runat="server">
                            <DragHandleTemplate>
                                <div class="dragHandle">
                                </div>
                            </DragHandleTemplate>
                            <ItemTemplate>
                                <div class="itemArea">
                                    <asp:Label ID="ItemName" runat="server" Text='<%# Eval("ItemName") %>'></asp:Label>
                                </div>
                            </ItemTemplate>
                            <ReorderTemplate>
                                <asp:Panel ID="Panel2" runat="server" CssClass="reorderCue" />
                            </ReorderTemplate>
                        </asp:ReorderList>                       
                            <div style=" width:100%; text-align:right;">
                        <asp:LinkButton ID="saveColPref" runat="server"  OnClick="saveColPref_Click">Save as Default</asp:LinkButton><br />                      
                        <asp:Label ID="prefSaveLabel" runat="server"  EnableViewState="False"></asp:Label>        
                            </div> 
                    </div>
                </div>
               </div>            
               </div>
               
                <table class="gridWrapper" cellpadding="0" cellspacing="0">                                 
                    <tr>
                        <td colspan="2">
                            <asp:GridView ID="taskGridView" runat="server" CssClass="Gridview" HeaderStyle-Wrap="False"
                                Width="100%" AllowSorting="True" AllowPaging="True" AlternatingRowStyle-CssClass="alt"
                                DataKeyNames="ID" AutoGenerateColumns="False" PageSize="25" OnPageIndexChanging="taskGridView_PageIndexChanging"
                                OnSorting="taskGridView_Sorting" OnRowDataBound="taskGridView_RowDataBound" OnDataBound="taskGridView_DataBound"
                                EnableViewState="False">
                                <AlternatingRowStyle CssClass="alt" />
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="idCheckBox" CssClass="idCheckBox" runat="server" />
                                        </ItemTemplate>
                                        <ItemStyle Width="15px" />
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="Inst" HeaderText="Inst" >
                                        <ItemTemplate>
                                            <asp:Image ID="imgInstantiated" runat="server" ImageUrl='<%# GetInstantiatedIconUrl((bool)Eval("Instantiated")) %>' />
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="Base" HeaderText="Base">
                                        <ItemTemplate>
                                            <asp:Image ID="imgBaseTask" runat="server" ImageUrl='<%# GetBaseIconUrl((bool)Eval("BaseTask")) %>' />
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="Title" HeaderText="Title" SortExpression="Title" >
                                        <ItemTemplate>
                                            <asp:Label ID="LabelTitle" runat="server" Text='<%# GetTaskUrl(Container.DataItem) %>'></asp:Label>                                            
                                        </ItemTemplate>
                                         <ItemStyle CssClass="testClass" />
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="TaskType" HeaderText="Task Type" SortExpression="TaskType">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelTaskType" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "TaskType.Title") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                     <asp:TemplateField AccessibleHeaderText="Originator" HeaderText="Created<br/>By"
                                         SortExpression="Originator">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelOriginator" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Originator.DisplayName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="Owner" HeaderText="Owner" SortExpression="Owner">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelOwner" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Owner.DisplayName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="Assigned" HeaderText="Assignee" SortExpression="Assigned">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelAssigned" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Assigned.DisplayName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField AccessibleHeaderText="Source" DataField="Source" HeaderText="Source"
                                        SortExpression="Source" NullDisplayText="N/A" />
                                    <asp:BoundField AccessibleHeaderText="SourceID" DataField="SourceID" HeaderText="Source<br/>ID"
                                        SortExpression="SourceID" />
                                    <asp:TemplateField AccessibleHeaderText="StartDate" HeaderText="Start<br/>Date" SortExpression="StartDate">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelSDate" runat="server" Text='<%# GetDate((DateTime?)Eval("StartDate")) %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="DueDate" HeaderText="Due<br/>Date" SortExpression="DueDate">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelDueDate" runat="server" Text='<%# GetDate((DateTime?)Eval("DueDate")) %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="Created" HeaderText="Created<br/>Date" SortExpression="Created">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelCreatedDate" runat="server" Text='<%# GetDateTime((DateTime?)Eval("Created")) %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="CompletedDate" HeaderText="Completed<br/>Date"
                                        SortExpression="CompletedDate">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelCompletedDate" runat="server" Text='<%# GetDateTime((DateTime?)Eval("CompletedDate")) %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField AccessibleHeaderText="OnHoldDate" HeaderText="On Hold<br/>Date"
                                        SortExpression="OnHoldDate">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelOnHoldDate" runat="server" Text='<%# GetDateTime((DateTime?)Eval("OnHoldDate")) %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField AccessibleHeaderText="Status" DataField="Status" HeaderText="Status"
                                        SortExpression="Status" />
                                    <asp:BoundField AccessibleHeaderText="Hours" DataField="Hours" HeaderText="Alloc<br/>Hours"
                                        SortExpression="Hours" HtmlEncode="False" />
                                    <asp:BoundField AccessibleHeaderText="Estimate" DataField="Estimate" HeaderText="R/E"
                                        SortExpression="Estimate" />
                                    <asp:BoundField AccessibleHeaderText="Complexity" DataField="Complexity" HeaderText="Complexity"
                                        SortExpression="Complexity" />
                                    <asp:BoundField AccessibleHeaderText="Spent" DataField="Spent" HeaderText="Hours<br/>Spent"
                                        HtmlEncode="False" />
                                    <asp:TemplateField AccessibleHeaderText="Program" HeaderText="Program" SortExpression="Program">
                                        <ItemTemplate>
                                            <asp:Label ID="LabelProgram" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Program.Title") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField AccessibleHeaderText="ExitCriteria" DataField="ExitCriteria" HeaderText="Exit<br/>Criteria"
                                        HtmlEncode="False">
                                        <ItemStyle VerticalAlign="Top" />
                                    </asp:BoundField>
                                    <asp:BoundField AccessibleHeaderText="WBS" DataField="WBS" HeaderText="WBS" SortExpression="WBS">
                                        <ItemStyle VerticalAlign="Top" />
                                    </asp:BoundField>
                                    <asp:BoundField AccessibleHeaderText="OwnerComments" DataField="OwnerComments" HeaderText="Owner<br/>Comments"
                                        HtmlEncode="False">
                                        <itemstyle verticalalign="Top" />
                                    </asp:BoundField>
                                    <asp:BoundField AccessibleHeaderText="AssigneeComments" DataField="AssigneeComments"
                                        HeaderText="Assignee<br/>Comments" HtmlEncode="False">
                                        <itemstyle verticalalign="Top" />
                                    </asp:BoundField>
                                </Columns>
                                <HeaderStyle Wrap="True" />
                                <PagerSettings Mode="NumericFirstLast" />
                                <PagerStyle Wrap="False" CssClass="pgr"></PagerStyle>
                            </asp:GridView>                            
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lblRowCount" runat="server" Text=""></asp:Label>
                        </td>
                        <td style="padding: 10px; text-align: right;">
                            <asp:DropDownList ID="PageSizeDropDownList" runat="server" OnSelectedIndexChanged="PageSizeDropDownList_SelectedIndexChanged"
                                AutoPostBack="True">
                                <asp:ListItem Selected="True">25</asp:ListItem>
                                <asp:ListItem>50</asp:ListItem>
                                <asp:ListItem>100</asp:ListItem>
                                <asp:ListItem>200</asp:ListItem>
                                <asp:ListItem>500</asp:ListItem>
                                <asp:ListItem>1000</asp:ListItem>
                            </asp:DropDownList>
                            Per Page
                        </td>
                    </tr>
                </table>                
            </div>
        </asp:PlaceHolder>    
    </ContentTemplate>
</asp:UpdatePanel>

