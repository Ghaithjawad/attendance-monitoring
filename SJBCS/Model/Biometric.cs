//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Biometric
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Biometric()
        {
            this.RelBiometrics = new HashSet<RelBiometric>();
        }
    
        public System.Guid FingerID { get; set; }
        public byte[] FingerPrintTemplate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RelBiometric> RelBiometrics { get; set; }
    }
}
