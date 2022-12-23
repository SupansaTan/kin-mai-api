namespace KinMai.Api.Models
{
    public class ResponseModel<T>
    {
        public T Data { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public ResponseModel()
        {
            this.Data = default(T);
            this.Status = 400;
            this.Message = "Bad request.";
        }
    }
}
