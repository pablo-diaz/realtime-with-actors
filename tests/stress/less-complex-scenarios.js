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
        }
    }
}