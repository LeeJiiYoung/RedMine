using RedMine.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RedMine
{
    public class MainDbContext : DbContext
    {
        public MainDbContext() : base("name=MySQLConnStr")
        {

        }
        public DbSet<Master_seq> master_seq { get; set; }
    }
}