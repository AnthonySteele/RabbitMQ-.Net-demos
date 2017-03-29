namespace Messages
{
    using System;

    [Serializable]
    public class RequestMessage
    {
        public int Id { get; set; }
        public string Request { get; set; }

        public override string ToString()
        {
            return $"Id: {Id} Request: '{Request}'";
        }
    }
}
