# Device load tests
These stress tests try to simulate how real devices would be sending their events through to our API

Please run these stress tests with https://k6.io/

## Examples:

### With regular K6 instalation

k6 run --config slow-paced-scenarios.js testing-logic.js

k6 run --config stressing-scenarios.js testing-logic.js

### With K6 adding module to export testing progress metrics to InfluxDB

docker run -d --name k6-load-tests-with-influx --env-file docker-env-vars -v $(pwd):/scripts:ro pabloandresd/k6:influx run --config /scripts/less-complex-scenarios.js /scripts/testing-logic.js
