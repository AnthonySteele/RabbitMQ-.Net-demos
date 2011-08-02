Rabbit MQ .Net Demos
====================
These small C# programs that demonstrate simple use of RabbitMQ in .Net code. I used Visual studio 2010, c# and Nuget. The Nuget package for the .Net RabbitMq client is used in this solution.

The programs assume that you have a RabbitMQ broker installed on localhost. If your broker is elsewhere, you can change the constant for this. 

These demos do not address reliability. Exception handling is rudimentary and no provision is made for reconnecting if a connection is lost.

No abstraction of common RabbitMQ-handling code into libraries or wrappers has been done. Not because this is a bad idea, but because it would unnecessarily complicate the sample. It is a good idea in a more complex program, but any approach taken in these simple samples will likely be a false start for your needs.

Simple Send And Receive
=======================

This pair of small C# programs demonstrates a very simple use of RabbitMQ. The sender puts messages on a queue. The receiver takes them off.

You can launch multiple senders, and they will all put data onto the same queue. This demonstrates a many-to-one scenario. 

You can launch multiple receivers, and they will take turns getting messages off the queue. Each message will be processed by only one receiver. This demonstrates a round-robin load balancing scenario.

If a sender is launched but a receiver is launched later, messages will be retained on the queue until a receiver dequeues them.

Two different types are used as messages. This demonstrates versatility: sending, transporting and handling more than one kind of message, and how this maps to what I think is the preferred way to do it: each kind of message is a C# type, instances of which function as Data Transfer Objects across the message queue.

A RabbitMQ message is just an array of bytes. So it is important to turn these from and to c# objects. There is a project called "Messages" in my code, which contains the serializable types that are sent across the message queue as bytes. This design relies on the sender and receiver using the same version of the same types as messages, so that they can serialize and deserialize symmetrically. If your architecture does not allow this - e.g. if either the sender or receiver is not written in .Net, then you will need another way to interpret data. In that case, my first choice would be to use the XML serializer to turn objects to XML, then send the XML as UTF-8 data. You could do similar with JSON if that's what you prefer. Failing that, the sender and receiver would have to agree on the meaning of the contents of the byte arrays.

PubSub
======

This pair of small C# programs demonstrates use of RabbitMQ in a publish and subscribe manner, in which each message is sent to all current subscribers. This is similar to the Java example at http://www.rabbitmq.com/tutorials/tutorial-three-java.html

In order to use this, you need one new concept: An Exchange.  The sender sends messages to the exchange, not directly to a queue. The exchange copies received messages onto all attached queues. The exchange has zero or more queues, one for each subscriber.  

The exchange does not buffer messages - that is what queues do. If there are no queues attached when a message is received by the exchange, the message is discarded. This is a feature of broadcast messaging - you don't need to now how many listeners there are, or even if there are any. 

The subscriber's queue is temporary, it is created for the subscriber, and is deleted when the subscriber disconnects. 

Notes: 

- The code is very similar to the first example. The part that has changed is just the RabbitMQ setup - sending and receiving messages, program flow, output and reading the keyboard are unchanged.

- The kind of exchange used in this example is a "fanout" exchange. 

- The "Simple Send And Receive" example does in fact use an exchange, a default one to which the named queue is attached.  

- In the pub-sub example, the subscriber does not pick a name for its queue - that could result in name clashes. Instead a unique name is generated and returned from QueueDeclare(). It's not a GUID, but it's just as unreadable.

- There are ways for exchanges to filter messages so that some messages are sent to some queues and not others based on message metadata (the "routing key"), but that is outside the scope of this example.

==============
Anthony Steele