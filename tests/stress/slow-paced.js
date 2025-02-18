// k6 run slow-paced.js

import { sleep } from 'k6';
import http from 'k6/http';
import { vu, scenario } from 'k6/execution';
import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

export const options = {
    discardResponseBodies: true,
	
	scenarios: {
        coldStart: {
            executor: 'per-vu-iterations',
            startTime: '0s',
            maxDuration: '1s',
            gracefulStop: '1s',
            vus: 1,
            iterations: 1
        },
        oneDevice: {
            executor: 'per-vu-iterations',
            startTime: '1s',
            maxDuration: '90s',
            gracefulStop: '1s',
            vus: 1,
            iterations: 1
        },
        tenDevices: {
            executor: 'per-vu-iterations',
            startTime: '100s',
            maxDuration: '90s',
            gracefulStop: '1s',
            vus: 10,
            iterations: 1
        },
        /*fiftyDevices: {
            executor: 'per-vu-iterations',
            startTime: '200s',
            maxDuration: '90s',
            gracefulStop: '1s',
            vus: 50,
            iterations: 1
        },
        twoHundredDevices: {
            executor: 'per-vu-iterations',
            startTime: '300s',
            maxDuration: '90s',
            gracefulStop: '1s',
            vus: 200,
            iterations: 1
        },
        oneThousandDevices: {
            executor: 'per-vu-iterations',
            startTime: '400s',
            maxDuration: '90s',
            gracefulStop: '1s',
            vus: 1000,
            iterations: 1
        },
        threeThousandDevices: {
            executor: 'per-vu-iterations',
            startTime: '500s',
            maxDuration: '100s',
            gracefulStop: '1s',
            vus: 3000,
            iterations: 1
        },
        fiveThousandDevices: {
            executor: 'per-vu-iterations',
            startTime: '610s',
            maxDuration: '100s',
            gracefulStop: '1s',
            vus: 5000,
            iterations: 1
        },
        tenThousandDevices: {
            executor: 'per-vu-iterations',
            //startTime: '2s',
            startTime: '720s',
            maxDuration: '300s',
            gracefulStop: '1s',
            vus: 10000,
            iterations: 1
        }*/
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

const serviceUrl = "http://localhost:81";

const buildInitialState = (deviceId, scenarioName) => {
    return {
        DevId: `DevFor!${scenarioName}@${deviceId.toString().padStart(10, "0")}`,
        Temp: randomIntBetween(10, 30),
        Lat: 51 + Math.round(Math.random() * 10) - Math.round(Math.random() * 10) + Math.random(),
        Lon: 0 + Math.round(Math.random() * 10) - Math.round(Math.random() * 10) + Math.random()
    };
}

const sendDeviceState = (deviceStateToSend, actionPerformed) => {
    const params = {
        headers: {
            'Content-Type': 'application/json',
            'User-Agent': `${actionPerformed} for ${deviceStateToSend.DevId}`
        }
    };

    http.post(`${serviceUrl}/api/devicemetric`, JSON.stringify(deviceStateToSend), params);
}

const shouldItDecrease = () => randomIntBetween(0, 10) % 2 == 0;

const startRandomTransitioningPhase = withDeviceState => {
    const cycles = randomIntBetween(40, 70);
    for(let i = 1; i <= cycles; i++) {
        const possibleTriggerToTake = possibleTriggers[Math.round(Math.random() * possibleTriggers.length)];
        if(possibleTriggerToTake === triggers.changeTemperature) {
            withDeviceState.Temp += Math.random() * 2.0 * (shouldItDecrease() ? -1 : 1);
            sendDeviceState(withDeviceState, `[${i}/${cycles}] Changing temperature`);
        }
        else if(possibleTriggerToTake === triggers.changeLocation) {
            withDeviceState.Lat += Math.random() * 1.5 * (shouldItDecrease() ? -1 : 1);
            withDeviceState.Lon += Math.random() * 1.5 * (shouldItDecrease() ? -1 : 1);
            sendDeviceState(withDeviceState, `[${i}/${cycles}] Changing location`);
        }
        else if(possibleTriggerToTake === triggers.noAction) {
            sendDeviceState(withDeviceState, `[${i}/${cycles}] No action`);
        }
        sleep(0.5 + Math.random());
    }
}

const runColdStartScenario = (deviceId, scenarioName) => {
    const initialDeviceState = buildInitialState(deviceId, scenarioName);
    sendDeviceState(initialDeviceState, 'Initial state');
}

const runOtherScenarios = (deviceId, scenarioName) => {
    const initialDeviceState = buildInitialState(deviceId, scenarioName);
    sendDeviceState(initialDeviceState, 'Initial state');
    startRandomTransitioningPhase(initialDeviceState);
}

export default function () {
    if(scenario.name === 'coldStart')
        runColdStartScenario(vu.idInTest, scenario.name);
    else
        runOtherScenarios(vu.idInTest, `${scenario.name}_${new Date(scenario.startTime).toISOString()}`);
}
