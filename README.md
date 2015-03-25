# jmoa-assesment
Simple ASP.NET skills  assessment created for JMOA

1. Fix the Identity Implementation located in Ln 10 (App_Code\Exigo\Identity\Identity.cs)
Hint: Every IPrincipal implementation expects a get method returning System.Security.Principal.IIdentity.

2. Fix the SQL authentication method CreateFormsAuthenticationTicket(int customerID) located in Ln 90 (App_Code\Exigo\Identity\IdentityAuthenticationService.cs)
Hint: Create a SQL query to pull data from Customers table, this data will be used to format the ticket, expected type is DataRow.

3. Render a message in the Home.aspx after successful login
Hint: Use Page.Render(HtmlTextWriter writer) override method to write a message in the page. Do not forget to end the current response processing.