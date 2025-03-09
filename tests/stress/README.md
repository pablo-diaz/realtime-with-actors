# Device load tests
These stress tests try to simulate how real devices would be sending their events through to our API

Please run these stress tests with https://k6.io/

## Examples:

k6 run --config slow-paced-scenarios.js testing-logic.js

k6 run --config stressing-scenarios.js testing-logic.js
