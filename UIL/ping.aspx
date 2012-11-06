<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ping.aspx.cs" Inherits="WALT.UIL.ping" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:Timer ID="Timer1" ontick="Timer1_Tick" runat="server" />
    </div>
    </form>
</body>
</html>
