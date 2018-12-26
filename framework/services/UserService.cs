using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Medtrix.WebService;
using Medtrix.DataAccessControl;
using Medtrix.Trace;
using System.Web.Script.Serialization;

namespace Conference
{
  
    public class Email
    {
        public String type = String.Empty; 
        public String name = String.Empty;
        public String message = String.Empty;
        public String email = String.Empty;
        public String position = String.Empty;
    }


    class UserService : Service
    {

        JavaScriptSerializer _json = new JavaScriptSerializer();

    }
}