using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.InputModels
{
    public class CourseEditInputModel : IValidatableObject
    {
        [Required]
        public int Id {get; set;}

        [Required(ErrorMessage = "Il titlolo è obbligatorio"),
         MinLength(10, ErrorMessage = "Il titlolo dev'essere almeno {1} caratteri"),
         MaxLength(100, ErrorMessage = "Il titlolo dev'essere al masimo {1} caratteri"),
         RegularExpression(@"^[\w\s\.]+$", ErrorMessage = "Titolo non valido"),
         Remote(action: nameof(CourserController.IsTitleAviable), controller: "Courser", ErrorMessage = "Il titolo esiste già", AdditionalFields ="Id"),
         Display(Name = "Titolo")]
        public string Title {get; set;}

        [MinLength(10, ErrorMessage = "La Descrizione dev'essere almeno {1} caratteri"),
         MaxLength(4000, ErrorMessage = "La Descrizione dev'essere al masimo {1} caratteri"),
         Display(Name = "Descrizione")]
         public string Description {get; set;}

         [Display(Name = "Immagine rappresentativa")]
        public string ImagePath {get; set;}

        [Required(ErrorMessage = "L'email di contatto è obbligatoria"),
         EmailAddress(ErrorMessage = "Devi inserire un indirizzo email"),
         Display(Name = "Email di contatto")]
        public string Email {get; set;}

        [Required (ErrorMessage = "Il prezzo intero è obbligatorio"),
         Display(Name ="Prezzo Intero")]


        public Money FullPrice {get; set;}

        [Required (ErrorMessage = "Il prezzo corrente è obbligatorio"),
         Display(Name ="Prezzo Corrente")]
        public Money CurrentPrice {get; set;}     
        [Display (Name = "Nuova immagine...")]
        public IFormFile Image {get; set;}
        public string RowVersion { get;  set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FullPrice.Currency != CurrentPrice.Currency)
            {
                yield return new ValidationResult("Il prezzo intero deve avere la stessa valuta del prezzo corrente", new [] {nameof(FullPrice)});
            }
            else if (FullPrice.Amount < CurrentPrice.Amount)
            {
                yield return new ValidationResult("Il prezzo intero non può essere inferiore al prezzo corrente,", new [] {nameof(FullPrice)});
            }
        }
        
        public static CourseEditInputModel FromDataRow(DataRow courseRow)
        {
            CourseEditInputModel courseEditInputModel = new CourseEditInputModel()
            {
                Title = Convert.ToString(courseRow["Title"]),
                Description = Convert.ToString(courseRow["Description"]),
                ImagePath = Convert.ToString(courseRow["ImagePath"]),
                Email = Convert.ToString(courseRow["Email"]),
                FullPrice = new Money(
                    Enum.Parse<Currency>(Convert.ToString(courseRow["FullPrice_Currency"])),
                    Convert.ToDecimal(courseRow["FullPrice_Amount"])
                ),
                CurrentPrice = new Money(
                    Enum.Parse<Currency>(Convert.ToString(courseRow["CurrentPrice_Currency"])),
                    Convert.ToDecimal(courseRow["CurrentPrice_Amount"])
                ),
                Id = Convert.ToInt32(courseRow["Id"]),
                RowVersion = Convert.ToString(courseRow["RowVersion"])
            };
            return courseEditInputModel;            
        }

        internal static CourseEditInputModel FromEntity(Course course)
        {
            return new CourseEditInputModel 
            {
                Id = Convert.ToInt32(course.Id),
                Title = course.Title,
                Description = course.Description,
                ImagePath = course.ImagePath,
                Email = course.Email,
                FullPrice = course.FullPrice,
                CurrentPrice = course.CurrentPrice,
                RowVersion = course.RowVersion
            };
            throw new NotImplementedException();
        }
    }
}