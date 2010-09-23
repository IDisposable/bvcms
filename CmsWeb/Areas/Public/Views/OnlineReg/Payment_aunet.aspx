<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/onlinereg.Master" Inherits="System.Web.Mvc.ViewPage<CmsWeb.Models.PaymentModel>" %>
<asp:Content ID="registerContent" ContentPlaceHolderID="MainContent" runat="server">
<style type="text/css">
div.terms {
   width:600px;
   height:200px;
   border:1px solid #ccc;
   background:#f2f2f2;
   padding:6px;
   overflow:auto;
}
div.terms p,
div.terms li {font:normal 11px/15px arial;color:#333;}
div.terms h3 {font:bold 14px/19px arial;color:#000;}
div.terms h4 {font:bold 12px/17px arial;color:#000;}
div.terms strong {color:#000;}	
a.submitbutton,a.button {
  padding:5px;
    border-color:#D9DFEA #0E1F5B #0E1F5B #D9DFEA;
    background-color:#3B5998;
  border: 1px solid;
    color:#FFFFFF;
  text-decoration:none;
}
</style>
    <script src="/Content/js/jquery-1.4.2.min.js" type="text/javascript"></script>
    <script src="/Content/js/jquery.idle-timer.js" type="text/javascript"></script>
    <script src="/Content/js/jquery.showpassword.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function() {
            $(document).bind("idle.idleTimer", function() {
                window.location.href = '<%=Model._URL %>';
            });
            var tmout = parseInt('<%=Model._timeout %>');

            $("a.submitbutton").click(function(ev) {
                ev.preventDefault();
                var f = $(this).closest('form');
                var q = f.serialize();
                $.post(this.href, q, function(ret) {
                    if (ret.error) {
                        $('#validatecoupon').text(ret.error);
                    }
                    else {
                        window.location = ret.confirm;
                    }
                }, "json");
                return false;
            });
            $.idleTimer(tmout);
            $('form').showPassword();
        });
    </script>
    <script type="text/javascript">
        $(function() {
            if ($('#IAgree').attr("id")) {
                $("#Submit").attr("disabled", "disabled");
                $("a.submitbutton").attr("disabled", "disabled");
            }
            $("#IAgree").click(function() {
                var checked_status = this.checked;
                if (checked_status == true) {
                    $("#Submit").removeAttr("disabled");
                    $("a.submitbutton").removeAttr("disabled");
                }
                else {
                    $("#Submit").attr("disabled", "disabled");
                    $("a.submitbutton").attr("disabled", "disabled");
                }
            });
        });
    </script>

    <h2>Payment Processing</h2>
<% if(Model.Terms.HasValue())
   { %>
<%=Model.Terms %>
<p><%=Html.CheckBox("IAgree") %> I agree to the above terms and conditions.</p>
<% } %>

    <form action="https://test.authorize.net/gateway/transact.dll" method="post">
    <%=Html.Hidden("x_login") %>
    <%=Html.Hidden("x_amount") %>
    <%=Html.Hidden("x_description") %>
    <%=Html.Hidden("x_fp_sequence") %>
    <%=Html.Hidden("x_fp_timestamp") %> 
    <%=Html.Hidden("x_fp_hash") %>
    <%=Html.Hidden("x_test_request") %>
    <%=Html.Hidden("x_first_name")%>
    <%=Html.Hidden("x_last_name")%>
    <%=Html.Hidden("x_address") %>
    <%=Html.Hidden("x_city") %>
    <%=Html.Hidden("x_state")%>
    <%=Html.Hidden("x_zip") %>
    <%=Html.Hidden("x_cust_id") %>
    <%=Html.Hidden("x_delim_data", ",") %>
    <%=Html.Hidden("x_encap_data", "\\") %>
    <%=Html.Hidden("x_show_form", "PAYMENT_FORM") %>

    <%=Html.Hidden("_datumid") %>
    <%=Html.Hidden("_timeout") %>
    <%=Html.Hidden("_URL") %>
    <%=Html.Hidden("_confirm") %>

<% if (Model.Terms.HasValue())
   { %>
    <p>
        You must agree to the terms above for you or your minor child before you can continue.</p>
<% } %>
    <p>
        When you click the 'Pay with Credit Card' button you will be redirected to ServiceU.com to process your payment of <%=Model.Amount.ToString("C") %>.
        After you are finished there, you will be redirected back here to get your confirmation.
        Your information will not be committed until you complete the transaction on the next page.
    </p>
    <p><%=Html.SubmitButton("Submit", "Pay with Credit Card") %></p>
    <p>If you have received a Cash Payment Code, please enter that number here and click the blue link next to it:</p>
    <input id="_Coupon" type="password" name="_Coupon" value="<%=Model._Coupon %>" />
    <a href="/OnlineReg/PayWithCoupon/" class="submitbutton">Pay with coupon</a>
    <span style="color:Red" id="validatecoupon"></span>
    </form>
</asp:Content>