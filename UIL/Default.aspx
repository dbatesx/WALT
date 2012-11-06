<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WALT.UIL._Default" MasterPageFile="Site1.Master" %>
<%@ Register TagPrefix="ts" TagName="TeamSummary" Src="~/Controls/TeamSummary.ascx" %>
<%@ Register TagPrefix="ap" TagName="AlertPanel" Src="~/Controls/AlertPanel.ascx" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <link href="/css/WALT.css" rel="stylesheet" type="text/css" media="screen" />
    <ts:TeamSummary id="TeamSummary" runat="server" /><br />
    <ap:AlertPanel id="AlertPanel" runat="server" />
</asp:Content>
