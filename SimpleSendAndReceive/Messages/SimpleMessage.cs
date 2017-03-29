namespace Messages
{
    using System;
    
    [Serializable]
    public class SimpleMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"Id: {Id} Text: '{Text}'";
        }
    }
}
