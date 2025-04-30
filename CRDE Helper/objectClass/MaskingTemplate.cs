using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace CRDEConverterJsonExcel.objectClass
{
    class MaskingTemplate : IValidatableObject
    {
        [Required]
        public string Name { get; set; }

        public ObservableCollection<Masking> Mask { get; set; } = new ObservableCollection<Masking>();

        // Unique Validate
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var maskingTemplate = validationContext.Items["MaskingTemplateList"] as IEnumerable<MaskingTemplate>;
            if (maskingTemplate != null && maskingTemplate.GroupBy(mt => mt.Name).Any(mtg => mtg.Count() > 1))
            {
                yield return new ValidationResult("Masking template name was exist.", new[] { nameof(Name) });
            }
        }
    }

    class Masking : IValidatableObject
    {

        [Required]
        public string Variable { get; set; }
        
        [Required]
        public string Value { get; set; }

        // Unique Validate
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var masking = validationContext.Items["MaskingList"] as IEnumerable<Masking>;
            if (masking != null && masking.GroupBy(m => m.Variable).Any(mg => mg.Count() > 1))
            {
                yield return new ValidationResult("Masking variable was exist.", new[] { nameof(Variable) });
            }
        }
    }
}
