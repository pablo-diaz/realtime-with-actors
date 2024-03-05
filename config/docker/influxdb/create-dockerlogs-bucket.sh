#!/bin/sh

set -e
influx bucket create -n docker-logs -o ${DOCKER_INFLUXDB_INIT_ORG} -r 7d
