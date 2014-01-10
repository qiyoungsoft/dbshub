using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dbshub.Models
{
    public class TableSchema
    {
        public FieldSchema[] Fields { get; set; }
    }

    public class FieldSchema {
        public string Name { get; set; }
        public string Caption { get; set; }

    }
}