#!/bin/sh

set -e
influx bucket create -n actor-model-performance -o ${DOCKER_INFLUXDB_INIT_ORG} -r 7d
