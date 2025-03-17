// k6 run --config stressing-scenarios.js testing-logic.js

import { sleep } from 'k6';
import http from 'k6/http';
import { vu, scenario } from 'k6/execution';
import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

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

const serviceUrl = "http://host.docker.internal:81";

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
