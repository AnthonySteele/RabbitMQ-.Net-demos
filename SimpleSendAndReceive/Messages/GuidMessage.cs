namespace Messages
{
    using System;

    [Serializable]
    public class GuidMessage
    {
        public Guid Identifier { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return $"Id: {Identifier} Content: '{Content}'";
        }
    }
}
