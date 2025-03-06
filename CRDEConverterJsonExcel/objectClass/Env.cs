using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDEConverterJsonExcel.objectClass
{
    class Env : IValidatableObject
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string API { get; set; }

        // Unique Validate
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var environment = validationContext.Items["EnvironmentList"] as IEnumerable<Env>;
            if (environment != null && environment.Any(p => (p.Name == Name && p.API != API) || (p.Name != Name && p.API == API)))
            {
                yield return new ValidationResult("Environment was exist.", new[] { nameof(Name) });
            }
        }
    }
}
