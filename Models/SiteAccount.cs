using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace dbshub.Models
{
    /// <summary>
    /// 会员登陆过虑
    /// </summary>

    public class SiteAccount : dbshub.Models.Account
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static public dbshub.Models.Account Get(HttpContextBase context, MongoDB.Driver.MongoDatabase db)
        {
            var signin = context.Session["UserNO"] != null;
            dbshub.Models.Account account = null;
            if (signin ){
                account = db.GetCollection<dbshub.Models.Account>("Account").FindOne(Query.EQ("_userno", context.Session["UserNO"].ToString().ToLower()));
                context.Session["UserNO"] = account.UserNO;
                account.Signin = true;
            }
            else {
                account = new dbshub.Models.Account();
                HttpCookie c = context.Request.Cookies["pf2"];
                if (c == null)
                {
                    var cookid = new Random().NextDouble().ToString();
                    c = new HttpCookie("pf2", cookid);
                    c.Expires = DateTime.Now.AddDays(30);
                    context.Response.AppendCookie(c);
                }
                if (c["CookieID"] == null)
                    account.UserNO = c.Value;
                else 
                    account.UserNO = c["CookieID"];
                
            }
            //account.UserName = account.UserNO + (account.Signin ? "先生" : "游客");
            return account;
        }
    }
}