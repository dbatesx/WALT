<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportView.aspx.cs" Inherits="WALT.UIL.Reports.ReportView" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>WALT</title>
</head>
<body>
    <form id="form1" runat="server">
    <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%" Font-Names="Verdana"
        Font-Size="8pt" Height="100%" meta:resourcekey="ReportViewer1Resource1">
        <LocalReport>
            <DataSources>
                <rsweb:ReportDataSource />
            </DataSources>
        </LocalReport>
    </rsweb:ReportViewer>
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    </form>
</body>
</html>
