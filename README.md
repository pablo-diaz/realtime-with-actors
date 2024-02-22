# Device events tracing application on realtime

## Overall architecture
![Overall architecture](./doc/architecture.jpg?raw=true)

## Motivation
I wanted to create this solution architecture, to learn about some technologies and to apply some concepts.

## Use Case
I wanted to know how I could trace devices, about their temperature and location, so I can quickly be notified when a device temperature has increased or decreased, or its location has changed, as well as have certain alarms, such as when devices get out of a specific scope/region, or their temperature get off a specific range I would be interested to watch out for.

## Solution strategy
I acquire device data (current location and temperature) quite often, and I send it to a Broker Queue (here RabbitMQ), so that it can pass that info to the Actor Model (here Proto.Actor) where device state is being tracked by each Actor that represents a single device. When certain conditions are met (new device location, or new temperature) then domain events are pushed as notifications to the user's browser, using ServerSentEvents (not WebSockets) with a cool tool called PushPin. In the functional tests folders, you can find an HTML file where you can view (in real time) how these devices are rendered in a world map and track their location and temperature. Also, I wanted to allow the user to set certain watching zones, where they can monitor which devices would ever get out of that (sqared) zone, and be notified as soon as possible; same for watching out for devices that get off from a defined temperature range.

## Technologies and Concepts used
- Domain Driven Design: most of its patterns: Aggregate, Entity, Value Object, Domain Events
- https://proto.actor/ To place an actor model for this solution, using C# and .Net Core 8
- https://pushpin.org/  For real time communication with users (push notifications for events), not necesarily with websockets. Here I use ServerSentEvents
- https://www.influxdata.com/  Perfect time-based database to store both device metics and their related events
- https://rabbitmq.com/  To capture device events and pass them over to the actor model/system
- https://k6.io/  To simulate stress conditions, when sending device data, such as 10K concurrent devices sending information
- https://www.influxdata.com/time-series-platform/telegraf/   To monitor RabbitMQ performance
- ASP.Net Core 8 with C#
