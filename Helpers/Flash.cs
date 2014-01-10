using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dbshub
{
    public class Flash  {
        static private HttpContext context;
        static public void Message(string message)  {
            if (HttpContext.Current != null)  {
                if (context == null) context = HttpContext.Current;
            }
            if (HttpContext.Current != null || context != null)
                if ((HttpContext.Current ?? context).Session != null)
                    (HttpContext.Current ?? context).Session["Flash"] = message;
        }

        static public string Get()  {
            var r = HttpContext.Current.Session["Flash"];
            HttpContext.Current.Session.Remove("Flash");
            return r == null ? null : r.ToString();
        }
    }
}