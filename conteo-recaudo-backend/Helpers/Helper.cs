namespace ConteoRecaudo.Helpers
{
    public class Helper
    {
        public static bool FechasValidas(DateTime fechaInicial, DateTime fechaFinal)
        {
            bool RangoValido = true;
            
            if (fechaInicial < fechaFinal)
            {
                RangoValido = false;
            }
            
            return RangoValido;
        }

        public static void CrearDirectorio(string RutaDirectorio)
        {
            if (!Directory.Exists(RutaDirectorio))
            {
                Directory.CreateDirectory(RutaDirectorio);
            }
        }

        public static void EliminarDirectorio(string RutaDirectorio)
        {
            if (Directory.Exists(RutaDirectorio))
            {
                Directory.Delete(RutaDirectorio);
            }
        }
    }
}
