﻿using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;
using System;
using System.Collections.Generic;

namespace MyCourse.Models.Entities
{
    public partial class Course
    {
        public Course(string title, string author)
        {
            ChangeTitle(title);
            ChangeAuthor(author);
            Lessons = new HashSet<Lesson>();
            CurrentPrice = new Money(Currency.EUR, 0);
            FullPrice = new Money(Currency.EUR, 0);
            ImagePath = "/Courses/default.png";
        }

        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Author { get; private set; }
        public string Email { get; private set; }
        public double Rating { get; private set; }
        public Money FullPrice { get; private set; }
        public Money CurrentPrice { get; private set; }
        public string RowVersion { get; private set; }
        public CourseStatus Status { get; private set; }

        public void ChangeStatus(CourseStatus newStatus)
        {
            //TODO: aggiungere logica di validazione
            Status = newStatus;
        }

        public void ChangeAuthor(string newAuthor)
        {
            if (string.IsNullOrWhiteSpace(newAuthor))
            {
                throw new ArgumentException("The author must have a name");
            }
            Author = newAuthor;
        }

        public void ChangeTitle(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                throw new ArgumentException("The course must have a title");
            }
            Title = newTitle;
        }

        public void ChangePrices(Money newFullPrice, Money newDiscountPrice)
        {
            if (newFullPrice == null || newDiscountPrice == null)
            {
                throw new ArgumentException("Prices can't be null");
            }
            if (newFullPrice.Currency != newDiscountPrice.Currency)
            {
                throw new ArgumentException("Currencies don't match");
            }
            if (newFullPrice.Amount < newDiscountPrice.Amount)
            {
                throw new ArgumentException("Full price can't be less than the current price");
            }
            FullPrice = newFullPrice;
            CurrentPrice = newDiscountPrice;
        }

        public void ChangeEmail(string newEmail)
        {
            if (string.IsNullOrEmpty(newEmail))
            {
                throw new ArgumentException("Email can't be empty");
            }
            Email = newEmail;
        }

        public void ChangeDescription(string newDescription)
        {
            if (newDescription != null)
            {
                if (newDescription.Length < 20)
                {
                    throw new Exception("Description is too short");
                }
                else if (newDescription.Length > 4000)
                {
                    throw new Exception("Description is too long");
                }
            }
            Description = newDescription;
        }

        public void ChangeImagePath(string imagePath)
        {
            ImagePath = imagePath;
        }

        public virtual ICollection<Lesson> Lessons { get; private set; }
    }
}
