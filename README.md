# NSQ Demonstration Project

## Introduction

This project demonstrates the use of [NSQ](https://nsq.io/), a real-time distributed messaging platform, implemented in C#. It includes a Publisher, a Consumer, a NSQ library, and a Docker-based setup for the NSQ system, along with Prometheus and Grafana for monitoring.

## Directory Structure

```
NSQ-Demonstration/
├── NSQ/
│   ├── docker-compose.yml  # Docker configuration for NSQ, Prometheus, Grafana
│   ├── prometheus.yml      # Prometheus configuration for NSQ metrics
│   ├── NSQ.sln             # Main solution file
│   ├── Common/             # Shared library (models, logger, input provider, payload generator)
│   ├── Consumer/           # Consumer application to receive messages from NSQ
│   ├── Publisher/          # Publisher application to send messages to NSQ
│   └── NSQ/                # NSQ client wrapper library (Publisher/Consumer logic)
├── .gitignore
└── README.md
```

## Requirements

*   [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
*   [Docker](https://www.docker.com/)
*   (Optional) [NSQ CLI](https://nsq.io/deployment/installing.html) if you want to run NSQ locally without Docker.

## Setup & Running Instructions

### 1. Start NSQ, Prometheus, and Grafana using Docker

Navigate to the `NSQ` directory and start the services:
```sh
cd NSQ
docker compose up -d
```
This will start the following services:
*   **NSQ Admin:** [http://localhost:4171](http://localhost:4171)
*   **Prometheus:** [http://localhost:9090](http://localhost:9090)
*   **Grafana:** [http://localhost:3000](http://localhost:3000) (default credentials: `admin`/`admin`)

The Docker setup is defined in [NSQ/docker-compose.yml](NSQ/docker-compose.yml) and Prometheus configuration in [NSQ/prometheus.yml](NSQ/prometheus.yml).

### 2. Build the Project

Build the .NET solution:
```sh
dotnet build NSQ/NSQ.sln
```

### 3. Run the Publisher

Navigate to the Publisher project directory and run it:
```sh
cd NSQ/Publisher
dotnet run
```

### 4. Run the Consumer

Navigate to the Consumer project directory and run it:
```sh
cd NSQ/Consumer
dotnet run
```

## Scenarios

The Publisher and Consumer applications include several scenarios to demonstrate different NSQ functionalities. You can switch between scenarios by editing the `Program.cs` file in the respective project ([NSQ/Publisher/Program.cs](NSQ/Publisher/Program.cs) or [NSQ/Consumer/Program.cs](NSQ/Consumer/Program.cs)) and uncommenting the desired scenario.

Available scenarios:
*   **Scenario (Default Demo):** A basic publisher/consumer setup.
*   **Scenario1 ([Publisher](NSQ/Publisher/Scenarios/Scenario1.cs), [Consumer](NSQ/Consumer/Scenarios/Scenario1.cs)):** Demonstrates multiple consumers on the *same channel* for load balancing.
*   **Scenario2 ([Publisher](NSQ/Publisher/Scenarios/Scenario2.cs), [Consumer](NSQ/Consumer/Scenarios/Scenario2.cs)):** Demonstrates multiple consumers on *different channels* for message broadcasting.
*   **Scenario3 ([Publisher](NSQ/Publisher/Scenarios/Scenario3.cs), [Consumer](NSQ/Consumer/Scenarios/Scenario3.cs)):** Demonstrates multiple publishers sending messages to the *same topic*.
*   **Scenario4 ([Publisher](NSQ/Publisher/Scenarios/Scenario4.cs), [Consumer](NSQ/Consumer/Scenarios/Scenario4.cs)):** A performance testing scenario for sending and receiving a large number of messages, with detailed statistics collection and reporting.

## Monitoring

*   NSQ metrics are exposed by the `nsqd` instances and scraped by the `nsq_metrics` service in the Docker setup.
*   Prometheus is configured to collect these metrics (see [NSQ/prometheus.yml](NSQ/prometheus.yml)).
*   Grafana can be used to visualize these metrics. You can create dashboards to monitor message rates, queue depths, topic/channel statistics, latency, etc.

## References

*   [NSQ Documentation](https://nsq.io/)
*   [Prometheus](https://prometheus.io/)
*   [Grafana](https://grafana.com/)

---
