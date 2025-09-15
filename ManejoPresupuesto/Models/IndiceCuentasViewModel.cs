namespace ManejoPresupuesto.Models
{
    public class IndiceCuentasViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; } //Coleccion
        public decimal Balance => Cuentas.Sum(x => x.Balance); //suma cada objeto x.Balance para obtener la suma total

    }
}
