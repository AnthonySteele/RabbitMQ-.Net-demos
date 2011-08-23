namespace Messages
{
    using System;
    
    [Serializable]
    public class ReplyMessage
    {
        public int Id { get; set; }
        public string Answer { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} Response: '{1}'", Id, Answer);
        }
    }
}
