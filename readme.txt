This is a pair of small C# programs that demonstrate a very simple use of RabbitMQ in .Net code. I used Visual studio 2010, c# and Nuget. The Nuget  package for the .Net RabbitMq client is used in this solution.

The sender puts messages on a queue. The receiver takes them off.

You can launch multiple senders, and they will all put data onto the same queue. This demonstrates a many-to-one scenario. 
You can launch multiple receivers, and they will take turns getting messages off the queue. Each message will be processed by only one receiver. This demonstrates a round-robin load balancing scenario.

Two different types are used as messages. This demonstrates versatility: sending, transporting and handling more than one kind of message, and how this maps to what I think is the preferred way to do it: each kind of message is a C# type, instances of which function as Data Transfer Objects across the message queue.

The programs assume that you have a RabbitMQ broker installed on localhost. If your broker is elsewhere, you can change the constant for this. 

A RabbitMQ message is just an array of bytes. So it is important to turn these from and to c# objects. There is a project in my code called "messages" that contains the serializable types that are sent across the message queue as bytes. This design relies on the sender and receiver using the same version of the same types as messages, so that they can serialize and deserialize symmetrically. If your architecture does not allow this - e.g. if either the sender or receiver is not written in .Net, then you will need another way to interpret data. In that case, my first choice would be to use the XML serializer to turn objects to XML, then send the XML as UTF-8 data. You could do similar with JSON if that's what you prefer to XML. Failing that, the sender and receiver would have to agree on the meaning of the contents of the byte arrays.

This demo does not address reliability. Exception handling is rudimentary and no aprovision is made for reconnecting if a connection is lost.

No abstraction of common RabbitMQ-handling code into libraries or wrappers has been done. Not because this is a bad idea, but because it would unnecessarily complicate the sample. It is a good idea in a more complex program, but any approach taken in this simple sample will likely be a false start for your needs.
