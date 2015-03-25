using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Exigo.WebControls
{
    public class ErrorManager : WebControl
    {
	    public ErrorManager()
	    {
            this.ClientReferenceID = string.Empty;
            this.Header = string.Empty;
		    this.Message = string.Empty;

            this.ClientIDMode = System.Web.UI.ClientIDMode.Static;
	    }

        #region Properties
        /// <summary>
        /// The name of the Javascript variable created that stores the error details.
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public string ClientReferenceID { get; set; }

        /// <summary>
        /// An optional header of the error message. Used only if desired.
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Header { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Message { get; set; }

        /// <summary>
        /// The type of error.
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public ErrorMessageType Type { get; set; }
        #endregion

        #region Render
        protected override void OnPreRender(EventArgs e)
        {
            if(string.IsNullOrEmpty(this.ClientReferenceID)) this.ClientReferenceID = this.ID.ToLower();

            base.OnPreRender(e);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            // If we have no message to show, stop here and don't render anything
            if(string.IsNullOrEmpty(this.Message)) return;


            StringBuilder html = new StringBuilder();
            html.Append("<!-- Error rendered by the ErrorManager -->");
            html.AppendFormat(@"
                    <script>
                        var {0} = {{
                            header: ""{1}"",
                            message: ""{2}"",
                            success: {3}
                        }};
                    </script>
                ", this.ClientReferenceID,
                 GlobalUtilities.FormatErrorMessageForJavascript(this.Header),
                 GlobalUtilities.FormatErrorMessageForJavascript(this.Message),
                 (this.Type == ErrorMessageType.Success) ? "true" : "false");

            writer.Write(html.ToString());

            // Handle custom error handling
            RenderCustomJavascriptHandling(writer);
        }

        protected void RenderCustomJavascriptHandling(HtmlTextWriter writer)
        {
            StringBuilder html = new StringBuilder();


            html.AppendFormat(@"
                <div class='alertwrapper'></div>
                <script>
                    var errorArray = {0}.success + '|' + {0}.header + '|' + {0}.message;
                    $(function() {{
                        exigo.throwErrors('.alertwrapper', errorArray)
                    }});
                </script>
            ", this.ClientReferenceID);


            writer.Write(html.ToString());
        }
        #endregion
    }

    public enum ErrorMessageType
    {
        Success,
        Failure
    }
}