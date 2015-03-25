<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" MasterPageFile="~/MasterPages/Public.master" %>



<asp:Content ID="Head1" runat="server" ContentPlaceHolderID="Head">
    <style>
        h1 { float:left; }
        #lstLanguage { float: right; margin-top:20px; }
    </style>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Content">  
    <div class="row-fluid">
        <span class="span4 offset4">

            <h1><%=Resources.Login.SignIn %></h1>

            <asp:DropDownList runat="server" ID="lstLanguage" OnSelectedIndexChanged="SetLanguage" AutoPostBack="true" ClientIDMode="Static"></asp:DropDownList>
                <div style="clear:both;"></div>  

            <div class="loginwrapper well">

                <%=ErrorString %>

                <div class="row-fluid">
                    <span class="span12">
                        <label for="txtLoginName"><%=Resources.Login.Username %></label>
                        <asp:TextBox ID="txtLoginName" runat="server" CssClass="span12" />
                        <asp:RequiredFieldValidator ID="rtxtLoginName" ControlToValidate="txtLoginName" Display="Dynamic" SetFocusOnError="true" runat="server" />
                    </span>
                </div>

                <div class="row-fluid">
                    <span class="span12">
                        <label for="txtPassword"><%=Resources.Login.Password %></label>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="span12" />
                        <asp:RequiredFieldValidator ID="rtxtPassword" ControlToValidate="txtPassword" Display="Dynamic" SetFocusOnError="true" runat="server" />
                    </span>
                </div>

                <div class="row-fluid">
                    <span class="span12 checkbox">
                        <asp:CheckBox ID="chkRememberMe" runat="server" />
                    </span>
                </div>

                <div class="row-fluid">
                    <span class="span12">
                        <asp:LinkButton ID="cmdSignIn" runat="server" CssClass="btn btn-primary btn-large btn-block" OnClick="SignIn_Click"><i class="icon-lock icon-white"></i> <%=Resources.Login.SignIn %></asp:LinkButton>
                    </span>
                </div>
            </div>

        </span>
    </div>
</asp:Content>
