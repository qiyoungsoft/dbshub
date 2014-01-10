using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;

namespace dbshub.Models
{
    public class TableSchema
    {
        public ObjectId _id { get; set; }
        public ObjectId Account { get; set; }
        public FieldSchema[] Fields { get; set; }
    }

    public class FieldSchema {
        public string Name { get; set; }
        public string Caption { get; set; }

    }
}