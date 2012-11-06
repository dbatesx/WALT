<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TreeSelector.ascx.cs" Inherits="WALT.UIL.Controls.TreeSelector" %>

<asp:PlaceHolder ID="phTreeScript" runat="server">
<script type="text/javascript">
    
    function SelectNode(tree, id, loadDesc, expand, comp)
    {
        var prefix = GetTreePrefix(tree);
        var selId = document.getElementById(prefix + "selNode");

        if (selId.value != -1) {
            node = document.getElementById(tree + "_node_" + selId.value);
            node.style.color = "#000000";
            node.parentNode.parentNode.style.backgroundColor = "#ffffff";
        }

        if (id != -1) {
            node = document.getElementById(tree + "_node_" + id);

            if (node) {
                node.style.color = "#ffffff";
                node.parentNode.parentNode.style.backgroundColor = "#0A246A";
                node.parentNode.style.width = "100%";

                document.getElementById(prefix + "selValue").value = $('#' + node.id).html();

                if (loadDesc) {
                    $('#' + prefix + 'nodeText').html(eval(tree + "_nodeDesc_" + id));
                }

                if (expand) {
                    TreeviewExpandNode(tree, node);
                    node.parentNode.scrollIntoView(true)
                }
            }
            else {
                id = -1;
                document.getElementById(prefix + "selValue").value = '';
            }
        }
        else {
            document.getElementById(prefix + "selValue").value = '';
        }

        selId.value = id;
        var compTree = document.getElementById('complexityTree');

        if (compTree && compTree.value == tree) {

            if (id != -1) {
                $('#complexTbl').show();
            }
            
            var ids = eval('compID_' + id);
            var titles = eval('compTitle_' + id);
            var re = eval('compRE_' + id);
            var len = titles.length;

            var dd = document.getElementById('ddComplexity');
            dd.options.length = 0;
            dd.options[0] = new Option('', '', true, false);

            $('#compRE').html('');

            for (var i = 0; i < len; i++) {
                var selected = (comp && comp == ids[i]);
                dd.options[i + 1] = new Option(titles[i], ids[i], false, selected);

                if (selected) {
                    $('#compRE').html(re[i]);
                }
            }            
        }
    }
    
    function TreeViewExpandAll(tree)
    {
        TreeviewExpandCollapseAll(tree, true);
    }
    
    function TreeViewCollapseAll(tree)
    {
        TreeviewExpandCollapseAll(tree, false);
    }
    
    function TreeviewExpandCollapseAll(tree, expandAll)
    {
        var displayState = (expandAll == true ? "none" : "block");
        var prefix = GetTreePrefix(tree);
        var treeLinks = $("'a[id^=\"" + prefix + "\"]'");
        var nodeCount = treeLinks.length;
        
        for (i = 0; i < nodeCount; i++)
        {
            if (treeLinks[i].firstChild && treeLinks[i].firstChild.tagName &&
                treeLinks[i].firstChild.tagName.toLowerCase() == "img")
            {
                var node = treeLinks[i];
                var level = parseInt(node.id.substr(node.id.length-1), 10);
                var childContainer = GetParentByTagName("table", node).nextSibling;
                
                if (childContainer && childContainer.style.display == displayState)
                {
                    TreeView_ToggleNode(eval(prefix + "tree_Data"), level, node, ' ', childContainer);
                }
            }
        }
    }
    
    function TreeviewExpandNode(tree, node)
    {
        var prefix = GetTreePrefix(tree);

        while (node.tagName && node.tagName != "FORM")
        {
            if (node.tagName == "DIV" && node.id.indexOf("_treen") != -1)
            {
                TreeviewExpandID(node.id, prefix);
            }
            
            node = node.parentNode;
        }
    }
    
    function TreeviewExpandID(id, prefix)
    {
        var start = id.indexOf("_treen") + 6;
        var branchId = id.substr(start, id.indexOf("Nodes")-start);
        var parent = document.getElementById(prefix + "treen" + branchId);
        var childContainer = GetParentByTagName("table", parent).nextSibling;
        
        if (childContainer && childContainer.style.display == "none")
        {
            TreeView_ToggleNode(eval(prefix + "tree_Data"), branchId, parent, ' ', childContainer);
        }
    }
    
    function GetTreePrefix(tree)
    {
        var prefix = $("'span[id$=\"" + tree + "_treeScript\"]'").attr('id');
        return prefix.substr(0, prefix.length-10);
    }
    
    function GetParentByTagName(parentTagName, childElementObj)
    {
        var parent = childElementObj.parentNode;
        
        while (parent.tagName.toLowerCase() != parentTagName.toLowerCase())
        {
            parent = parent.parentNode;
        }
        
        return parent;
    }
    
    function IsTreeSelected(tree)
    {
        return (document.getElementById(GetTreePrefix(tree) + "selNode").value != "-1");
    }

    function GetTreeSelectedID(tree) {
        return document.getElementById(GetTreePrefix(tree) + "selNode").value;
    }

    function GetTreeSelectedValue(tree) {
        return document.getElementById(GetTreePrefix(tree) + "selValue").value;
    }

    function ShowRE(idx) {
        if (idx > 0) {
            var compTree = document.getElementById('complexityTree');

            if (compTree)
            {
                var re = eval('compRE_' + GetTreeSelectedID(compTree.value));
                $('#compRE').html(re[idx-1]);
            }
        }
        else {
            $('#compRE').html('');
        }
    }

    function TreeValidateComplexity() {
        var compTree = document.getElementById('complexityTree');

        if (compTree && compTree.value != '' && document.getElementById('ddComplexity').selectedIndex == 0)
        {
            alert("Please select a complexity");
            document.getElementById('ddComplexity').focus();
            return false;
        }

        return true;
    }
</script>
</asp:PlaceHolder>

<asp:Label ID="treeScript" runat="server"></asp:Label>

<asp:HiddenField ID="selNode" Value="-1" runat="server" />
<asp:HiddenField ID="selValue" runat="server" />

<table cellpadding="0" cellspacing="0" class="treeTbl">
 <tr>
  <td class="tree" id="cellTree" runat="server">
   <asp:Panel ID="treePanel" runat="server" ScrollBars="Vertical">
    <asp:TreeView ID="tree" runat="server" OnSelectedNodeChanged="tree_SelectedNodeChanged" NodeWrap="true"></asp:TreeView>
   </asp:Panel>
  </td>
  <td style="padding: 0px 0px 0px 10px" valign="top" id="cellDesc" runat="server">
   <asp:PlaceHolder ID="phComplexity" Visible="false" runat="server">
    <table id="complexTbl" style="display: none">
     <tr>
      <td style="padding: 0px 3px 3px 3px"><b>Complexity:</b></td>
      <td style="padding: 0px 3px 3px 3px"><select name="ddComplexity" id="ddComplexity" onchange="ShowRE(this.selectedIndex)"></select></td>
    </tr>
    <tr>
      <td align="right" style="padding: 0px 3px 0px 3px"><b>R/E:</b></td>
      <td style="padding: 0px 3px 0px 3px"><div id="compRE"></div></td>
     </tr>
    </table>
   </asp:PlaceHolder>
   <div id="nodeText" runat="server"></div>&nbsp;
  </td>
 </tr>
</table>

