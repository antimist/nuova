﻿using System;
using System.Collections.Generic;
using MyCourse.Models.Enums;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Entities
{
    public partial class Course
    {
        public Course(string title, string author)
        {
            // Modifica come da sezione 17 lezione 139
            // ----------------------- INIZIO ------------------------------
            //if (string.IsNullOrWhiteSpace(title))
            //{
            //    throw new ArgumentException("The Course must have a title");
            //}
            //
            //if (string.IsNullOrWhiteSpace(author))
            //{
            //    throw new ArgumentException("The Course must have a author");
            //}

            //Title   = title;
            //Author  = author;
            ChangeTitle(title);
            ChangeAuthor(author);
            ChangeStatus(CourseStatus.Draft);
             // ----------------------- FINE ------------------------------
            Lessons = new HashSet<Lesson>();
            CurrentPrice = new Money(Currency.EUR, 0);
            FullPrice = new Money(Currency.EUR, 0);
            ImagePath = "/Courses/default.png";

        }

        public long Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Author { get; private set; }
        public string Email { get; private set; }
        public double Rating { get; private set; }
        public Money FullPrice { get; private set; }
        public Money CurrentPrice { get; private set; }
        public string RowVersion { get; private set; }
        public CourseStatus Status {get; private set;}

        public void ChangeStatus (CourseStatus newStatus)
        {
            //TODO: LOGICA DI VALIDAZIONE
            Status= newStatus;
        }

        public void ChangeTitle(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                throw new ArgumentException("The Course must have a title");
            }            
            Title= newTitle;
         }

        public void ChangePrices(Money newFullPrice, Money newDiscountPrice)
        {
            if(newFullPrice == null || newDiscountPrice == null)
            {
                throw new ArgumentException("The Course must have a title");
            }
            if(newFullPrice.Currency != newDiscountPrice.Currency)
            {
                throw new ArgumentException("The Course must have a title");
            }
            if(newFullPrice.Amount <= newDiscountPrice.Amount)
            {
                throw new ArgumentException("The Course must have a title");
            }
            FullPrice = newFullPrice;
            CurrentPrice= newDiscountPrice;                
        }

        public void ChangeEmail (string newEmail)
        {
            if(string.IsNullOrEmpty(newEmail))
            {
                throw new ArgumentException("Email can't be empty");
            }
            Email=newEmail;
        }

        public void ChangeDescription(string newDescription)
        {
            if(string.IsNullOrEmpty(newDescription))
            {
                throw new ArgumentException("Email can't be empty");
            }
            Description=newDescription;
        }
        public virtual ICollection<Lesson> Lessons { get; private set; }

        internal void ChangeImagePath(string imagePath)
        {
            if(string.IsNullOrEmpty(imagePath))
            {
                throw new NotImplementedException();
            }
            ImagePath = imagePath;
        }

        public void ChangeAuthor(string newAuthor)
        {
            if (string.IsNullOrWhiteSpace(newAuthor))
            {
               throw new ArgumentException("The Course must have a author");
            }
            Author = newAuthor;
        }
    }
}
