// k6 run slow-paced.js

import { sleep } from 'k6';
import http from 'k6/http';
import { vu, scenario } from 'k6/execution';

export const options = {
    discardResponseBodies: true,
	
	scenarios: {
        coldStart: {
            executor: 'per-vu-iterations',
            startTime: '0s',
            vus: 1,
            iterations: 1
        },
        fewDevices: {
            executor: 'per-vu-iterations',
            startTime: '2s',
            vus: 10,
            iterations: 1
        }
    }
};

const serviceUrl = "http://localhost:5264";

const buildInitialState = (deviceId, scenarioName) => {
    const devId = `AL-${deviceId.toString().padStart(4, "0")}`;
    console.log(`[${scenarioName} - ${devId}] Creating device`);
    return {
        DevId: devId,
        Temp: 15.5,
        Lat: 13.5698,
        Lon: 35.6974
    };
}

const sendDeviceState = deviceStateToSend => {
    const params = {
        headers: { 'Content-Type': 'application/json' }
    };

    http.post(`${serviceUrl}/event`, JSON.stringify(deviceStateToSend), params);
}

const remainTheSameForSomeCycles = (withDeviceState, scenarioName) => {
    const cycles = Math.round(Math.random() * 10);
    for(let i = 1; i <= cycles; i++)
    {
        console.log(`[${scenarioName} - ${withDeviceState.DevId}] Remaining the same for ${i}/${cycles} time`);
        sendDeviceState(withDeviceState);
        sleep(1);
    }
}

const runColdStartScenario = (deviceId, scenarioName) => {
    const initialDeviceState = buildInitialState(deviceId, scenarioName);
    sendDeviceState(initialDeviceState);
}

const runFewDevicesScenario = (deviceId, scenarioName) => {
    const initialDeviceState = buildInitialState(deviceId, scenarioName);
    sendDeviceState(initialDeviceState);
    remainTheSameForSomeCycles(initialDeviceState, scenarioName);
}

export default function () {
    if(scenario.name === 'coldStart')
        runColdStartScenario(vu.idInTest, scenario.name);
    else if(scenario.name === 'fewDevices')
        runFewDevicesScenario(vu.idInTest, scenario.name);
}
