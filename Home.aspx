<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Home.aspx.cs" Inherits="Home" MasterPageFile="~/MasterPages/Site.master" %>

<asp:Content ID="Head1" runat="server" ContentPlaceHolderID="Head">
    <script src="Assets/Plugins/jquery.masonry/jquery.masonry.min.js"></script>
    <script>
        // Set page variables
        var page = {
            activenavigation: 'home'
        };

        function bounceIcons(element) {
            $(element).find('.animated').effect("bounce", { times: 3, distance: 7 }, 1000);
        } 

        function RenderLoginSuccess() {

            $.ajax({
                url: '<%=Request.Url.AbsolutePath%>?datakey=login',
                type: 'GET',
                success: function(data) {
                    $(data).appendTo(".message");
                }
            });
        }

        $(function () {
            RenderLoginSuccess(); 
        });

    </script>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Content">
    <h1><%=Identity.Current.FirstName + " " + Identity.Current.LastName %></h1>

    <div class="row-fluid" style="min-width: 740px;">
        <div class="message"> 
        </div>
    </div>
</asp:Content>
