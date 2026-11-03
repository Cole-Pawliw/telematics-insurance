# Telematics Insurance

A usage-based car insurance demo. Drivers get a monthly **safety score** from how they drive,
and the score sets a **discount or surcharge** on their premium.

- **Policy service** creates insurance policies.
- **Telematics service** records trips (miles, hard brakes, rapid accelerations, top speed).
- **Billing service** turns each trip into a monthly safety score and premium adjustment.
- **Web dashboard** (React) lets you create policies, simulate trips and watch the score change.

The services talk to each other through **RabbitMQ** and store their data in one **Postgres**
database, with a separate schema and login per service.

This is a project to re-familiarize myself with docker, as well as learning Domain-Driven Design practices. Microservices are used to apply DDD in a convenient way.

---

## Run the app

You only need **Docker** (Docker Desktop on Windows/macOS, or Docker Engine with the Compose
plugin on Linux). Everything else is built inside containers.

### 1. Create your settings file

Copy `.env.example` to `.env` in the repository root:

```bash
cp .env.example .env            # macOS / Linux / Git Bash
Copy-Item .env.example .env     # Windows PowerShell
```

Open `.env` and replace every `change-me` with a password of your choice.
Passwords must not contain quotes (`'`) or dollar signs (`$`).

### 2. Start everything

From the repository root:

```bash
docker compose up -d --build
```

The first run takes a few minutes while images are built. Later runs are quick.

### 3. Open the dashboard

Go to **http://localhost:3000**.

| What | Address |
|---|---|
| Dashboard | http://localhost:3000 |
| Policy API (Swagger) | http://localhost:5001/swagger |
| Telematics API (Swagger) | http://localhost:5002/swagger |
| Billing API (Swagger) | http://localhost:5003/swagger |
| RabbitMQ admin page | http://localhost:15672 (log in with `RABBITMQ_USER` / `RABBITMQ_PASSWORD` from `.env`) |
| Postgres | `localhost:5432`, database `telematics_insurance` |

All ports can be changed in `.env`.

### 4. Try it out

1. **Create a policy**: enter a name and a 17-character VIN (e.g. `1HGCR2F83HA000000`), then
   click **Issue policy**. The new policy loads automatically.
2. **Simulate a trip**: set miles, hard brakes, rapid accelerations and max speed, then click
   **Record trip**.
3. About a second later, the **safety score**, **premium adjustment** and **trip history** update.
4. To view a policy later, type its number (e.g. `POL-1A2B3C4D`) in **Find a policy** and press **Load**.

### Stop, restart and reset

```bash
docker compose stop           # stop, keeping the data
docker compose start          # start again
docker compose down -v        # remove everything, including all data
docker compose logs -f        # follow the logs of all services
```

**After changing passwords in `.env`**, run `docker compose down -v` and then
`docker compose up -d`. The database logins are only created when the database is first set
up, so a plain restart keeps the old passwords.

---

## How scoring works

Each policy gets one invoice per calendar month (UTC). A trip counts in the month it **ended**.
The score starts at 100 each month and only goes down:

| Driving event | Points lost |
|---|---|
| Hard brake | 3 |
| Rapid acceleration | 2 |
| Each mph over 80 (top speed) | 0.5 (rounded down per trip) |

| Safety score | Premium adjustment |
|---|---|
| 90 – 100 | −15% |
| 75 – 89 | −5% |
| 60 – 74 | 0% |
| 45 – 59 | +10% |
| below 45 | +25% |

Trips with a top speed over 100 mph are also marked **Flagged for review**.
