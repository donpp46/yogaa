using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Net;

namespace Medtrix.PDF
{
    public class PDF
    {
       
        public static void ReplaceHtmlWithData(XmlDocument doc, String id, String value)
        {
            XmlNodeList nl = doc.SelectNodes("//*[@id='" + id + "']");
            foreach (XmlNode node in nl)
            {
                node.InnerText = System.Web.HttpUtility.HtmlDecode(value);
            }
        }



        public static object DataTableToJSON(DataTable dt)
        {
            if (dt == null)
                return null;

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return rows;
        }
    }
    
}
