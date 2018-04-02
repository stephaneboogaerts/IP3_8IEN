﻿using System.Data.Entity;
using IP3_8IEN.BL.Domain.Data;
using IP3_8IEN.BL.Domain.Gebruikers;
using System;

namespace IP3_8IEN.DAL.EF
{
    [DbConfigurationType(typeof(OurDbConfiguration))]
    internal class OurDbContext : DbContext
    {
        private readonly bool delaySave;


        public OurDbContext(bool unitOfWorkPresent = false) : base("OurDB_EFCodeFirst")
        {
            //Database.SetInitializer<OurDbContext>(new OurDbInitializer());
            delaySave = unitOfWorkPresent;
        }

        //16 mrt 2018 : Stephane
        public DbSet<Message> Messages { get; set; }
        //27 mrt 2018 : Stephane
        public DbSet<SubjectMessage> SubjectMessages { get; set; }
        public DbSet<Persoon> Personen { get; set; }
        //28 mrt 2018 : Stephane
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<Onderwerp> Onderwerpen { get; set; }
        //30 mrt 2018 : Stephane
        public DbSet<Gebruiker> Gebruikers { get; set; }
        public DbSet<AlertInstelling> AlertInstellingen { get; set; }
        //31 mrt 2018 : Stephane
        public DbSet<Alert> Alerts { get; set; }
        //2 apr 2018 : Stephane
        public DbSet<Organisatie> Organisaties { get; set; }
        public DbSet<Tewerkstelling> Tewerkstellingen { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //hier komt 'Fluent api' als je dat nodig zou hebben
        }

        public override int SaveChanges()
        {
            if (delaySave) return -1;
            return base.SaveChanges();
        }
        internal int CommitChanges()
        {
            if (delaySave)
            {
                return base.SaveChanges();
            }
            throw new InvalidOperationException("No UnitOfWork present, use SaveChanges instead");
        }
    }
}
