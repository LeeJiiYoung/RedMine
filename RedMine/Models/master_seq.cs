using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RedMine.Models
{
    public class Master_seq
    {
        //ID
        [Required]
        public int id { get; set; }

        //시퀀스명
        [Required]
        public string seq_name{ get; set; }
    }
}