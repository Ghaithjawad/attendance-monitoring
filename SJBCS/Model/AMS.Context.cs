﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SJBCS.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class AMSEntities : DbContext
    {
        public AMSEntities()
            : base("name=AMSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<Biometric> Biometrics { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<DistributionList> DistributionLists { get; set; }
        public virtual DbSet<Level> Levels { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<RelBiometric> RelBiometrics { get; set; }
        public virtual DbSet<RelDistributionList> RelDistributionLists { get; set; }
        public virtual DbSet<RelOrganization> RelOrganizations { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
    
        public virtual ObjectResult<SP_ListSection_Result> SP_ListSection()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SP_ListSection_Result>("SP_ListSection");
        }
    
        public virtual ObjectResult<SP_SP_ListStudent_Result> SP_ListStudent()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SP_SP_ListStudent_Result>("SP_ListStudent");
        }
    
        public virtual ObjectResult<SP_ListStudentContact_Result> SP_ListStudentContact()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SP_ListStudentContact_Result>("SP_ListStudentContact");
        }
    
        public virtual ObjectResult<ListStudent_Result> ListStudent_Result()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<ListStudent_Result>("ListStudent_Result");
        }
    
        public virtual ObjectResult<ListStudent_Result> ListStudent()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<ListStudent_Result>("ListStudent");
        }
    
        public virtual ObjectResult<Section> ListSection()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Section>("ListSection");
        }
    
        public virtual ObjectResult<Section> ListSection(MergeOption mergeOption)
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Section>("ListSection", mergeOption);
        }
    }
}