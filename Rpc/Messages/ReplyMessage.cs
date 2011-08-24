namespace Messages
{
    using System;
    
    [Serializable]
    public class ReplyMessage
    {
        public int Id { get; set; }
        public string Reply { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} Reply: '{1}'", Id, this.Reply);
        }
    }
}
