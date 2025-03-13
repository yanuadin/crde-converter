using System.ComponentModel.DataAnnotations;

namespace CRDEConverterJsonExcel.objectClass
{
    class Env : IValidatableObject
    {
        public string Name { get; set; } = "";

        public string API { get; set; } = "";

        public string HostName { get; set; } = "";

        public string Port { get; set; } = "";

        public string AccessKeyID { get; set; } = "";

        public string SecretAccessKey { get; set; } = "";

        // Unique Validate
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var environment = validationContext.Items["EnvironmentList"] as IEnumerable<Env>;
            if (environment != null && environment.GroupBy(e => e.Name).Any(eg => eg.Count() > 1))
            {
                yield return new ValidationResult("Environment Name was exist.", new[] { nameof(Name) });
            }
        }
    }
}
