namespace ConteoRecaudo.Helpers.HttpExeptionHelper
{
    public class HttpException : Exception
    {
        public int HttpStatusCode { get; }
        public HttpException(int httpStatusCode, string message, Exception inner) : base(message, inner)
        {
            HttpStatusCode = httpStatusCode;
        }
        public HttpException(int httpStatusCode, string message) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
