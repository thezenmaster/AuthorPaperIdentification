//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Journal
    {
        public Journal()
        {
            this.Papers = new HashSet<Paper>();
        }
    
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string HomePage { get; set; }
    
        public virtual ICollection<Paper> Papers { get; set; }
    }
}
