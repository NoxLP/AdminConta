using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModuloContabilidad
{
    public class AsientoDinamico_ImporteValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
        {
            TabExpTabAsientoVM VM = (TabExpTabAsientoVM)((BindingExpression)owner).DataItem;
            string input = (string)value;

            if (!VM.Asiento.Abierto)
                return new ValidationResult(false, GlobalSettings.Properties.ContabilidadSettings.Default.MENSAJESERROR_ASIENTO_ASIENTOCERRADO);

            decimal importe;
            if (!decimal.TryParse(input, out importe))
            {
                if (!VM.AsientoComplejoSiHayMasDeDosApuntes())
                    return new ValidationResult(false, GlobalSettings.Properties.ContabilidadSettings.Default.MENSAJESERROR_ASIENTO_LETRASENCELDAIMPORTE);

                return new ValidationResult(true, null);
            }

            return new ValidationResult(true, null);
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }
}
