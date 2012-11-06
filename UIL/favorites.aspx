<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="favorites.aspx.cs" Inherits="WALT.UIL.favorites" MasterPageFile="Site1.Master" %>
<%@ Register TagPrefix="cc" TagName="TreeSelector" Src="~/Controls/TreeSelector.ascx" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

 <script type="text/javascript">
     function ValidateHours(input, maxCheck) {
         if (input.value.length > 0 && (isNaN(parseFloat(input.value)) ||
            parseFloat(input.value) < 0 || (maxCheck == 1 && parseFloat(input.value) > 24))) {

             alert("Please enter a valid number of hours");
             setTimeout("document.getElementById('" + input.id + "').focus();document.getElementById('" + input.id + "').select();", 0);
             return;
         }
         else {
             var idx = input.value.indexOf(".")

             if (idx != -1 && input.value.length - idx > 2) {
                 input.value = input.value.substr(0, idx + 2);
             }
         }
     }

    function PopTaskType(id)
    {
        var prefix = GetPrefix();
        $('#favID').val(id);
        $('#ttFavTitle').html($('#' + prefix + 'txtTitle_' + id).val());
        SelectNode("treeTaskType", $('#' + prefix + "hdnTaskTypeID_" + id).val(), false, true);
        $('#' + prefix + 'popupTaskType').dialog('open');
        return false;
    }

    function CloseTaskType()
    {
        $("'div[id$=\"popupTaskType\"]'").dialog('close');
    }

    function GetPrefix() {
        var prefix = $('input[id$="_prefix"]').attr('id');
        return prefix.substr(0, prefix.length - 6);
    }

    function UpdateTaskType() {
        var prefix = GetPrefix();
        var id = $('#favID').val();
        var tt = GetTreeSelectedValue('treeTaskType');
        
        $('#' + prefix + "hdnTaskTypeID_" + id).val(GetTreeSelectedID('treeTaskType'));
        $('#' + prefix + "hdnTaskTypeVal_" + id).val(tt);
        $('#ttTitle_' + id).html(tt);
        CloseTaskType();
    }

    function UpdateRE(id, id2, val) {
        var re = eval('compRE_' + id2 + '_' + val);
        $('#reDiv_' + id).html(re);
        $('#re_' + id).val(re);
    }

    function ValidateUpdates() {
        var success = true;

        $('input[id*="txtTitle_"]').each(function () {
            if (this.value == '') {
                alert('Please enter a title');
                this.focus();
                success = false;
                return false;
            }
        });

        return success;
    }
    
 </script>

 <asp:Label ID="lblCompScript" runat="server"></asp:Label>

 <asp:PlaceHolder ID="phDialogScript" Visible="false" runat="server">
  <script type="text/javascript">
    $(function () {
        $('div[id$="popupTaskType"]').dialog({
            autoOpen: false,
            width: 300,
            modal: true,
            resizable: false,
            open: function (type, data) { $(this).parent().appendTo('form'); }
        });
    });
  </script>
 </asp:PlaceHolder>

 <asp:PlaceHolder ID="phHeader" runat="server"></asp:PlaceHolder>
 <asp:PlaceHolder ID="phCompScript" runat="server"></asp:PlaceHolder>

 <asp:HiddenField ID="prefix" runat="server" />
 <input type="hidden" name="favID" id="favID" />

 <asp:Table ID="outerTbl" runat="server" CssClass="weeklyOuterTbl">
  <asp:TableRow>
   <asp:TableCell ColumnSpan="2" style="font-size: 16pt"><b>Favorites</b></asp:TableCell>
  </asp:TableRow>
  <asp:TableRow>
   <asp:TableCell ColumnSpan="2">
    <asp:Table CellSpacing="0" CellPadding="5" runat="server">
     <asp:TableRow ID="rowLinkBtns">
      <asp:TableCell>      
       <asp:LinkButton id="lnkCreate" Text="Create Task From Favorite" runat="server" OnClick="lnkCreate_Click" />
      </asp:TableCell>
      <asp:TableCell>
       <asp:LinkButton id="lnkEdit" Text="Edit" runat="server" OnClick="lnkEdit_Click" />
      </asp:TableCell>
      <asp:TableCell>
       <asp:LinkButton id="lnkDelete" Text="Delete" runat="server" OnClientClick="return confirm('Are you sure you want to delete the selected favorites?')" OnClick="lnkDelete_Click" />
      </asp:TableCell>
     </asp:TableRow>
    </asp:Table>
   </asp:TableCell>
  </asp:TableRow>
  <asp:TableRow>
   <asp:TableCell ColumnSpan="2">
    <asp:Table ID="tblFavorites" runat="server" class="weeklyTbl">
     <asp:TableHeaderRow>
      <asp:TableHeaderCell ColumnSpan="2" ID="hdrTitle">Title</asp:TableHeaderCell>
      <asp:TableHeaderCell ColumnSpan="2">Task Type</asp:TableHeaderCell>
      <asp:TableHeaderCell>Program</asp:TableHeaderCell>
      <asp:TableHeaderCell>Hours Allocated</asp:TableHeaderCell>
      <asp:TableHeaderCell ID="hdrComp">Complexity</asp:TableHeaderCell>
      <asp:TableHeaderCell>R/E</asp:TableHeaderCell>
      <asp:TableHeaderCell>Template</asp:TableHeaderCell>
      <asp:TableHeaderCell Width="30">Mon</asp:TableHeaderCell>
      <asp:TableHeaderCell Width="30">Tue</asp:TableHeaderCell>
      <asp:TableHeaderCell Width="30">Wed</asp:TableHeaderCell>
      <asp:TableHeaderCell Width="30">Thu</asp:TableHeaderCell>
      <asp:TableHeaderCell Width="30">Fri</asp:TableHeaderCell>
      <asp:TableHeaderCell Width="30">Sat</asp:TableHeaderCell>
      <asp:TableHeaderCell Width="30">Sun</asp:TableHeaderCell>
     </asp:TableHeaderRow>
    </asp:Table>
   </asp:TableCell>
  </asp:TableRow>
  <asp:TableRow>
   <asp:TableCell>
    <asp:Button ID="btnAdd" Text="Create Favorite" runat="server" OnClick="btnAdd_Click" />
   </asp:TableCell>
   <asp:TableCell HorizontalAlign="Right">
    <asp:Button ID="btnUpdate" Text="Update" runat="server" onClientClick="return ValidateUpdates()" OnClick="btnUpdate_Click" Visible="false" />
    <asp:Button ID="btnCancel" Text="Cancel" runat="server" OnClick="btnCancel_Click" Visible="false" />
   </asp:TableCell>
  </asp:TableRow>
 </asp:Table>

 <asp:Panel ID="popupTaskType" title="Set Task Type" style="display: none; overflow: hidden" Visible="false" runat="server">
  <table cellpadding="3" cellspacing="0" class="dialogTbl">
   <tr>
    <td rowspan="4" width="15">&nbsp;</td>
    <td width="85"><b>Favorite Title:</b></td>
    <td width="145"><div id="ttFavTitle"></div></td>
   </tr>
   <tr>
    <td colspan="2"><b>Task Type:</b></td>
   </tr>
   <tr>
    <td colspan="2">
     <cc:TreeSelector ID="treeTaskType" Height="250" Width="230" LoadDesc="false" ShowScript="true" runat="server" />
    </td>
   </tr>
   <tr>
    <td colspan="2" align="center" height="35">
     <asp:Button ID="btnUpdTaskType" Text="Update Task Type" OnClick="btnUpdTaskType_Click" runat="server" />
     <input type="button" value="Cancel" onclick="CloseTaskType()" />
    </td>
   </tr>
  </table>
 </asp:Panel>

</asp:Content>
