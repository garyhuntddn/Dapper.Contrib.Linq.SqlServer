// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;
using DDN.SharedCode;

namespace Example.DAL
{

  [Table( "child", Schema = "dbo" )]
  public class Child 
  {
    [Key]
    [Required]
    [Display( Name = "Child" )]
    public Guid ChildId { get; set; } 

    [Required]
    [Timestamp]
    [Column( "rowVersion" )]
    [Display( Name = "Row version" )]
    public byte[] Timestamp { get; set; } 

    [Required]
    [StringLength( 100 )]
    [Unicode( false )]
    [Display( Name = "Name" )]
    public string Name { get; set; } 

    [Required]
    //[ForeignKeyConstraint( typeof( Header ) )]
    [UIHint( "HeaderId" )]
    [Display( Name = "Header" )]
    public Guid HeaderId { get; set; } 

    [DataType( DataType.DateTime )]
    [Display( Name = "Created" )]
    public DateTime? Created { get; set; } 

    // foreign keys
 
    [ForeignKey( "HeaderId" )]
    [NotMapped] // this will probably break EF
    public virtual Header FK_HeaderId { get; set; }
  }
}