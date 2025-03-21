using System.ComponentModel.DataAnnotations;

namespace CRDEConverterJsonExcel.objectClass
{
    class APIAddress : IValidatableObject
    {
        public string UUID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string API { get; set; } = "";

        // Unique Validate
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var apiAddress = validationContext.Items["APIAddressList"] as IEnumerable<APIAddress>;
            if (apiAddress != null && apiAddress.GroupBy(e => e.Name).Any(eg => eg.Count() > 1))
            {
                yield return new ValidationResult("API Name was exist.", new[] { nameof(Name) });
            }
        }
    }
}
