using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Medtrix.MailService;
using System.Net.Mail;

public partial class service_sendEmail : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var name = Request.Form["name"];
        var type = Request.Form["type"];
        var email = Request.Form["email"];
        var message = Request.Form["message"];

        var age = Request.Form["age"];
        var gender = Request.Form["gender"];
        var country = Request.Form["country"];
        var mobile = Request.Form["mobile"];
        var address = Request.Form["address"];
        var hasissue = Request.Form["issue"];
        var reason = Request.Form["reason"];
        var doj = Request.Form["doj"];
        var dod = Request.Form["dod"];
        var employee = Request.Form["employee"];
        var info = Request.Form["info"];
        var purpose = Request.Form["purpose"];

        if (email != null)
        {

            String recep = "pradeepece.s@gmail.com";
            String myNotes = String.Empty;

            String body = "";
            if(type == "contact")
            {
                body =  "<div> Name : " + name + " </div>" +
                "<div> Email : " + email + " </div>" +
                "<div> Message : " + message + " </div>";
            } else if (type == "register") {
                body = "<div> Name : " + name + " </div>" +
               "<div> Email : " + email + " </div>" +
               "<div> Age : " + age + " </div>" +
               "<div> Email : " + email + " </div>" +
               "<div> Gender : " + gender + " </div>" +
               "<div> Country : " + country + " </div>" +
               "<div> Mobile : " + mobile + " </div>" +
               "<div> Address : " + address + " </div>" +
               "<div> Health Issue : " + (Convert.ToBoolean(hasissue) ? "Yes" : "No" ) + " </div>" +
               "<div> Reason : " + reason + " </div>" +
               "<div> Date of Joining : " + doj + " </div>" +
               "<div> Date of Disperse : " + dod + " </div>" +
               "<div> Previous Employee : " + (Convert.ToBoolean(employee) ? "Yes" : "No") + " </div>" +
               "<div> How Did You Get The Information of The Studio : " + info + " </div>" +
               "<div> What is Your Purpose of Joining Yoga Classes : " + purpose + " </div>";
            }
            

            Response.Write(Medtrix.MailService.EmailService.SendMail(recep, type, body, null));

        }

    }
}