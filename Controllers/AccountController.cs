using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Net.Mail;
using System.Text;
using System.Net;
using dbshub.Models;
using dbshub;

namespace dbshub.Controllers
{
    public class LoginUser :IValidatableObject {
        [Required(ErrorMessage="请输入用户名")]
        public string UserNO { get; set; }
        
        public string Password { get; set; }
        public bool admin;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)  {
            string p = l.core.Crypt.Encode(UserNO.ToLower() + Password);
            var db = l.core.MongoDBHelper.GetMongoDB();
            var accounts = db.GetCollection<Account>("Account");
            Account acc = accounts.FindOneAs<Account>(Query.EQ("_userno", UserNO.ToLower()));
            
            if ((acc != null) && (acc.Password == p || (acc.Password??"") ==  (Password??""))) {
                admin = acc.Admin;// !string.IsNullOrEmpty(acc.Admin);
            }
            else {
                yield return new ValidationResult("密码不对或用户不存在", new[] {   "Password" });
            }
              
        }
    }


    public class ChgPassUser : IValidatableObject
    {
        public string UserNO { get; set; }
        public string OldPass { get; set; }
        [ DisplayName("新密码"), Required]
        public string NewPass { get; set; }

        public string NewPass1 { get; set; }
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (NewPass1 != NewPass)
                yield return new ValidationResult("新旧密码不一致", new[] { "NewPass" });
            else {
                var db = l.core.MongoDBHelper.GetMongoDB();
                var account = db.GetCollection<Account>("Account");
                var acc = account.FindOne(Query.EQ("UserNO", UserNO));
                Password = l.core.Crypt.Encode(UserNO.ToLower() + OldPass);
                if ((acc != null) && (acc.Password == Password || (acc.Password ?? "") == (OldPass ?? ""))) {
                    Password = l.core.Crypt.Encode(UserNO.ToLower() + NewPass);
                }
                else yield return new ValidationResult("旧密码不对", new[] { "OldPass" });
            }

        }
    }
    public class AccountController : Controller  {
        static Dictionary<string, string> ErrorMsg = new Dictionary<string, string> { 
                {"login", "登录过期或者未登录"},
                {"power", "您没有权限访问这个页面"}
            };

        public ActionResult Signin(string id) {
            return View(new LoginUser() { UserNO ="admin"  });
        }

        public ActionResult Register()
        {
 
            return View(new Account());
        }

        [HttpPost]
        public ActionResult Register(Account account)
        {
            if (ModelState.IsValid) {
                var db = l.core.MongoDBHelper.GetMongoDB();
                var accounts = db.GetCollection("Account");
                account._userno = account.UserNO.ToLower();
                account._id = new ObjectId();
                account.Password = l.core.Crypt.Encode(account._userno + account.Password);
                account.From = Request.UserHostAddress;
                accounts.Save(account);

                Flash.Message("感谢您的注册，我们已经给您的账户邮箱发送了一封电子邮件，请牢记您的密码并请登录");
                return RedirectToAction("Signin");
            }
            return View(account);
        }

        [HttpPost]
        public ActionResult Signin(LoginUser account)  {
            var db = l.core.MongoDBHelper.GetMongoDB();
            if (ModelState.IsValid) {
                Session["UserNO"] = account.UserNO;
                if (account.admin) Session["Admin"] = true;
                return Redirect(Request.QueryString["rel"]??"/");
            }
            Session.Remove("UserNO");
            Session.Remove("Admin");
            return View(account);
        }

        public ActionResult Signout(object model)
        {
            Session.Remove("UserNO");
            Session.Remove("Admin");
            return Redirect("/Account/Signin?rel=" + Request.UrlReferrer.ToString());
        }

        public JsonResult CheckUserAccountExists(string UserNO) {
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [LoginFilter]
        public ActionResult ChgPass() {
            return View(new ChgPassUser { UserNO = Session["UserNO"].ToString() });
        }

        [HttpPost]
        public ActionResult ChgPass(ChgPassUser user) {
            if (ModelState.IsValid) {
                var db = l.core.MongoDBHelper.GetMongoDB();
                var account = db.GetCollection<Account>("Account");
                account.Update(Query.EQ("UserNO", user.UserNO),
                    Update<Account>.Set(p => p.Password, user.Password));
                return Redirect("/");
            }
            else return View(user);
        }

 
  

        public ActionResult ToLogin() {
            var code = Request.QueryString["code"] ?? "login";
            ViewBag.errmsg = ErrorMsg[code];
            ViewBag.time = code == "login" ? 0 : 10;
            ViewBag.url = "/account/signin?rel=" + Request.QueryString["rel"];
            return View();
        }
    }
}
