<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="WALT.UIL.login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="/js/jquery.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            var now = new Date();
            document.getElementById('localTime').value =
                (now.getMonth() + 1).toString() + '/' + now.getDate() + '/' + now.getFullYear() + ' ' + now.getHours() + ':' + now.getMinutes() + ':00';
            document.form1.submit();
        });
    </script>
</head>
<body bgcolor="#CCCCCC">
    <form id="form1" runat="server">
    <div>
        <input type="hidden" id="localTime" name="localTime" />
    </div>
    </form>
</body>
</html>
