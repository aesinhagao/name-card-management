﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NCMSystem.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class NCMSystemEntities : DbContext
    {
        public NCMSystemEntities()
            : base("name=NCMSystemEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<contact> contacts { get; set; }
        public virtual DbSet<flag> flags { get; set; }
        public virtual DbSet<group> groups { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<status> status { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<token> tokens { get; set; }
        public virtual DbSet<user> users { get; set; }
    }
}