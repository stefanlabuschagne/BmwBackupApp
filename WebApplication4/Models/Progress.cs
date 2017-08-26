using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication4.Models
{
    public class Progress
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public String ID { get; set; }
        public String Guid { get; set; }
        public DateTime DateTime { get; set; }
        public string Description { get; set; }

    }
}