# Device events tracing application on realtime

## Overall architecture
![Overall architecture](./doc/architecture.jpg?raw=true)

## Motivation
I wanted to create this solution architecture, to learn about some technologies and to apply some concepts.

## Use Case
I wanted to know how I could trace devices, about their temperature and location, so I can quickly be notified when a device temperature has increased or decreased, or its location has changed, as well as have certain alarms, such as when devices get out of a specific scope/region, or their temperature get off a specific range I would be interested to watch out for.

## Solution strategy

- I acquire device data (current location and temperature) quite often (once every second)
- Then these metrics are sent to a broker Queue (here RabbitMQ)
- Then the device state service subscribes to that Queue, consuming and processing each metric at its own pace
- Device state is done using an Actor Model (here with the Proto.Actor implementation), where device state is being tracked by each Actor that represents a single device
- Once metrics are processed, then Domain Events are fired; these events are stored in the event log and then they are pushed as notifications to the user's browser, using ServerSentEvents (not WebSockets) with a cool tool called PushPin.
- In the functional tests folders, you can find an HTML file where you can view (in real time) how these devices are rendered in a world map and track their location and temperature.
- Also, for resiliency purposes, if the device state service (the actor model) is restarted, then it restores each device state to the most recent state, restoring it from the event log (i.e. using event sourcing technique)
- Idle devices are removed from the actor model, to improve service memory consumption
- You can switch the implementation when persisting domain events, from KurrentDb (known before as Event Store DB) to InfluxDB (TimeSeries database)
- I also wanted to allow the user to set certain watching zones, where they can monitor which devices would ever get out of that (sqared) zone, and be notified as soon as possible; same for watching out for devices that get off from a defined temperature range.

## Technologies and Concepts used
- Domain Driven Design: most of its patterns: Aggregate, Entity, Value Object, Domain Events
- Event Sourcing: to improve resiliency to the actor model state
- https://proto.actor/ Actor Model implementation, using C# and .Net Core 8
- https://pushpin.org/  For real time communication with users (push notifications for events), not necesarily with websockets. Here I use ServerSentEvents
- https://www.influxdata.com/  Perfect time-based database to store Domain Events
- https://www.kurrent.io/ Event Store database to store all domain events and restore Actor State when restarting stateful service
- https://rabbitmq.com/  To capture device metrics and pass them over to the actor model
- https://k6.io/  To simulate stress conditions, when sending device data, such as 10K concurrent devices sending information
- Unit Testing a Domain
- ASP.Net Core 8 with C#
- https://www.influxdata.com/time-series-platform/telegraf/ To monitor RabbitMQ performance

## Actor Model
![ActorModel](./doc/actor-model-design.png?raw=true)

## Simulation with stress tests
![Simulation](./doc/simulation01.jpg?raw=true)

## K6 stress test performance metrics
![K6](./doc/k6-stress-test-outcome.png?raw=true)

## RabbitMQ message consumption
![RMQC](./doc/rabbitmq-message-consumption.png?raw=true)

## RabbitMQ consumption contention
![RMQMC](./doc/rabbitmq-contention.png?raw=true)
