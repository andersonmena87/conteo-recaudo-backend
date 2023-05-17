namespace ConteoRecaudo.Helpers
{
    public class Helper
    {
        public static bool FechasValidas(DateTime fechaInicial, DateTime fechaFinal)
        {
            bool rangoValido = true;
            DateTime fechaMaxima = fechaInicial.AddMonths(1);

            if (fechaInicial > fechaFinal || fechaMaxima < fechaFinal)
            {
                rangoValido = false;
            }

            return rangoValido;
        }

    }
}
