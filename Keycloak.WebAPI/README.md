## Running Keycloak with Docker

This project uses **Keycloak** as the identity and access management solution.  
You can run a Keycloak instance locally using Docker with the following command.

### Prerequisites

- Docker installed on your machine
- Docker daemon running

### Run Keycloak Container

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:latest \
  start-dev
```

### Access Keycloak Admin Console

Once the container is running, open your browser and navigate to:

```
http://localhost:8080
```

Login credentials:

- **Username:** admin  
- **Password:** admin  

### Notes

- The `start-dev` option runs Keycloak in development mode.
- For production environments, you should use a proper database, HTTPS, and production-ready configuration.
- You can stop and remove the container using:

```bash
docker stop keycloak
docker rm keycloak
```