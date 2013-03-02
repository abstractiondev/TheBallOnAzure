<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TheBallLogin.aspx.cs" Inherits="WebInterface.TheBallLogin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="Label1" runat="server" Text="OpenID Login" /><asp:TextBox ID="openIdBox"
            runat="server" /><asp:Button ID="loginButton" runat="server" Text="Login" OnClick="loginButton_Click" /><asp:CustomValidator
                runat="server" ID="openidValidator" ErrorMessage="Invalid OpenID Identifier"
                ControlToValidate="openIdBox" EnableViewState="false" OnServerValidate="openidValidator_ServerValidate" /><br />
                <asp:Button ID="bGoogleLogin" runat="server" Text="Google Login" OnClick="bGoogleLogin_Click" />
        <asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
            Visible="False" /><asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False"
                Text="Login canceled" Visible="False" />
    </div>
    </form>
</body>
</html>
