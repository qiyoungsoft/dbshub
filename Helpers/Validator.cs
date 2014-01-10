using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace PageDesigners.Library.ValidationAttributes
{
    public class EmailAttribute : RegularExpressionAttribute, IClientValidatable
    {
        public EmailAttribute()
            : base(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                   @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                   @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")
        {
        }
        
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        //public IEnumerable GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = FormatErrorMessage(metadata.GetDisplayName());

            yield return new EmailValidationRule(errorMessage);
        }
    }

    public class EmailValidationRule : ModelClientValidationRule
    {
        public EmailValidationRule(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ValidationType = "email";
        }
    }

    public class ExistsAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value,
        ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;
            var db = l.core.MongoDBHelper.GetMongoDB();
            var found = db.GetCollection(validationContext.ObjectType.Name).FindOne(Query.EQ(
                validationContext.DisplayName,
                    BsonString.Create(value)));
            if (found != null)
            {
                if (validationContext.ObjectInstance.ToBsonDocument()["_id"]
                    != found["_id"])
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                else return ValidationResult.Success;
            }
            else return ValidationResult.Success;
        }
    }
}