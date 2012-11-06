<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErrorPage.aspx.cs" Inherits="WALT.UIL.ErrorPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>WALT</title>
    <link id="Link1" runat="server" rel="shortcut icon" href="favicon.ico" type="image/x-icon"/>
    <link id="Link2" runat="server" rel="icon" href="favicon.ico" type="image/ico"/>
    <link rel="stylesheet" href="css/jquery.ui.all.css" type="text/css" media="screen" />
 <%--   <link href="/css/jquery-ui.css" rel="stylesheet" type="text/css" media="screen" />--%>
    <link href="/css/WALT.css" rel="stylesheet" type="text/css" media="screen" />
    <script src="/js/jquery.js" type="text/javascript"></script>
    <script src="/js/jquery-ui.js" type="text/javascript"></script>
    <script src="/js/jquery.ui.core.js" type="text/javascript"></script>
    <script src="/js/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="/js/jquery.ui.datepicker.js" type="text/javascript"></script>
    <script src="/js/jquery.ui.button.js" type="text/javascript"></script>
    <style type="text/css">
        .style1
        {
            width: 222px;
        }
    </style>
</head>
<body bgcolor="#cccccc">
    <form id="form1" runat="server">
    <table bgcolor="#ffffff" width="100%" border="0" cellpadding="5" cellspacing="0" style="border: 1px solid #999999">
        <tr>
            <td width="400px">
                <img src="/images/logo.jpg" alt="Organization Logo" />
            </td>
            <td align="left">
                <asp:Label ID="Label3" runat="server" Text="WALT" Font-Bold="True" 
                    Font-Size="24pt" Font-Names="Verdana" ForeColor="#284E98"></asp:Label>
            </td>
            <td width="200px" align="right" style="padding: 20px">
                &nbsp;</td>
        </tr>
        <tr><td bgcolor="#B5C7DE" colspan="3">&nbsp;</td></tr>
        <tr>
            <td colspan="3">
            </td>            
        </tr>
        <tr valign="top">
            <td colspan="3">
            <a href="/">Main</a>
            </td>
        </tr>
        <tr valign="top">
            <td colspan="3" style="padding: 20px">
            <h1>System Error</h1>
                <pre id="Pre1" runat="server" /></td>
        </tr>
        <tr><td>
            <asp:Label ID="Label4" runat="server" Text="" Font-Size="X-Small" ForeColor="#999999"></asp:Label></td><td colspan="2" align="right">
            <asp:Label ID="VersionLabel" runat="server" Text="" Font-Size="X-Small" ForeColor="#999999"></asp:Label></td></tr>
    </table>
    
    </form>
</body>
</html>
