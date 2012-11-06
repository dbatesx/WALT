<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TaskForm.aspx.cs" Inherits="WALT.UIL.Task.TaskForm"
    MasterPageFile="/Site1.Master" EnableEventValidation="false" %>

<%@ Register TagPrefix="asp" TagName="ProfileSelector" Src="~/Controls/ProfileSelector.ascx" %>


<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/JavaScript" language="JavaScript">
       
        function changeParent() {
            var selectedParent = $('[id=<%= parentDropDownList.ClientID %>]').val();

            if (selectedParent != null) {
                $('[id=<%= parentIdHiddenField.ClientID %>]').val(selectedParent);
            }
        }

        function LoadFromFAv() {            
            $(" #favDiv").show(); 
        }
        $(document).ready(function () {

            $(".datepicker").datepicker({ minDate: 0 });

            $('#<%= favDropDownList.ClientID %>').change(function () {
                if (!confirm('You will lose all changes that you\'ve made to this task. Are you sure you want to load your favorite?')) {
                    return false;
                }
                else {

                 }
            });

            $('.changeParentBtn').click(function () {
                $('#parentStaticDiv').hide();
                $('#parentEditDiv').show();
            });

            $('.cancelParentBtn').click(function () {
                $('*[id*=parentIdHiddenField]').val("-1");

                $('#parentStaticDiv').show();
                $('#parentEditDiv').hide();
            });
        });    

        function TaskTypeClientValidate(sender, args) {
            var selectedTaskType = $("[id*='taskTypeHiddenField']").val();
            
            if (selectedTaskType != null && selectedTaskType != 0) {               
                args.IsValid = true;
            }
            else {               
                args.IsValid = false;
            }
        }


        function UnplannedClientValidate(sender, args) {
            var selectedUnplanned = $("[id*='UnplannedHiddenField']").val();
         
        
            if (selectedUnplanned != null && selectedUnplanned != 0) {               
                args.IsValid = true;
            }
            else {               
                args.IsValid = false;
            }
        }

       
    </script>
    <asp:PlaceHolder ID="TaskFormPlaceHolder" runat="server">       
        <table cellpadding="0" cellspacing="0" width="100%;">
            <tr>
                <td width="350px" style="width: 350px; padding-right:10px;">
                    <table cellpadding="0" cellspacing="0" width="100%">
                        <tr>
                            <td width="360px">
                                <asp:Label ID="HeaderTitleLabel" runat="server" Text="" Font-Bold="true"></asp:Label>
                            </td>
                            <td width="15px" style="border-right: 3px solid white;">
                                <asp:HyperLink ID="PreLink" CssClass="NextPreLinks" runat="server" ImageUrl="~/css/images/UpButton.png"
                                    AlternateText="Previous" CausesValidation="false"></asp:HyperLink>
                            </td>
                            <td width="15px">
                                <asp:HyperLink ID="NextLink" CssClass="NextPreLinks" runat="server" ImageAlign="Middle"
                                    ImageUrl="~/css/images/DownButton.png" AlternateText="Next" CausesValidation="false">                                 
                                </asp:HyperLink>
                            </td>
                        </tr>
                    </table>
                </td>
                <td>
                    <asp:Button ID="CancelButton1" runat="server" Text="Cancel" OnClick="Close_Click" UseSubmitBehavior="false"
                        Width="100px" CssClass="btnStyleLeft" CausesValidation="False" OnClientClick="if( !confirm('Are you sure you want to leave this page without saving?')) { return false;}" />
                    <asp:Button ID="OKButton1" runat="server" Text="Save" OnClick="SaveButton_Click"
                        Width="100px" CssClass="btnStyleRight" UseSubmitBehavior="false" />
                    <asp:Button ID="saveNewButton1" runat="server" Text="" OnClick="SaveButton_Click"
                        Width="200px" Visible="false" CssClass="btnStyleRight" UseSubmitBehavior="false" />
                    <asp:Button ID="BtnFav" runat="server" Text="Load from Favorites" OnClientClick="LoadFromFAv(); return false;"
                        CssClass="btnStyleRight" CausesValidation="false" UseSubmitBehavior="false" />
                </td>
            </tr>
            <tr>
                <td id="TaskTreeViewCell" valign="top" width="350px" style="width: 350px;" class="leftCell">
                    <div id="TaskTreeViewDiv" style="height: 300px; width: 300px; padding: 10px; overflow-y: auto;">
                        <asp:TreeView ID="TaskTreeView" ExpandDepth="1" runat="server" OnPreRender="TaksTreeView_PreRender"
                            ShowLines="True" Width="100%" NodeWrap="true" SelectedNodeStyle-VerticalPadding="3px"
                            SelectedNodeStyle-HorizontalPadding="3px" NodeStyle-CssClass="treeNodeStyle"
                            NodeStyle-HorizontalPadding="3px" NodeStyle-VerticalPadding="3px" SelectedNodeStyle-ForeColor="Black">
                            <SelectedNodeStyle CssClass="TaskTreeSelectedNode" Font-Underline="true" Font-Italic="true"
                                Font-Bold="true" />
                        </asp:TreeView>
                    </div>
                </td>
                <td valign="top">
                    <table id="FormTable" cellpadding="0" cellspacing="0" width="100%;">
                        <tr>
                            <td colspan="2">
                                <div id="favDiv" style="display: none; width: 100%;">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell" style="height: 20px;">
                                                Favorites
                                            </td>
                                            <td class="inputCell">
                                                <asp:DropDownList ID="favDropDownList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="favDropDownList_SelectedIndexChanged">
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Title<span style="font-size: 12px; color: red;"> *</span>
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="titleEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="titleTextBox" runat="server" Width="99%"></asp:TextBox>                                                                     
                                    <asp:RequiredFieldValidator ID="titleRequiredFieldValidator" runat="server" ControlToValidate="titleTextBox"
                                        ErrorMessage="<br />Title is a required field." ForeColor="Red" Display="Dynamic"
                                        ></asp:RequiredFieldValidator>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="titleStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="titleLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="parentPlaceHolder" runat="server">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell" style=" height:20px;">
                                                Parent
                                            </td>
                                            <td class="inputCell">
                                                <asp:PlaceHolder ID="parentEditPlaceHolder" runat="server">
                                                    <div id="parentEditDiv" style="display: none; width: 100%;">
                                                        <asp:DropDownList ID="parentDropDownList" runat="server" AutoPostBack="false" onChange="javascript:changeParent()">
                                                            
                                                        </asp:DropDownList>
                                                        <span class="cancelParentBtn" style="float: right;"><a href="JavaScript:">Cancel</a></span>
                                                        <asp:HiddenField ID="parentIdHiddenField" runat="server" />
                                                    </div>
                                                    <div id="parentStaticDiv" style="width: 100%;">
                                                        <asp:Label ID="parentTitleLabel" runat="server" Text=""></asp:Label>
                                                        <span class="changeParentBtn" style="float: right;"><a href="JavaScript:">Change Parent</a></span>
                                                    </div>
                                                </asp:PlaceHolder>
                                                <asp:PlaceHolder ID="parentStaticPlaceHolder" runat="server" Visible="false">
                                                    <asp:Label ID="parentStaticLabel" runat="server" Text=""></asp:Label>
                                                </asp:PlaceHolder>                                               
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
                                <asp:PlaceHolder ID="statusEditPlaceHolder" runat="server">
                                    <asp:DropDownList ID="statusDropDownList" runat="server">
                                    </asp:DropDownList>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="statusStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="statusLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
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
                            <td class="textCell">
                                Program<span style="font-size: 12px; color: red;"> *</span>
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="programEditPlaceHolder" runat="server">
                                    <asp:DropDownList ID="programDropDownList" runat="server" CausesValidation="true">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="programRequiredFieldValidator" Text="<br />Program is a required field."
                                        InitialValue="none" ControlToValidate="ProgramDropDownList" runat="server" ForeColor="Red"
                                        Display="Dynamic"  />
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="programStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="programLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                WBS
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="wbsEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="wbsTextBox" runat="server" Width="250px"></asp:TextBox>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="wbsStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="wbsLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Hours Allocated
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="hrsEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="hrsTextBox" runat="server" Width="250px"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="hrsTextBox"
                                        ValidationExpression="^(\.\d)|(\d+(\.\d)?)$" ErrorMessage="<br />Please Enter a valid hours"
                                        ForeColor="Red" Display="Dynamic"></asp:RegularExpressionValidator>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="hrsStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="hrsLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                                <asp:Label ID="NotAllocatedLabel" runat="server" Text="" Visible="false"></asp:Label>
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
                                                <asp:PlaceHolder ID="fullyAllocatedEditPlaceHolder" runat="server">
                                                    <asp:CheckBox ID="fullyAllocatedCheckBox" runat="server" />
                                                </asp:PlaceHolder>
                                                <asp:PlaceHolder ID="fullyAllocatedStaticPlaceHolder" runat="server" Visible="false">
                                                    <asp:Label ID="fullyAllocatedLabel" runat="server" Text=""></asp:Label>
                                                </asp:PlaceHolder>                                                
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
                                <asp:PlaceHolder ID="reqStartEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="reqStartTextBox" runat="server" CssClass="datepicker" Width="250px"></asp:TextBox>                                   
                                    <asp:CompareValidator ID="reqStartValidator" runat="server" ControlToValidate="reqStartTextBox"
                                        Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                        ForeColor="Red" Display="Dynamic" ></asp:CompareValidator>
                                    <asp:CompareValidator ID="reqStartCompareValidator" runat="server" ControlToCompare="dueDateTextBox"
                                        ControlToValidate="reqStartTextBox" Display="Dynamic" ErrorMessage="<br />Start date must be before due date."
                                        Operator="LessThanEqual" Type="Date" ValueToCompare="<%= reqStartTextBox.Text.ToShortString() %>"
                                        ></asp:CompareValidator>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="reqStartStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="reqStartLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Due Date
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="dueDateEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="dueDateTextBox" runat="server" CssClass="datepicker" Width="250px"></asp:TextBox>                                   
                                    <asp:CompareValidator ID="dueDateValidator" runat="server" ControlToValidate="dueDateTextBox"
                                        Type="Date" Operator="DataTypeCheck" ErrorMessage="<br />Please Enter a valid date in the format (mm/dd/yyyy)"
                                        ForeColor="Red" Display="Dynamic" Font-Size="10px"></asp:CompareValidator>
                                    <asp:CompareValidator ID="StartDueCompareValidator" runat="server" ControlToCompare="reqStartTextBox"
                                        ControlToValidate="dueDateTextBox" Display="Dynamic" ErrorMessage="<br />Due date must be after start date."
                                        Operator="GreaterThanEqual" Type="Date" ValueToCompare="<%= dueDateTextBox.Text.ToShortString() %>"
                                        ></asp:CompareValidator>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="dueDateStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="dueDateLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:PlaceHolder ID="ownerPlaceHolder" runat="server">
                                    <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                        <tr>
                                            <td class="textCell">
                                                Owner<span style="font-size: 12px; color: red;"> *</span>
                                            </td>
                                            <td class="inputCell">
                                                <asp:PlaceHolder ID="ownerEditPlaceHolder" runat="server">
                                                    <asp:ProfileSelector runat="server" ID="ownerProfileSelector" PreLoadWithTeammates="true" IsRequired="true" AllowMultiple="false">
                                                    </asp:ProfileSelector>
                                                </asp:PlaceHolder>
                                                <asp:PlaceHolder ID="ownerStaticPlaceHolder" runat="server" Visible="false">
                                                    <asp:Label ID="ownerLabel" runat="server" Text=""></asp:Label>
                                                </asp:PlaceHolder>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Assignee<span style="font-size: 12px; color: red;"> *</span>
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="assigneeEditPlaceHolder" runat="server">
                                    <asp:ProfileSelector runat="server" ID="assigneeProfileSelector" IsRequired="true"
                                        PreLoadWithTeammates="true" AllowMultiple="false">
                                    </asp:ProfileSelector>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="assigneeStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="assigneeLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:UpdatePanel runat="server" ID="TaskTypeUpdatePanel" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:PlaceHolder ID="TaskTypePlaceHolder" runat="server" Visible="false">
                                            <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                                <tr>
                                                    <td class="textCell" style="width: 183px; min-width: 183px;">
                                                        Task Type
                                                        <asp:Label ID="TaskTypeReqSign" runat="server" Text=" *" ForeColor="Red" Visible="false"></asp:Label>
                                                    </td>
                                                    <td class="inputCell">
                                                        <asp:PlaceHolder ID="taskTypeEditPlaceHolder" runat="server">
                                                            <table width="100%" cellpadding="0" cellspacing="0" style="height: 240px; width: 100%;">
                                                                <tr>
                                                                    <td valign="top" style="width: 235px;">
                                                                        <div id="taskTypeTreeViewDiv" style="height: 220px; width: 225px;
                                                                            overflow-y: auto; padding: 10px; background-color: White; border: 1px solid #C0C0C0;">
                                                                            <asp:TreeView ID="TaskTypeTreeView" CssClass="tree" ExpandDepth="1" runat="server" OnPreRender="TaskTypeTreeView_PreRender"
                                                                                OnSelectedNodeChanged="TaskTypeTreeView_SelectedNodeChanged" Width="100%" NodeWrap="true"
                                                                                SelectedNodeStyle-VerticalPadding="3px" SelectedNodeStyle-HorizontalPadding="3px"
                                                                                NodeStyle-CssClass="treeNodeStyle" NodeStyle-HorizontalPadding="3px" NodeStyle-VerticalPadding="3px"
                                                                                SelectedNodeStyle-ForeColor="Black" EnableViewState="True">
                                                                                <SelectedNodeStyle Font-Bold="True" Font-Italic="True" Font-Underline="True" CssClass="TypeSelectedNode"/>
                                                                            </asp:TreeView>
                                                                            <asp:HiddenField ID="taskTypeHiddenField" runat="server" />                                                                            
                                                                        </div>                                                                        
                                                                        <asp:CustomValidator ID="taskTypeCustomValidator" Enabled="false" runat="server"
                                                                            ClientValidationFunction="TaskTypeClientValidate" ErrorMessage="Task Type is a required field."
                                                                            ForeColor="Red" Display="Dynamic" ></asp:CustomValidator>
                                                                    </td>
                                                                    <td valign="top">
                                                                        <div style="height: 220px; width: 225px; overflow-y: auto; margin-left: 15px;
                                                                            background-color: White; border: 1px solid #C0C0C0; padding: 10px;">
                                                                            <asp:Label ID="taskTypeDescLabel" runat="server" Text=""></asp:Label>
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </asp:PlaceHolder>
                                                        <asp:PlaceHolder ID="taskTypeStaticPlaceHolder" runat="server" Visible="false">
                                                            <asp:Label ID="taskTypeLabel" runat="server" Text=""></asp:Label>
                                                        </asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:PlaceHolder>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:UpdatePanel runat="server" ID="complexityUpdatePanel" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:PlaceHolder ID="complexityPlaceHolder" runat="server" Visible="false">
                                            <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                                <tr>
                                                    <td class="textCell">
                                                        Complexity
                                                        <asp:Label ID="complexityReqSign" runat="server" Text=" *" ForeColor="Red" Visible="false"></asp:Label>
                                                    </td>
                                                    <td class="inputCell">
                                                        <asp:PlaceHolder ID="complexityEditPlaceHolder" runat="server">
                                                            <asp:DropDownList ID="complexityDropDownList" runat="server" AutoPostBack="true"
                                                                OnSelectedIndexChanged="complexityDropDownList_SelectedIndexChanged1">
                                                            </asp:DropDownList>
                                                            <asp:RequiredFieldValidator ID="complexityRequiredFieldValidator" Text="<br />Complexity is a required field."
                                                                InitialValue="none" ControlToValidate="complexityDropDownList" runat="server"
                                                                ForeColor="Red" Display="Dynamic" Enabled="false" />
                                                            <asp:HiddenField ID="complexityHiddenField" runat="server" />
                                                        </asp:PlaceHolder>
                                                        <asp:PlaceHolder ID="complexityStaticPlaceHolder" runat="server" Visible="false">
                                                            <asp:Label ID="complexityLabel" runat="server" Text=""></asp:Label>
                                                        </asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:PlaceHolder>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:UpdatePanel runat="server" ID="REUpdatePanel" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:PlaceHolder ID="rePlaceHolder" runat="server" Visible="false">
                                            <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                                <tr>
                                                    <td class="textCell" style="width: 180">
                                                        R/E
                                                        <asp:Label ID="REReqSign" runat="server" Text=" *" ForeColor="Red" Visible="false"></asp:Label>
                                                    </td>
                                                    <td class="inputCell">
                                                        <asp:PlaceHolder ID="reEditPlaceHolder" runat="server">
                                                            <asp:TextBox ID="reTextBox" runat="server" Width="250px"></asp:TextBox>                                                           
                                                            <asp:RegularExpressionValidator ID="RERegularExpressionValidator" runat="server"
                                                                ControlToValidate="reTextBox" ValidationExpression="^(\.\d)|(\d+(\.\d)?)$" ErrorMessage="Please Enter a valid R/E"
                                                                ForeColor="Red" Display="Dynamic" ></asp:RegularExpressionValidator>
                                                            <asp:RequiredFieldValidator ID="reRequiredFieldValidator" runat="server" ControlToValidate="reTextBox"
                                                                ErrorMessage="<br />R/E is a required field." ForeColor="Red" Display="Dynamic"
                                                                Enabled="false" ></asp:RequiredFieldValidator>
                                                        </asp:PlaceHolder>
                                                        <asp:PlaceHolder ID="reStaticPlaceHolder" runat="server" Visible="false">
                                                            <asp:Label ID="reLabel" runat="server" Text=""></asp:Label>
                                                        </asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:PlaceHolder>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>                       
                        <tr>
                            <td colspan="2">
                                <asp:UpdatePanel runat="server" ID="unplannedUpdatePanel" UpdateMode="Conditional">                                    
                                    <ContentTemplate>
                                        <asp:PlaceHolder ID="unplannedPlaceHolder" runat="server" Visible="false">
                                            <table cellpadding="0" cellspacing="0" width="100%" style="width: 100%;">
                                                <tr>
                                                    <td class="textCell" style="width: 183px; min-width: 183px;">
                                                        Unplanned Code<span style="font-size: 12px; color: red;"> *</span>
                                                    </td>
                                                    <td class="inputCell">
                                                        <asp:PlaceHolder ID="unplannedEditPlaceHolder" runat="server">
                                                            <table cellpadding="0" cellspacing="0" style="height: 240px;">
                                                                <tr>
                                                                    <td valign="top">
                                                                        <div id="UnplannedTreeViewDiv" style="height: 220px; width: 225px;
                                                                            overflow-y: auto; padding: 10px; background-color: White; border: 1px solid #C0C0C0;">
                                                                            <asp:TreeView ID="unplannedTreeView" CssClass="tree" ExpandDepth="1" runat="server"
                                                                                NodeWrap="true" OnPreRender="unplannedTreeView_PreRender" OnSelectedNodeChanged="unplannedTreeView_SelectedNodeChanged"
                                                                                Width="100%" SelectedNodeStyle-VerticalPadding="3px" SelectedNodeStyle-HorizontalPadding="3px"
                                                                                NodeStyle-CssClass="treeNodeStyle" NodeStyle-HorizontalPadding="3px" NodeStyle-VerticalPadding="3px"
                                                                                SelectedNodeStyle-ForeColor="Black">
                                                                                <SelectedNodeStyle Font-Bold="True" Font-Italic="True" Font-Underline="True" CssClass="unplannedSelectedNode"/>
                                                                            </asp:TreeView>
                                                                            <asp:HiddenField ID="UnplannedHiddenField" runat="server" />                                                                            
                                                                        </div>                                                                       
                                                                        <asp:CustomValidator ID="UnplannedCustomValidator" runat="server" ClientValidationFunction="UnplannedClientValidate"
                                                                            ErrorMessage="Unplanned Code is a required field." ForeColor="Red" Display="Dynamic"
                                                                            ></asp:CustomValidator>
                                                                    </td>
                                                                    <td valign="top">
                                                                        <div style="height: 220px; width: 225px; overflow-y: auto; margin-left: 15px;
                                                                            background-color: White; border: 1px solid #C0C0C0; padding: 10px;">
                                                                            <asp:Label ID="unplannedDescLabel" runat="server" Text=""></asp:Label>
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </asp:PlaceHolder>
                                                        <asp:PlaceHolder ID="unplannedStaticPlaceHolder" runat="server" Visible="false">
                                                            <asp:Label ID="unplannedLabel" runat="server" Text=""></asp:Label>
                                                        </asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:PlaceHolder>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Owner Comments
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="ownerCommentsEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="ownerCommentsTextBox" runat="server" Height="150px" TextMode="MultiLine"
                                        Width="99%"></asp:TextBox>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ownerCommentsStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="ownerCommentsLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell">
                                Assignee Comments
                            </td>
                            <td class="inputCell">
                                <asp:PlaceHolder ID="assigneeCommentsEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="assigneeCommentsTextBox" runat="server" Height="150px" TextMode="MultiLine"
                                        Width="99%"></asp:TextBox>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="assigneeCommentsStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="assigneeCommentsLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                        <tr>
                            <td class="textCell" style="border-bottom: 1px solid #C0C0C0;">
                                Exit Criteria
                            </td>
                            <td class="inputCell" style="border-bottom: 1px solid #C0C0C0;">
                                <asp:PlaceHolder ID="exitCritriaEditPlaceHolder" runat="server">
                                    <asp:TextBox ID="exitCritriaTextBox" runat="server" Height="150px" TextMode="MultiLine"
                                        Width="99%"></asp:TextBox>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="exitCritriaStaticPlaceHolder" runat="server" Visible="false">
                                    <asp:Label ID="exitCritriaLabel" runat="server" Text=""></asp:Label>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
            <td></td>
            <td>
                <asp:Button ID="Button1" runat="server" Text="Cancel" OnClick="Close_Click" Width="100px" UseSubmitBehavior="false"
                    CssClass="btnStyleLeft" CausesValidation="False" OnClientClick="if( !confirm('Are you sure you want to leave this page without saving?')) { return false;}" />
                <asp:Button ID="Button2" runat="server" Text="Save" OnClick="SaveButton_Click" Width="100px" 
                    CssClass="btnStyleRight" UseSubmitBehavior="false" />
                <asp:Button ID="saveNewButton2" runat="server" Text="" OnClick="SaveButton_Click"
                    Visible="false" CssClass="btnStyleRight" UseSubmitBehavior="false" />
            </td>
            </tr>
        </table>
    </asp:PlaceHolder>    
  
    <asp:CustomValidator ID="TaskFormCustomValidator" runat="server" ErrorMessage=""
        Display="Dynamic" OnServerValidate="TaskFormCustomValidator_ServerValidate"></asp:CustomValidator>
</asp:Content>
