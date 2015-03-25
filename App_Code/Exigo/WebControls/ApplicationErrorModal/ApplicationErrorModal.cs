using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Text;

namespace Exigo.WebControls
{
    /// <summary>
    /// Renders a jQuery modal alert script and a hidden vaiable called 'ApplicationErrors' designed to display alerts throughout your applications.
    /// </summary>
    public class ApplicationErrorModal : WebControl
    {
        #region Constructors
        public ApplicationErrorModal()
        {

        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The error message you want to display in the modal. This value is server-encoded and optimized for Javascript automatically when set.
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = HttpContext.Current.Server.UrlEncode(value.Replace("'", "\"").Replace("%0a", "").Replace("%0A", "")); }
        }
        private string _errorMessage;

        /// <summary>
        /// The text of the button used to close the modal.
        /// </summary>
        public string ButtonText { get; set; }
        #endregion

        #region Render
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            // Create a hidden variable so our features can communicate with the jQuery.
            HiddenField applicationErrorsField = new HiddenField();
            applicationErrorsField.ID = "ApplicationErrors";
            applicationErrorsField.Value = HttpContext.Current.Server.UrlDecode(ErrorMessage);
            applicationErrorsField.RenderControl(writer);


            // Build the jQuery
            StringBuilder javascripts = new StringBuilder();


            // jQuery UI scripts to stylize common web elements
            javascripts.AppendLine(string.Format(@"
                <!-- ***** START Error Handling ***** -->
                <link href='Assets/Plugins/jquery.msgbox/styles/jquery.msgbox.css' rel='stylesheet' type='text/css' />
                <script src='Assets/Plugins/jquery.msgbox/jquery.msgbox.min.js' type='text/javascript'></script>

                <script type='text/javascript' language='javascript'>
                    $(document).ready(function () {{
                        if ($('INPUT[type=""hidden""][id*=""ApplicationErrors""]').val() != '') {{
                            $(function () {{
                                var error = $('INPUT[type=""hidden""][id*=""ApplicationErrors""]').val();
                                $.msgbox(error, {{ 
                                    type: 'error',
                                    buttons: [
                                        {{ type: 'submit', value: '" + ((!string.IsNullOrEmpty(ButtonText)) ? ButtonText : "OK") + @"'}}
                                    ]
                                }});
                            }});
                        }}
                    }});
                </script>
                <!-- ***** END Error Handling ***** -->
            "));


            // Render the jQuery
            writer.Write(javascripts.ToString());
        }
        #endregion
    }
}