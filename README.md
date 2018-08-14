# SignalR Hub on PCF

This is a basic example of a .NET Core SignalR hub that will run on Pivotal Cloud Foundry.

This example includes the following functionality.

* Implements CORS for limiting cross origin requests
* Implements JSESSIONID cookie for sticky session during negotiation (https://docs.cloudfoundry.org/concepts/http-routing.html)
* Implements connection to Redis for backplane
* Handles scale out of the SignalR endpoint
