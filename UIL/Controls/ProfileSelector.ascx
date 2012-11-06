<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileSelector.ascx.cs"
    Inherits="WALT.UIL.Controls.ProfileSelector" %>
<style type="text/css">
    .searchBtn{margin-left:2px; margin-bottom:-5px; }
    .resultBox{ border:1px solid #C0C0C0;}
    .searchBox{ margin-bottom: 5px;}
    
</style>
<script type="text/javascript">
    function SetCursorToTextEnd(textControl) {
        if (textControl != null) {
            textControl.focused = true;
            if (textControl.value.length > 0 && textControl.createTextRange) {
                var FieldRange = textControl.createTextRange();
                FieldRange.moveStart('character', textControl.value.length);
                FieldRange.collapse();
                FieldRange.select();
            }
        }
    }

    function HasFocus(inputId) {
        var txt = document.getElementById(inputId.replace('btnFilterItems', 'txtFilterItems'));
        if (txt.focused && txt.focused == true) {
            txt.focused = false;
            return true;
        }

        return false;
    }

    function SearchKeyDown(e, txt) {
        if (e.keyCode == 13) {
            var btn = document.getElementById(txt.id.replace('txtFilterItems', 'btnFilterItems'));
            btn.click();
            btn.focus();
            return false;
        }
    }

    function ClientValidate(sender, args) {
        var selectedP = $("[id*='profileSelected'] :selected").val();

        if (selectedP != null) {           
            args.IsValid = true;
        }
        else {          
            args.IsValid = false;
        }
    }

    function searchResultListBox_DoubleClick() {       
        $('*[id*=searchListBoxHidden]').val("doubleclicked");      
        __doPostBack("<%= button.ClientID %>", "");       
    }


</script>

<asp:UpdatePanel runat="server" ID="profilePanel" UpdateMode="Conditional">
 <ContentTemplate>  
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td>
                <span style="font-size: 7.5pt"><b>Search:</b></span>&nbsp;<asp:TextBox ID="txtFilterItems" runat="server" Width="122px"
                    onfocus="SetCursorToTextEnd(this)" onkeydown="return SearchKeyDown(event, this)" CssClass="searchBox">
                    </asp:TextBox><asp:ImageButton ID="btnFilterItems" runat="server" ImageUrl="/css/images/search-icon.png"
                    OnClientClick="if(!HasFocus(this.id))return false" OnClick="txtFilterItems_TextChanged" CausesValidation="false" CssClass="searchBtn" />
            </td>
            <td></td>
            <td valign="bottom"><b>Selected:</b></td>
        </tr>
        <tr>
            <td>
                <asp:ListBox ID="searchResultListBox" runat="server" SelectionMode="Single" Width="200px" 
                    ondblclick="searchResultListBox_DoubleClick()" Height="100px" CssClass="resultBox" CausesValidation="false"></asp:ListBox>
                <asp:HiddenField ID="searchListBoxHidden" runat="server" />                           
                <asp:Button ID="button" runat="server" Style="display: none;" UseSubmitBehavior="false" />
            </td>
            <td valign="top" style="padding-left: 5px; padding-right: 5px">
                    <asp:Button ID="addButton" runat="server" Text="Add" OnClick="addButton_Click" CausesValidation="false"
                        ForeColor="Green" Width="67px" UseSubmitBehavior="false" />
                    <br /><br />
                    <asp:Button ID="removeButton" runat="server" Text="Remove" OnClick="removeButton_Click"
                        CausesValidation="false" ForeColor="Red" Width="67px" Enabled="false" UseSubmitBehavior="false" />
            </td>
            <td>
                <asp:ListBox ID="profileSelected" SelectionMode="Multiple" runat="server" Width="200px"
                    Height="100px" CssClass="resultBox" OnSelectedIndexChanged="profileSelected_SelectedIndexChanged">
                </asp:ListBox>
            </td>            
        </tr>
        <tr>
         <td>
          <asp:Label ID="lblNoMatch" runat="server" Visible="false" Style="color: Red;">
           No results were found to match<br />your search item.
          </asp:Label>
         </td>
         <td></td>
         <td>
          <asp:RequiredFieldValidator ID="profileRequiredFieldValidator" Text="This profile is a required field."
            ControlToValidate="profileSelected" runat="server" ForeColor="Red"
            Display="Dynamic" Font-Size="11px" />
         </td>
        </tr>
    </table>
   </ContentTemplate>
   <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnFilterItems" EventName="Click" />
        <asp:AsyncPostBackTrigger ControlID="button" EventName="Click" />
        <asp:AsyncPostBackTrigger ControlID="addButton" EventName="Click" />
        <asp:AsyncPostBackTrigger ControlID="removeButton" EventName="Click" />
   </Triggers>
</asp:UpdatePanel>
