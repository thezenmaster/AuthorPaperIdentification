﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AuthorPaperIdentification
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AuthorPaperEntities : DbContext
    {
        public AuthorPaperEntities()
            : base("name=AuthorPaperEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Author> Authors { get; set; }
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Paper> Papers { get; set; }
        public DbSet<PaperAuthor> PaperAuthors { get; set; }
    }
}
