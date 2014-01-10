using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;

namespace dbshub.Models
{
 
    public class Account : IValidatableObject
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public ObjectId InviteByAccountID { get; set; }
        [Required(ErrorMessage = "必须输入用户名"), DisplayName("用户名")]
        [PageDesigners.Library.ValidationAttributes.Email(ErrorMessage="必须是一个邮件帐号")]
        [PageDesigners.Library.ValidationAttributes.Exists(ErrorMessage = "用户账号已存在")]
        public string UserNO {get;set;}
        //小写的
        public string _userno { get; set; }

        [DisplayName("昵称")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "必须输入密码"), DisplayName("密码")]
        public string Password { get; set; }
        public string Password1 { get; set; }

        public bool Admin { get; set; }
        public bool Signin;
        public bool Guest { get; set; }
        public string From { get; set; } //ip
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }

        public string InviteCode { get; set; }

        public List<string> Powers { get; set; }

        public Account() {
            CreateTime = DateTime.Now;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != Password1) {
                yield return new ValidationResult("两次密码输入不一致", new[] { "Password1" });
            }
            else  {
                var db = l.core.MongoDBHelper.GetMongoDB();
                var i = db.GetCollection("InviteCode").FindOneAs<InviteCode>(Query.And(
                        Query.EQ("Code", InviteCode ?? ""),
                        Query.EQ("UseTime", DateTime.MinValue)
                    ));
                if (false)//(i == null)
                    yield return new ValidationResult("邀请码不对", new[] { "InviteCode" });
                else
                {
                    // i.UseTime = DateTime.Now;
                    // db.GetCollection("InviteCode").Save(i);
                    // InviteByAccountID = i.AccountID;
                }
            }
        }
    }

    public class InviteCode {
        public ObjectId _id { get; set; }
        public string Code { get; set; }
        public ObjectId AccountID { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UseTime { get; set; }

    }
}
