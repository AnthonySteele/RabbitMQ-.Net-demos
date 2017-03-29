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
            return $"Id: {Id} Reply: '{Reply}'";
        }
    }
}
