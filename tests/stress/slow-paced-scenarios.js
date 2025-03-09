{
	"discardResponseBodies": true,
	
	"scenarios": {
        "coldStart": {
            "executor": "per-vu-iterations",
            "startTime": "0s",
            "maxDuration": "1s",
            "gracefulStop": "1s",
            "vus": 1,
            "iterations": 1
        },
        "oneDevice": {
            "executor": "per-vu-iterations",
            "startTime": "1s",
            "maxDuration": "90s",
            "gracefulStop": "1s",
            "vus": 1,
            "iterations": 1
        },
        "tenDevices": {
            "executor": "per-vu-iterations",
            "startTime": "100s",
            "maxDuration": "90s",
            "gracefulStop": "1s",
            "vus": 10,
            "iterations": 1
        },
        "fiftyDevices": {
            "executor": "per-vu-iterations",
            "startTime": "200s",
            "maxDuration": "90s",
            "gracefulStop": "1s",
            "vus": 50,
            "iterations": 1
        },
        "twoHundredDevices": {
            "executor": "per-vu-iterations",
            "startTime": "300s",
            "maxDuration": "90s",
            "gracefulStop": "1s",
            "vus": 200,
            "iterations": 1
        },
        "oneThousandDevices": {
            "executor": "per-vu-iterations",
            "startTime": "400s",
            "maxDuration": "90s",
            "gracefulStop": "1s",
            "vus": 1000,
            "iterations": 1
        },
        "threeThousandDevices": {
            "executor": "per-vu-iterations",
            "startTime": "500s",
            "maxDuration": "100s",
            "gracefulStop": "1s",
            "vus": 3000,
            "iterations": 1
        },
        "fiveThousandDevices": {
            "executor": "per-vu-iterations",
            "startTime": "610s",
            "maxDuration": "100s",
            "gracefulStop": "1s",
            "vus": 5000,
            "iterations": 1
        },
        "tenThousandDevices": {
            "executor": "per-vu-iterations",
            "startTime": "720s",
            "maxDuration": "300s",
            "gracefulStop": "1s",
            "vus": 10000,
            "iterations": 1
        }
    }
}