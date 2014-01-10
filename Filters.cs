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
using dbshub.Models;

namespace dbshub
{
    public class LoginFilter : ActionFilterAttribute
    {
        public bool Admin { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            #region 模拟登录
            filterContext.HttpContext.Session["UserNO"] = "lobby@21cn.com";
            #endregion

            var r = filterContext.HttpContext.Session["UserNO"] == null;
            var userAdmin = false;
            if (!r && Admin)
            {
                var acc = SiteAccount.Get(filterContext.HttpContext, l.core.MongoDBHelper.GetMongoDB());
                userAdmin = acc.Admin;
            }
            if (r || (Admin && !userAdmin)) {
                filterContext.HttpContext.Response.Redirect(
                    "/Account/ToLogin?" + (!r && Admin && !userAdmin? "code=power&":"") +  "rel=" +
                    (filterContext.HttpContext.Request.RequestType == "POST" ?
                    filterContext.HttpContext.Request.UrlReferrer :
                    filterContext.HttpContext.Request.Url));
            }
        }

        #region IExceptionFilter 成员

        public void OnException(ExceptionContext filterContext)
        {
            //
        }

        #endregion
    }

     
}