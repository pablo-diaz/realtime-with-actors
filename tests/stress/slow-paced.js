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
        oneDevice: {
            executor: 'per-vu-iterations',
            startTime: '1s',
            vus: 1,
            iterations: 1
        },
        tenDevices: {
            executor: 'per-vu-iterations',
            startTime: '100s',
            vus: 10,
            iterations: 1
        },
        fiftyDevices: {
            executor: 'per-vu-iterations',
            startTime: '200s',
            vus: 50,
            iterations: 1
        },
        twoHundredDevices: {
            executor: 'per-vu-iterations',
            startTime: '300s',
            vus: 200,
            iterations: 1
        },
        oneThousandDevices: {
            executor: 'per-vu-iterations',
            startTime: '400s',
            vus: 1000,
            iterations: 1
        },
        threeThousandDevices: {
            executor: 'per-vu-iterations',
            startTime: '500s',
            vus: 3000,
            iterations: 1
        },
        fiveThousandDevices: {
            executor: 'per-vu-iterations',
            startTime: '600s',
            vus: 5000,
            iterations: 1
        },
        tenThousandDevices: {
            executor: 'per-vu-iterations',
            startTime: '700s',
            vus: 10000,
            iterations: 1
        }
    }
};

const triggers = { noAction: "NOOP", changeTemperature: "ChangeTemperature", changeLocation: "ChangeLocation" };

const possibleTriggers = [
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.changeTemperature, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.changeTemperature, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.changeLocation, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction,
    triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction, triggers.noAction
];

const serviceUrl = "http://localhost:5264";

const buildInitialState = (deviceId, scenarioName) => {
    return {
        DevId: `DevFor_${scenarioName}-${deviceId.toString().padStart(10, "0")}`,
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

const shouldItDecrease = () => Math.round(Math.random() * 10) % 2 == 0;

const startRandomTransitioningPhase = withDeviceState => {
    const cycles = 50 + Math.round(Math.random() * 50);
    for(let i = 1; i <= cycles; i++) {
        const possibleTriggerToTake = possibleTriggers[Math.round(Math.random() * possibleTriggers.length)];
        if(possibleTriggerToTake === triggers.changeTemperature) {
            withDeviceState.Temp += Math.random() * 2.0 * (shouldItDecrease() ? -1 : 1);
            sendDeviceState(withDeviceState);
        }
        else if(possibleTriggerToTake === triggers.changeLocation) {
            withDeviceState.Lat += Math.random() * 1.5 * (shouldItDecrease() ? -1 : 1);
            withDeviceState.Lon += Math.random() * 1.5 * (shouldItDecrease() ? -1 : 1);
            sendDeviceState(withDeviceState);
        }
        else if(possibleTriggerToTake === triggers.noAction) {
            sendDeviceState(withDeviceState);
        }
        sleep(1);
    }
}

const runColdStartScenario = (deviceId, scenarioName) => {
    const initialDeviceState = buildInitialState(deviceId, scenarioName);
    sendDeviceState(initialDeviceState);
}

const runOtherScenarios = (deviceId, scenarioName) => {
    const initialDeviceState = buildInitialState(deviceId, scenarioName);
    sendDeviceState(initialDeviceState);
    startRandomTransitioningPhase(initialDeviceState);
}

export default function () {
    if(scenario.name === 'coldStart')
        runColdStartScenario(vu.idInTest, scenario.name);
    else
        runOtherScenarios(vu.idInTest, `${scenario.name}_${new Date(scenario.startTime).toISOString()}`);
}
