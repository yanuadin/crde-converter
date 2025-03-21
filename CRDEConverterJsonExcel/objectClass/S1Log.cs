using System.ComponentModel.DataAnnotations;

namespace CRDEConverterJsonExcel.objectClass
{
    class S1Log : IValidatableObject
    {
        public string UUID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string HostName { get; set; } = "";

        [Required]
        public string Port { get; set; } = "";

        [Required]
        public string AccessKeyID { get; set; } = "";

        [Required]
        public string SecretAccessKey { get; set; } = "";

        [Required]
        public string DirectoryS1 { get; set; } = "";


        // Unique Validate
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var s1Log = validationContext.Items["S1LogList"] as IEnumerable<S1Log>;
            if (s1Log != null && s1Log.GroupBy(e => e.Name).Any(eg => eg.Count() > 1))
            {
                yield return new ValidationResult("S1 Log Name was exist.", new[] { nameof(Name) });
            }
        }
    }
}
