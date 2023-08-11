using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace StaticFileSecureCall.Validation
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UniqueFieldsAttribute : ValidationAttribute
    {
        private readonly string _fieldNames;
        private readonly string _errorMessage;
        public UniqueFieldsAttribute(string fieldNames)
        {
            _fieldNames = fieldNames;
            _errorMessage = "Duplicate Values detected on model field";
        }
        //public string[] names { get { return _fieldNames; } }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            AppDbContext dbContext = (AppDbContext)validationContext.GetService(typeof(AppDbContext));
            if (dbContext == null)
            {
                throw new InvalidOperationException("Database context not available.");
            }
            var entity = validationContext.ObjectInstance;
            if (_fieldNames.Contains(','))
            {
                foreach (var fieldName in _fieldNames.Split(','))
                {
                    // Perform validation logic based on fieldName and dbContext
                    var property = entity.GetType().GetProperty(fieldName);
                    var fieldValue = property?.GetValue(entity);
                    var matchingEntities = dbContext.Set<FileRepository>()
                        .Where(e => e != entity && property.GetValue(e).Equals(fieldValue))
                        .ToList();
                    if (matchingEntities.Any())
                    {
                        return new ValidationResult(ErrorMessage = $"{_errorMessage} {fieldName}"); ;
                    }
                }
            }
            else
            {
                // Perform validation logic based on fieldName and dbContext
                var property = entity.GetType().GetProperty(_fieldNames);
                var fieldValue = property?.GetValue(entity);
                var matchingEntities = dbContext.Set<FileRepository>()
                    .Where(e => e != entity && property.GetValue(e).Equals(fieldValue))
                    .ToList();
                if (matchingEntities.Any())
                {
                    return new ValidationResult(ErrorMessage = $"{_errorMessage} {_fieldNames}");
                }
            }
            return ValidationResult.Success;
        }
    }
}